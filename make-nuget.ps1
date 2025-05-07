dotnet pack --configuration release

echo "Packed, verify then press enter to push"
pause

dotnet nuget push .\FreneticUtilities\bin\Release\FreneticLLC.FreneticUtilities.1.1.2.nupkg --source https://api.nuget.org/v3/index.json
