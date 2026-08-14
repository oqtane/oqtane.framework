@echo off
set TargetFramework=%1
set ProjectName=%2

XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Server\bin\Debug\%TargetFramework%\%ProjectName%.Server.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Shared\bin\Debug\%TargetFramework%\%ProjectName%.Shared.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y

cmd /c "ROBOCOPY ..\Server\wwwroot ..\..\[RootFolder]\Oqtane.Server\wwwroot\_content\%ProjectName% /E /XD _content & exit /b 0"
cmd /c "ROBOCOPY ..\Server\wwwroot\_content ..\..\[RootFolder]\Oqtane.Server\wwwroot\_content /E & exit /b 0"