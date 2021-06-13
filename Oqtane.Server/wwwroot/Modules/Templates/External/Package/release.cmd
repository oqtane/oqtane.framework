"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].[Module].nuspec 
XCOPY "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\Packages\" /Y

