"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].[Module]s.Module.nuspec 
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\wwwroot\Modules\" /Y
