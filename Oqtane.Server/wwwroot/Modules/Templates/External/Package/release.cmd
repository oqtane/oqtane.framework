@echo off
set TargetFramework=%1

del "*.nupkg"
"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].Module.[Module].nuspec -Properties targetframework=$(TargetFramework)
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\Packages\" /Y

