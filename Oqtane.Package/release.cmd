DEL "*.nupkg"
dotnet clean -c Release ..\Oqtane.sln 
dotnet build -c Release ..\Oqtane.sln 
nuget.exe pack Oqtane.Framework.nuspec
nuget.exe pack Oqtane.Client.nuspec
nuget.exe pack Oqtane.Server.nuspec
nuget.exe pack Oqtane.Shared.nuspec                  
