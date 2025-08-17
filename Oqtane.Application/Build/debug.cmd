@echo off
set TargetFramework=%1
set ProjectName=%2

XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.dll" "..\AppHost\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.pdb" "..\AppHost\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.dll" "..\AppHost\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.pdb" "..\AppHost\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.dll" "..\AppHost\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.pdb" "..\AppHost\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\wwwroot\*" "..\AppHost\wwwroot\" /Y /S /I