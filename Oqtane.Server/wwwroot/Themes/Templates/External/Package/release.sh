TargetFramework=$1

"..\..\oqtane.framework\oqtane.package\nuget.exe" pack [Owner].Theme.[Theme].nuspec -Properties targetframework=$TargetFramework
cp -f "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\Packages\"