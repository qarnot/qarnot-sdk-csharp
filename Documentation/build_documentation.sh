#!/bin/bash
set -e

docfx_version="2.58.9"

expected_cur_dir=$(readlink -f $(dirname ${BASH_SOURCE[0]}))
cd $expected_cur_dir

pushd ..

pushd QarnotSDK
dotnet restore
dotnet build -c Debug QarnotSDK.csproj
popd
popd
dotnet tool update -g docfx
docfx=$(find / -name docfx | grep tools/docfx)
$docfx metadata docfx.json
$docfx build docfx.json
chown $(whoami) -R ./_site

# back in toplevel directory
