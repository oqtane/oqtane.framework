#!/bin/bash

TargetFramework=$1
ProjectName=$2

cp -f "../Client/bin/Debug/$TargetFramework/$ProjectName$.Client.Oqtane.dll" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Client/bin/Debug/$TargetFramework/$ProjectName$.Client.Oqtane.pdb" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Server/bin/Debug/$TargetFramework/$ProjectName$.Server.Oqtane.dll" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Server/bin/Debug/$TargetFramework/$ProjectName$.Server.Oqtane.pdb" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Shared/bin/Debug/$TargetFramework/$ProjectName$.Shared.Oqtane.dll" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Shared/bin/Debug/$TargetFramework/$ProjectName$.Shared.Oqtane.pdb" "../../[RootFolder]/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -rf "../Server/wwwroot/"* "../../[RootFolder]/Oqtane.Server/wwwroot/_content/%ProjectName%/"
