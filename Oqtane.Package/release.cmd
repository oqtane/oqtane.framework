del "*.nupkg"
dotnet clean -c Release ..\Oqtane.sln 
dotnet build -c Release ..\Oqtane.sln 
dotnet publish ..\Oqtane.Server\Oqtane.Server.csproj /p:Configuration=Release
nuget.exe pack Oqtane.Framework.nuspec
nuget.exe pack Oqtane.Client.nuspec
nuget.exe pack Oqtane.Server.nuspec
nuget.exe pack Oqtane.Shared.nuspec                  
