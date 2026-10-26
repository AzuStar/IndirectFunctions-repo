#!/usr/bin/env bash
set -euo pipefail

taskRoot="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
taskProject="$taskRoot/tests/PackageSmoke/PackageSmoke.csproj"
taskConfig="$taskRoot/tests/PackageSmoke/NuGet.Config"
taskPackage="$taskRoot/artifacts/packages/IndirectFunctions.0.1.0.nupkg"
taskCache="$taskRoot/artifacts/package-smoke-cache"

if [[ ! -f "$taskPackage" ]]; then
  echo "Missing $taskPackage. Pack IndirectFunctions.Package.csproj first." >&2
  exit 1
fi

# Isolated package cache can safely refresh the same local development version after repacking.
rm -rf -- "$taskCache/indirectfunctions/0.1.0"
dotnet restore "$taskProject" --configfile "$taskConfig" --packages "$taskCache" -m:1 -nr:false
dotnet clean "$taskProject" -m:1 -nr:false -v:minimal
dotnet build "$taskProject" --no-restore -m:1 -nr:false
dotnet run --project "$taskProject" --no-build

taskGenerated="$taskRoot/tests/PackageSmoke/obj/Debug/net8.0/indirect-functions"
taskProducerGenerated="$taskRoot/tests/CallbackLibrary/obj/Debug/net8.0/indirect-functions"
rg -q 'CreateRegistered' "$taskGenerated" "$taskProducerGenerated"
taskBefore="$(find "$taskGenerated" "$taskProducerGenerated" -name '*.nindirect.cs' -printf '%p %T@\n' | sort)"
dotnet build "$taskProject" --no-restore -m:1 -nr:false
dotnet run --project "$taskProject" --no-build
taskAfter="$(find "$taskGenerated" "$taskProducerGenerated" -name '*.nindirect.cs' -printf '%p %T@\n' | sort)"
[[ "$taskBefore" == "$taskAfter" ]] || { echo 'Incremental build rewrote unchanged transformed source.' >&2; exit 1; }

taskInvalid="$(mktemp -d "$taskRoot/artifacts/package-invalid.XXXXXX")"
trap 'rm -rf -- "$taskInvalid"' EXIT
cp -- "$taskConfig" "$taskInvalid/NuGet.Config"
cat > "$taskInvalid/Invalid.csproj" <<'PROJECT'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><PackageReference Include="IndirectFunctions" Version="0.1.0" /></ItemGroup>
</Project>
PROJECT
dotnet restore "$taskInvalid/Invalid.csproj" --configfile "$taskInvalid/NuGet.Config" --packages "$taskCache" -m:1 -nr:false

expect_failure() {
  local taskExpected="$1"
  shift
  if dotnet build "$taskInvalid/Invalid.csproj" --no-restore -m:1 -nr:false "$@" > "$taskInvalid/build.log" 2>&1; then
    echo 'Invalid callback source compiled successfully.' >&2
    cat "$taskInvalid/build.log" >&2
    exit 1
  fi
  rg -q "$taskExpected" "$taskInvalid/build.log" || { cat "$taskInvalid/build.log" >&2; exit 1; }
  rg -F -q "$taskInvalid/Source.cs(" "$taskInvalid/build.log" || { cat "$taskInvalid/build.log" >&2; exit 1; }
}

cat > "$taskInvalid/Source.cs" <<'SOURCE'
using IndirectFunctions;
public static class Invalid
{
    public static void Create(NContext context)
    {
        Action callback = () => {};
        _ = NAction.Create(context, "package.invalid.delegate", callback);
    }
}
SOURCE
expect_failure 'error NIF(002|100):'
# Compile untouched markers: same source analyzer used by IDE analysis.
expect_failure 'error NIF002:' -p:IndirectFunctionsEnabled=false

cat > "$taskInvalid/Source.cs" <<'SOURCE'
using IndirectFunctions;
public static class Invalid
{
    public static void Create(NContext context, string key)
    {
        _ = NAction.Create(context, key, static () => {});
    }
}
SOURCE
expect_failure 'error NIF(001|100):'

cat > "$taskInvalid/Source.cs" <<'SOURCE'
using IndirectFunctions;
public static class Invalid
{
    public static void Create(NContext context)
    {
        _ = NAction.Create(context, "\uD800", static () => {});
    }
}
SOURCE
expect_failure 'error NIF001:'

cat > "$taskInvalid/Source.cs" <<'SOURCE'
using IndirectFunctions;
public static class Invalid
{
    public static void Create(NContext context)
    {
        _ = NAction.Create(context, "package.invalid.duplicate", static () => {});
        _ = NAction.Create(context, "package.invalid.duplicate", static () => {});
    }
}
SOURCE
expect_failure 'error NIF(006|100):'
echo 'Package validation: clean/incremental builds, transitive bootstrap, capture rebinding, and five diagnostic checks passed.'
