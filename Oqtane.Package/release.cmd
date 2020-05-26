DEL "*.nupkg"
dotnet clean -c Release ..\Oqtane.sln 
dotnet build -c Release ..\Oqtane.sln 
dotnet pack  -o .\ -c Release ..\Oqtane.sln 
nuget.exe pack Oqtane.Framework.nuspec                    
