#!/usr/bin/env bash

set -eux
set -o pipefail

ZONE="$(hostname -s | grep -oP '^[a-z]+[0-9]+' || echo 'ceph1')"
ZONE_GROUP="$(hostname -d | grep -oP '^[a-z0-9]+' || echo 'default')"
REALM="$(hostname -d | grep -oP '^[a-z0-9]+' || echo 'default')"
DOMAIN="$(hostname -d || echo 'localhost')"

##
# create ceph.conf
##
echo "create ceph.conf"

cat <<- EOF > /etc/ceph/ceph.conf
[global]
fsid = $(uuidgen)
mon_host = $(hostname -i)
auth_allow_insecure_global_id_reclaim = false
mon_warn_on_pool_no_redundancy = false
mon_osd_down_out_interval = 60
mon_osd_report_timeout = 300
mon_osd_down_out_subtree_limit = host
mon_osd_reporter_subtree_level = rack
osd_scrub_auto_repair = true
osd_pool_default_size = 1
osd_pool_default_min_size = 1
osd_pool_default_pg_num = 1
osd_crush_chooseleaf_type = 0
osd_objectstore = memstore
EOF

##
# create mon
##
echo "create ceph mon"

ceph-authtool \
    --create-keyring /tmp/ceph.mon.keyring \
    --gen-key -n mon. \
    --cap mon 'allow *'
ceph-authtool \
    --create-keyring /etc/ceph/ceph.client.admin.keyring \
    --gen-key -n client.admin \
    --cap mon 'allow *' \
    --cap osd 'allow *' \
    --cap mds 'allow *' \
    --cap mgr 'allow *'
ceph-authtool /tmp/ceph.mon.keyring \
    --import-keyring /etc/ceph/ceph.client.admin.keyring

# Derive the min-mon-release from the installed Ceph major version so the
# monmap is always consistent with what the binary actually supports.
CEPH_MAJOR=$(ceph --version | awk '{print $3}' | cut -d. -f1)
case "${CEPH_MAJOR}" in
    16) MON_MIN_RELEASE=pacific  ;;
    17) MON_MIN_RELEASE=quincy   ;;
    18) MON_MIN_RELEASE=reef     ;;
    19) MON_MIN_RELEASE=squid    ;;
    20) MON_MIN_RELEASE=tentacle ;;
    *)  MON_MIN_RELEASE=reef     ;;
esac
echo "Ceph ${CEPH_MAJOR}.x detected, setting min-mon-release=${MON_MIN_RELEASE}"

monmaptool \
    --create \
    --add "$(hostname -s)" "$(hostname -i)" \
    --fsid "$(grep -oP '(?<=^fsid = )[0-9a-z-]*' /etc/ceph/ceph.conf)" \
    --set-min-mon-release "${MON_MIN_RELEASE}" \
    --clobber \
    /tmp/monmap

mkdir -p "/var/lib/ceph/mon/ceph-$(hostname -s)"
rm -rf "/var/lib/ceph/mon/ceph-$(hostname -s)/*"
ceph-mon --mkfs -i "$(hostname -s)" --monmap /tmp/monmap --keyring /tmp/ceph.mon.keyring
chown -R ceph:ceph /var/lib/ceph/mon/
ceph-mon --cluster ceph --id "$(hostname -s)" --setuser ceph --setgroup ceph

# Wait for the monitor to reach quorum, with a crash check on each iteration
# so we surface the real error instead of hanging indefinitely.
MON_LOG_GLOB="/var/log/ceph/ceph-mon.$(hostname -s).log"
for i in $(seq 1 120); do
    if timeout 3 ceph mon stat > /dev/null 2>&1; then
        echo "Monitor ready after ${i}s"
        break
    fi
    if ! pgrep -x ceph-mon > /dev/null; then
        echo "ERROR: ceph-mon process died. Monitor log:"
        cat ${MON_LOG_GLOB} 2>/dev/null || echo "(no monitor log found)"
        exit 1
    fi
    if [ "${i}" -eq 120 ]; then
        echo "ERROR: Monitor did not reach quorum after 120s. Monitor log:"
        cat ${MON_LOG_GLOB} 2>/dev/null || echo "(no monitor log found)"
        exit 1
    fi
    sleep 1
