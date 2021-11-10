"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].[Module].nuspec 
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\Packages\" /Y

