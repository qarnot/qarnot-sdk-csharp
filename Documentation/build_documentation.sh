#!/bin/bash
set -e

expected_cur_dir=$(readlink -f $(dirname ${BASH_SOURCE[0]}))
cd $expected_cur_dir

pushd ..

pushd QarnotSDK
msbuild -t:restore
msbuild /p:Configuration=Debug QarnotSDK.csproj
popd
popd
if [ ! -d docfx ]; then
    nuget install docfx.console -OutputDirectory docfx -Version 2.49.0
fi
chmod 755 docfx/docfx.console.2.49.0/tools/docfx.exe
mono docfx/docfx.console.2.49.0/tools/docfx.exe metadata docfx.json
mono docfx/docfx.console.2.49.0/tools/docfx.exe build docfx.json

# back in toplevel directory
