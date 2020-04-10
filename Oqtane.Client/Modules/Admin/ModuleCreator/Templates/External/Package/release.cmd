"..\..\oqtane.framework\oqtane.package\nuget.exe" pack [Owner].[Module]s.Module.nuspec 
XCOPY "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\" /Y
