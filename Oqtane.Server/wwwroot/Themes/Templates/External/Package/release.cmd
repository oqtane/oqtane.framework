@echo off
set TargetFramework=%1

del "*.nupkg"
"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].Theme.[Theme].nuspec -Properties targetframework=%TargetFramework%
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\wwwroot\Packages\" /Y
