#!/bin/bash
dotnet clean
dotnet sonarscanner begin /k:"avalanchedotnet" /d:sonar.host.url="http://localhost:9000" \
 /d:sonar.login="sqp_9b360c6067c591cbd59a914609a8ad1924e6c613" \
 /d:sonar.cs.vscoveragexml.reportsPaths=.sonarqube/test-coverage.xml
dotnet build
dotnet-coverage collect "dotnet test" -f xml -o ".sonarqube/test-coverage.xml"
dotnet sonarscanner end /d:sonar.login="sqp_9b360c6067c591cbd59a914609a8ad1924e6c613"