ARG DEBIAN_FRONTEND=noninteractive
# Supported values: quincy, reef, squid, tentacle (and any future codename)
ARG CEPH_RELEASE=reef

FROM ubuntu:22.04 AS ceph
ENV TZ=Etc/UTC
ARG CEPH_RELEASE

RUN apt -y update && apt -y install \
    lsb-release \
    wget \
    curl \
    pgp \
    tzdata \
    vim \
    dnsutils \
    iputils-ping \
    iproute2 \
    jq

RUN wget \
        -q \
        -O- https://download.ceph.com/keys/release.asc | \
        gpg --dearmor > /etc/apt/trusted.gpg.d/ceph.gpg && \
    echo "deb https://download.ceph.com/debian-${CEPH_RELEASE}/ $(lsb_release -sc) main" \
        > /etc/apt/sources.list.d/ceph.list && \
    apt -y update && \
    apt install -y ceph radosgw

RUN apt clean && \
    apt autoremove -y && \
    rm -rf /var/lib/{apt,dpkg,cache,log}/

# Don't keep intermediate layers for the end image
FROM ceph AS radosgw
ENV TZ=Etc/UTC
ENV ACCESS_KEY="radosgwadmin"
ENV SECRET_KEY="radosgwadmin"
ENV MAIN="none"
ARG CEPH_RELEASE
ENV CEPH_RELEASE="${CEPH_RELEASE}"
LABEL ceph.release="${CEPH_RELEASE}"

EXPOSE 7480

COPY ./entrypoint.sh /entrypoint
ENTRYPOINT /entrypoint

# NOTE: partly written by Claude Sonnet