done

ceph config set global auth_allow_insecure_global_id_reclaim false

##
# create mgr
##
echo "create ceph mgr"

mkdir -p "/var/lib/ceph/mgr/ceph-$(hostname -s)"
ceph auth get-or-create "mgr.$(hostname -s)" mon 'allow profile mgr' osd 'allow *' mds 'allow *' \
    > "/var/lib/ceph/mgr/ceph-$(hostname -s)/keyring"
chown -R ceph:ceph /var/lib/ceph/mgr/
ceph-mgr --cluster ceph --id "$(hostname -s)" --setuser ceph --setgroup ceph

##
# create osd
##
OSD=$(ceph osd create)
echo "create ceph osd.${OSD}"

mkdir -p "/osd/osd.${OSD}/data"
ceph auth get-or-create "osd.${OSD}" mon 'allow profile osd' mgr 'allow profile osd' osd 'allow *' \
    > "/osd/osd.${OSD}/data/keyring"
ceph-osd -i "${OSD}" --mkfs --osd-data "/osd/osd.${OSD}/data"
chown -R ceph:ceph "/osd/osd.${OSD}/data"
ceph-osd -i "${OSD}" --osd-data "/osd/osd.${OSD}/data" --keyring "/osd/osd.${OSD}/data/keyring"

##
# create rgw
##
echo "create ceph rgw"

mkdir -p "/var/lib/ceph/radosgw/ceph-rgw.$(hostname -s)"
ceph auth get-or-create "client.rgw.$(hostname -s)" osd 'allow rwx' mon 'allow rw' \
    -o "/var/lib/ceph/radosgw/ceph-rgw.$(hostname -s)/keyring"
touch "/var/lib/ceph/radosgw/ceph-rgw.$(hostname -s)/done"
chown -R ceph:ceph /var/lib/ceph/radosgw

if [ "${MAIN}" == "none" ]; then
    echo "create admin-user"
    radosgw-admin user create \
        --uid=".admin" \
        --display-name="admin" \
        --system \
        --key-type="s3" \
        --access-key="${ACCESS_KEY}" \
        --secret-key="${SECRET_KEY}"

    ceph config set global rgw_enable_usage_log true
    ceph config set global rgw_dns_name "$(hostname -s)"

    radosgw --cluster ceph --rgw-zone "default" --name "client.rgw.$(hostname -s)" --setuser ceph --setgroup ceph
fi

