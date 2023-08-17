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
find / -name dotnet
export DOTNET_ROOT=$((test -e /usr/share/dotnet && echo /usr/share/dotnet) || (test -e /usr/local/bin/dotnet && echo /usr/local/bin))
echo DOTNET_ROOT changed to $DOTNET_ROOT
dotnet tool update -g docfx
docfx=$(find / -name docfx | grep tools/docfx)
$docfx metadata docfx.json
$docfx build docfx.json
chown $(whoami) -R ./_site

# back in toplevel directory
