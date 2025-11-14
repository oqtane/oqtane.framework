dotnet build -c Release ..\Oqtane.slnx
FixProps.exe
nuget.exe pack Oqtane.Client.nuspec
nuget.exe pack Oqtane.Server.nuspec
nuget.exe pack Oqtane.Shared.nuspec                  
nuget.exe pack Oqtane.Framework.nuspec
dotnet publish ..\Oqtane.Server\Oqtane.Server.csproj /p:Configuration=Release
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\Content"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\wwwroot\Content"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\android-arm"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\android-arm64"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\android-x64"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\android-x86"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\ios-arm"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\ios-arm64"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\iossimulator-arm64"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\iossimulator-x64"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\runtimes\iossimulator-x86"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\wwwroot\Modules\Templates"
rmdir /Q /S "..\Oqtane.Server\bin\Release\net10.0\publish\wwwroot\Themes\Templates"
del "..\Oqtane.Server\bin\Release\net10.0\publish\appsettings.json"
ren "..\Oqtane.Server\bin\Release\net10.0\publish\appsettings.release.json" "appsettings.json"
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe ".\install.ps1"
del "..\Oqtane.Server\bin\Release\net10.0\publish\appsettings.json"
del "..\Oqtane.Server\bin\Release\net10.0\publish\web.config"
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe ".\upgrade.ps1"
dotnet build -c Release ..\Oqtane.Updater.slnx
dotnet publish ..\Oqtane.Updater\Oqtane.Updater.csproj /p:Configuration=Release
nuget.exe pack Oqtane.Updater.nuspec
nuget.exe pack ..\Oqtane.Application\Oqtane.Application.Template.nuspec -NoDefaultExcludes
pause 

