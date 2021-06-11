del "*.nupkg"
dotnet clean -c Release ..\Oqtane.sln 
dotnet build -c Release ..\Oqtane.sln
copy /Y/B/V "..\Oqtane.Server\bin\Release\net5.0\System.Drawing.Common.dll" "..\Oqtane.Server\bin\Release\net5.0\publish\System.Drawing.Common.dll"
nuget.exe pack Oqtane.Client.nuspec
nuget.exe pack Oqtane.Server.nuspec
nuget.exe pack Oqtane.Shared.nuspec                  
nuget.exe pack Oqtane.Framework.nuspec
del /F/Q/S "..\Oqtane.Server\bin\Release\net5.0\publish" > NUL
rmdir /Q/S "..\Oqtane.Server\bin\Release\net5.0\publish"
dotnet publish ..\Oqtane.Server\Oqtane.Server.csproj /p:Configuration=Release
del "..\Oqtane.Server\bin\Release\net5.0\publish\appsettings.json"
ren "..\Oqtane.Server\bin\Release\net5.0\publish\appsettings.release.json" "appsettings.json"
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe ".\install.ps1"
del "..\Oqtane.Server\bin\Release\net5.0\publish\appsettings.json"
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe ".\upgrade.ps1"
dotnet clean -c Release ..\Oqtane.Updater.sln 
dotnet build -c Release ..\Oqtane.Updater.sln
dotnet publish ..\Oqtane.Updater\Oqtane.Updater.csproj /p:Configuration=Release
nuget.exe pack Oqtane.Updater.nuspec
pause 

