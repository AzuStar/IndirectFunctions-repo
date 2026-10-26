#!/usr/bin/env bash
set -euo pipefail

identity_templates=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
identity_repo=$(cd -- "$identity_templates/../.." && pwd)
for identity_required in \
  "$identity_repo/IndirectFunctions/bin/Release/net8.0/IndirectFunctions.dll" \
  "$identity_repo/IndirectFunctions.Generators/bin/Release/netstandard2.0/IndirectFunctions.Generators.dll" \
  "$identity_repo/IndirectFunctions.Build/bin/Release/net10.0/IndirectFunctions.Build.dll"; do
  if [[ ! -f "$identity_required" ]]; then
    printf 'Required Release artifact missing: %s\nBuild runtime, generator, and tool projects first.\n' "$identity_required" >&2
    exit 1
  fi
done

identity_work=$(mktemp -d /tmp/indirect-identity-rebuild.XXXXXX)
trap 'rm -rf -- "$identity_work"' EXIT
mkdir -p "$identity_work/first" "$identity_work/second" "$identity_work/persisted"
for identity_build in first second; do
  cp -- "$identity_templates/Consumer.csproj.in" "$identity_work/$identity_build/IdentityRebuild.csproj"
  cp -- "$identity_templates/Program.cs.in" "$identity_work/$identity_build/Program.cs"
done
cp -- "$identity_templates/Callbacks.First.cs.in" "$identity_work/first/OriginalCallbacks.cs"
cp -- "$identity_templates/Callbacks.Second.cs.in" "$identity_work/second/RenamedCallbacks.cs"

# Snapshot-built references only: these builds never rebuild or edit shared repository projects.
dotnet build "$identity_work/first/IdentityRebuild.csproj" -c Release --nologo -m:1 /nr:false "-p:IndirectFunctionsRepoPath=$identity_repo"
dotnet "$identity_work/first/bin/Release/net8.0/IdentityRebuildConsumer.dll" save "$identity_work/persisted"
dotnet build "$identity_work/second/IdentityRebuild.csproj" -c Release --nologo -m:1 /nr:false "-p:IndirectFunctionsRepoPath=$identity_repo"
dotnet "$identity_work/second/bin/Release/net8.0/IdentityRebuildConsumer.dll" restore "$identity_work/persisted"
