#!/bin/bash

TargetFramework=$1
ProjectName=$2

cp -f "../Client/bin/Debug/$TargetFramework/$ProjectName$.Client.Oqtane.dll" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Client/bin/Debug/$TargetFramework/$ProjectName$.Client.Oqtane.pdb" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Server/bin/Debug/$TargetFramework/$ProjectName$.Server.Oqtane.dll" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Server/bin/Debug/$TargetFramework/$ProjectName$.Server.Oqtane.pdb" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Shared/bin/Debug/$TargetFramework/$ProjectName$.Shared.Oqtane.dll" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Shared/bin/Debug/$TargetFramework/$ProjectName$.Shared.Oqtane.pdb" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"

## Copy module assets but exclude UI framework assets
rsync -av --exclude '_content' "../Server/wwwroot/" "../../../Oqtane.Server/wwwroot/_content/$ProjectName/"

## Copy MudBlazor assets if present 
# if [ -d "../Server/wwwroot/_content/MudBlazor" ]; then 
# rsync -av "../Server/wwwroot/_content/MudBlazor/" "../../../Oqtane.Server/wwwroot/_content/MudBlazor/" 
# fi


