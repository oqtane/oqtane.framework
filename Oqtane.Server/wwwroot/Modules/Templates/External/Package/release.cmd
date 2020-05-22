"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].[Module]s.nuspec 
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\wwwroot\Modules\" /Y
