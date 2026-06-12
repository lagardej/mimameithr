export PATH := "/snap/bin:" + env_var("PATH")
export DOTNET_ROOT := "/snap/dotnet-sdk-100/current"

[private]
default:
    @just --list

autodoc:
    dotnet run --project Volundr/Autodoc .

coverage:
    dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

format:
    dotnet format --no-restore

report-open:
    {{ if os() == "macos" { "open ./coverage/report/index.html" } else if os() == "linux" { "xdg-open ./coverage/report/index.html" } else { "start ./coverage/report/index.html" } }}

report: coverage
    dotnet tool run reportgenerator -- -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html
    just report-open

[group('seidr')]
seidr-build:
    cd Seidr && cargo build --release

[group('seidr')]
seidr-build-debug:
    cd seidr && cargo build

[group('seidr')]
seidr-check:
    cd seidr && cargo check

[group('seidr')]
seidr-clean:
    cd seidr && cargo clean

[group('seidr')]
seidr-format:
    cd seidr && cargo fmt

[group('seidr')]
seidr-lint:
    cd seidr && cargo clippy -- -D warnings
