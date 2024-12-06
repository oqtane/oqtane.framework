TargetFramework=$1

"..\..\oqtane.framework\oqtane.package\nuget.exe" pack [Owner].Module.[Module].nuspec -Properties targetframework=$TargetFramework
cp -f "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\Packages\"