del "*.nupkg"
"..\..\[RootFolder]\oqtane.package\nuget.exe" pack [Owner].Theme.[Theme].nuspec 
XCOPY "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\wwwroot\Packages\" /Y
