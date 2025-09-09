@echo off
set TargetFramework=%1
set ProjectName=%2

del "*.nupkg"
"..\..\[RootFolder]\oqtane.package\FixProps.exe"
"..\..\[RootFolder]\oqtane.package\nuget.exe" pack %ProjectName%.nuspec -Properties targetframework=%TargetFramework%;projectname=%ProjectName%
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\Packages\" /Y