TargetFramework=$1

"..\..\[RootFolder]\oqtane.package\nuget.exe" pack %ProjectName%.nuspec-Properties targetframework=%TargetFramework%;projectname=%ProjectName%
cp -f "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\Packages\"