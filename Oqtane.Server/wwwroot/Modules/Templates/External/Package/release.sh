TargetFramework=$1
ProjectName=$2

"..\..\[RootFolder]\oqtane.package\nuget.exe" pack %ProjectName%.nuspec -Properties targetframework=%TargetFramework%;projectname=%ProjectName%
cp -f "*.nupkg" "..\..\[RootFolder]\Oqtane.Server\Packages\"