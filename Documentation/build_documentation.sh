#!/bin/bash
set -e

docfx_version="2.58.9"

expected_cur_dir=$(readlink -f $(dirname ${BASH_SOURCE[0]}))
cd $expected_cur_dir

pushd ..

pushd QarnotSDK
msbuild -t:restore
msbuild /p:Configuration=Debug QarnotSDK.csproj
popd
popd
if [ ! -d docfx ]; then
    nuget install docfx.console -OutputDirectory docfx -Version $docfx_version
fi
chmod 755 docfx/docfx.console.$docfx_version/tools/docfx.exe
mono docfx/docfx.console.$docfx_version/tools/docfx.exe metadata docfx.json
mono docfx/docfx.console.$docfx_version/tools/docfx.exe build docfx.json

# back in toplevel directory
