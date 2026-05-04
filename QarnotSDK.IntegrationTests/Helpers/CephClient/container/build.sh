#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
IMAGE_NAME="ceph-s3-box"
RELEASES=("reef" "tentacle")

for release in "${RELEASES[@]}"; do
    echo "==> Building ${IMAGE_NAME}:${release} ..."
    docker build \
        --build-arg CEPH_RELEASE="${release}" \
        -t "${IMAGE_NAME}:${release}" \
        "${SCRIPT_DIR}"
    echo "==> ${IMAGE_NAME}:${release} done"
done


# NOTE: written by Claude Sonnet
