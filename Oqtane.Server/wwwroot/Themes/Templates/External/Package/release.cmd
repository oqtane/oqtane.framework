"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].[Theme].nuspec 
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\wwwroot\Themes\" /Y
