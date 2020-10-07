dotnet build -c Release CancelMSM.Generators/CancelMSM.Generators.CLI/CancelMSM.Generators.CLI.csproj
Function generate {
  ./CancelMSM.Generators/CancelMSM.Generators.CLI/bin/Release/netcoreapp3.1/CancelMSM.Generators.CLI.exe $args
 }
generate --outputdir ".." selenium side -f "../Twitter.md"