if [ "${MAIN}" == "yes" ]; then
    echo "create realm ${REALM}"
    radosgw-admin realm create \
        --rgw-realm="${REALM}" \
        --default

    echo "create zonegroup ${ZONE_GROUP}"
    radosgw-admin zonegroup create \
        --rgw-realm="${REALM}" \
        --rgw-zonegroup="${ZONE_GROUP}" \
        --endpoints="http://${DOMAIN}:7480" \
        --master \
        --default

    radosgw-admin zonegroup get --rgw-zonegroup="${ZONE_GROUP}" | \
        jq \
            --arg domain "${DOMAIN}" \
            --arg zone1 "dev1-${DOMAIN}" \
            --arg zone2 "dev2-${DOMAIN}" \
            '.hostnames |= [$domain, $zone1, $zone2]' | \
        radosgw-admin zonegroup set --rgw-zonegroup="${ZONE_GROUP}" -i -

    echo "create zone ${ZONE}"
    radosgw-admin zone create \
        --rgw-zonegroup="${ZONE_GROUP}" \
        --rgw-zone="${ZONE}" \
        --endpoints="http://${ZONE}-${DOMAIN}:7480" \
        --master \
        --default

    echo "create placement PREMIUM"
    radosgw-admin zonegroup placement add \
        --rgw-zonegroup="${ZONE_GROUP}" \
        --placement-id="default-placement" \
        --storage-class="PREMIUM"

    echo "create placement ARCHIVE"
    radosgw-admin zonegroup placement add \
        --rgw-zonegroup="${ZONE_GROUP}" \
        --placement-id="default-placement" \
        --storage-class="ARCHIVE"

    echo "create synchronization-user"
    radosgw-admin user create \
        --uid=".synchronization" \
        --display-name="synchronization-user" \
        --system \
        --key-type="s3" \
        --access-key="${ACCESS_KEY}" \
        --secret-key="${SECRET_KEY}"

    echo "add synchronization-user to zone ${ZONE}"
    radosgw-admin zone modify \
        --rgw-zone="${ZONE}" \
        --access-key="${ACCESS_KEY}" \
        --secret-key="${SECRET_KEY}"

    radosgw-admin sync group create \
        --group-id=group-main \
        --status=allowed
    radosgw-admin sync group flow create \
        --group-id=group-main \
        --flow-id=flow-main \
        --flow-type=symmetrical \
        --zones=dev1,dev2
    radosgw-admin sync group pipe create \
        --group-id=group-main \
        --pipe-id=pipe-main \
        --source-zones='*' \
        --source-bucket='*' \
        --dest-zones='*' \
        --dest-bucket='*'

    echo "create objstorage-admin user"
    radosgw-admin user create \
        --uid=".objstorage-admin" \
        --display-name=".objstorage-admin" \
        --system \
        --admin

    radosgw-admin period update \
        --commit
fi

if [ "${MAIN}" == "no" ]; then
    echo "get realm http://${DOMAIN}:7480"
    while ! radosgw-admin realm pull \
        --url="http://${DOMAIN}:7480" \
        --access-key="${ACCESS_KEY}" \
        --secret="${SECRET_KEY}"; do sleep 0.5; done

    echo "set default realm to ${ZONE_GROUP}"
    radosgw-admin realm default \
        --rgw-realm="${ZONE_GROUP}"

    echo "create zone ${ZONE}"
    radosgw-admin zone create \
        --rgw-zonegroup="${ZONE_GROUP}" \
        --rgw-zone="${ZONE}" \
        --access-key="${ACCESS_KEY}" \
        --secret-key="${SECRET_KEY}" \
        --endpoints="http://${ZONE}-${DOMAIN}:7480" \
        --default
fi

if [ "${MAIN}" == "yes" ] || [ "${MAIN}" == "no" ]; then
    echo "create placement PREMIUM for ${ZONE}"
    radosgw-admin zone placement add \
        --rgw-zone="${ZONE}" \
        --placement-id="default-placement" \
        --storage-class="PREMIUM" \
        --data-pool "${ZONE}.rgw.buckets.premium.data"

    echo "create placement STANDARD for ${ZONE}"
    radosgw-admin zone placement add \
        --rgw-zone="${ZONE}" \
        --placement-id="default-placement" \
        --storage-class="STANDARD" \
        --data-pool "${ZONE}.rgw.buckets.standard.data"

    echo "create placement ARCHIVE for ${ZONE}"
    radosgw-admin zone placement add \
        --rgw-zone="${ZONE}" \
        --placement-id="default-placement" \
        --storage-class="ARCHIVE" \
        --data-pool "${ZONE}.rgw.buckets.archive.data" \
        --compression lz4

    radosgw-admin period update --commit

    ceph config set global rgw_enable_usage_log true
    radosgw --cluster ceph --rgw-zone "${ZONE}" --name "client.rgw.$(hostname -s)" --setuser ceph --setgroup ceph
fi

echo "ceph-s3-box ready"

##
# log output in foreground
##
while ! tail -F /var/log/ceph/ceph* ; do
  sleep 0.1
done

echo "Container terminated ..."

# NOTE: written by Claude Sonnet
