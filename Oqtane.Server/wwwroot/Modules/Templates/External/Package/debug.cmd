@echo off
set TargetFramework=%1

XCOPY "..\Client\bin\Debug\%TargetFramework%\[Owner].Module.[Module].Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Debug\%TargetFramework%\[Owner].Module.[Module].Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\[Owner].Module.[Module].Server.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\[Owner].Module.[Module].Server.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\[Owner].Module.[Module].Shared.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\[Owner].Module.[Module].Shared.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\wwwroot\*" "..\..\[RootFolder]\Oqtane.Server\wwwroot\" /Y /S /I
