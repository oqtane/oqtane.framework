del "*.nupkg"
del "*.zip"
dotnet restore ..\Oqtane.sln
dotnet clean -c Release ..\Oqtane.sln 
dotnet build -c Release ..\Oqtane.sln
nuget.exe pack Oqtane.Client.nuspec
nuget.exe pack Oqtane.Server.nuspec
nuget.exe pack Oqtane.Shared.nuspec                  
nuget.exe pack Oqtane.Framework.nuspec
del /F/Q/S "..\Oqtane.Server\bin\Release\net9.0\publish" > NUL
rmdir /Q/S "..\Oqtane.Server\bin\Release\net9.0\publish"
dotnet publish ..\Oqtane.Server\Oqtane.Server.csproj /p:Configuration=Release
del /F/Q/S "..\Oqtane.Server\bin\Release\net9.0\publish\wwwroot\Content" > NUL
rmdir /Q/S "..\Oqtane.Server\bin\Release\net9.0\publish\wwwroot\Content"
setlocal ENABLEDELAYEDEXPANSION
set retain=Placeholder.txt
for /D %%i in ("..\Oqtane.Server\bin\Release\net9.0\publish\wwwroot\_content\*") do (
set /A found=0
for %%j in (%retain%) do (
if "%%~nxi" == "%%j" set /A found=1
)
if not !found! == 1 rmdir /Q/S "%%i"
)
set retain=Oqtane.Modules.Admin.Login,Oqtane.Modules.HtmlText
for /D %%i in ("..\Oqtane.Server\bin\Release\net9.0\publish\wwwroot\Modules\*") do (
set /A found=0
for %%j in (%retain%) do (
if "%%~nxi" == "%%j" set /A found=1
)
if not !found! == 1 rmdir /Q/S "%%i"
)
set retain=Oqtane.Themes.BlazorTheme,Oqtane.Themes.OqtaneTheme
for /D %%i in ("..\Oqtane.Server\bin\Release\net9.0\publish\wwwroot\Themes\*") do (
set /A found=0
for %%j in (%retain%) do (
if "%%~nxi" == "%%j" set /A found=1
)
if not !found! == 1 rmdir /Q/S "%%i"
)
del "..\Oqtane.Server\bin\Release\net9.0\publish\appsettings.json"
ren "..\Oqtane.Server\bin\Release\net9.0\publish\appsettings.release.json" "appsettings.json"
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe ".\install.ps1"
del "..\Oqtane.Server\bin\Release\net9.0\publish\appsettings.json"
del "..\Oqtane.Server\bin\Release\net9.0\publish\web.config"
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe ".\upgrade.ps1"
dotnet clean -c Release ..\Oqtane.Updater.sln 
dotnet build -c Release ..\Oqtane.Updater.sln
dotnet publish ..\Oqtane.Updater\Oqtane.Updater.csproj /p:Configuration=Release
nuget.exe pack Oqtane.Updater.nuspec
pause 

