#!/bin/bash

TargetFramework=$1

cp -f "../Client/bin/Debug/$TargetFramework/[Owner].Theme.[Theme].Client.Oqtane.dll" "../../oqtane.framework/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -f "../Client/bin/Debug/$TargetFramework/[Owner].Theme.[Theme].Client.Oqtane.pdb" "../../oqtane.framework/Oqtane.Server/bin/Debug/$TargetFramework/"
cp -rf "../Server/wwwroot/"* "../../oqtane.framework/Oqtane.Server/wwwroot/"