[private]
default:
    @just --list

[group('doc')]
doc-build:
    docfx metadata && docfx build

[group('doc')]
doc-serve:
    docfx serve

[group('code-quality')]
format:
    dotnet format --no-restore

[group('testing')]
coverage:
    dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

[group('testing')]
report: coverage
    dotnet tool run reportgenerator -- -filefilters:-*LibraryImports.g.cs -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html
    just report-open

[group('testing')]
report-open:
    {{ if os() == "macos" { "open ./coverage/report/index.html" } else if os() == "linux" { "xdg-open ./coverage/report/index.html" } else { "start ./coverage/report/index.html" } }}

#[group('rustr')]
#seidr-build:
#    cd Seidr && cargo build --release
#
#[group('rustr')]
#seidr-build-debug:
#    cd Seidr && cargo build
#
#[group('rustr')]
#seidr-check:
#    cd Seidr && cargo check
#
#[group('rustr')]
#seidr-clean:
#    cd Seidr && cargo clean
#
#[group('rustr')]
#seidr-format:
#    cd Seidr && cargo fmt
#
#[group('rustr')]
#seidr-lint:
#    cd Seidr && cargo clippy -- -D warnings
