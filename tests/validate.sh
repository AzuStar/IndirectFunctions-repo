#!/usr/bin/env bash
set -euo pipefail

repo_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
cd "$repo_dir"
snapshot_dir=$(mktemp -d "${TMPDIR:-/tmp}/indirect-validation.XXXXXXXX")
trap 'rm -rf -- "$snapshot_dir"' EXIT

dotnet build IndirectFunctions.slnx -c Release -m:1 -nr:false
dotnet IndirectFunctions.Runtime.Tests/bin/Release/net8.0/IndirectFunctions.Runtime.Tests.dll
dotnet IndirectFunctions.Generators.Tests/bin/Release/net10.0/IndirectFunctions.Generators.Tests.dll
dotnet tests/Integration/bin/Release/net8.0/Integration.dll
dotnet tests/TopLevel/bin/Release/net8.0/TopLevel.dll
dotnet tests/Integration/bin/Release/net8.0/Integration.dll --write-snapshot "$snapshot_dir/captures.json"
dotnet tests/Integration/bin/Release/net8.0/Integration.dll --read-snapshot "$snapshot_dir/captures.json"
dotnet tests/Serialization/bin/Release/net8.0/Serialization.dll
for format in json messagepack protobuf; do
    dotnet tests/Serialization/bin/Release/net8.0/Serialization.dll --write "$format" "$snapshot_dir/$format"
    dotnet tests/Serialization/bin/Release/net8.0/Serialization.dll --read "$format" "$snapshot_dir/$format"
done
bash tests/IdentityRebuild/validate.sh
dotnet pack IndirectFunctions.Package.csproj -c Release --no-build -m:1 -nr:false -o artifacts/packages
bash tests/PackageSmoke/validate.sh

if [[ "${1:-}" == "--aot" ]]; then
    dotnet publish tests/NativeAot/NativeAot.csproj -c Release -r "${2:-linux-x64}" -m:1 -nr:false -o artifacts/aot
    artifacts/aot/NativeAot
    artifacts/aot/NativeAot --write-snapshot "$snapshot_dir/native-captures.json"
    artifacts/aot/NativeAot --read-snapshot "$snapshot_dir/native-captures.json"
fi

if [[ -n "${GODOT_BINARY:-}" ]]; then
    dotnet build tests/GodotSmoke/GodotSmoke.csproj -c Debug -m:1 -nr:false
    XDG_DATA_HOME="$snapshot_dir/godot-data" "$GODOT_BINARY" --headless --path tests/GodotSmoke --script Runner.gd
fi
