#!/bin/bash
set -e

expected_cur_dir=$(readlink -f $(dirname ${BASH_SOURCE[0]}))
cd $expected_cur_dir
pushd ..
msbuild /p:Configuration=Debug QarnotSDK/QarnotSDK.csproj
if [ ! -d docfx ]; then
    nuget install docfx.console -OutputDirectory Documentation/docfx -Version 2.49.0
fi
chmod 755 Documentation/docfx/docfx.console.2.49.0/tools/docfx.exe
mono Documentation/docfx/docfx.console.2.49.0/tools/docfx.exe build Documentation/docfx.json

popd
# back in toplevel directory
