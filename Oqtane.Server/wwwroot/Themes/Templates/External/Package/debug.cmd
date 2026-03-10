@echo off
set TargetFramework=%1
set ProjectName=%2

XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y
XCOPY "..\Client\bin\Debug\%TargetFramework%\%ProjectName%.Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\%TargetFramework%\" /Y

REM Copy module assets (exclude Third-party assets)
ROBOCOPY "..\Server\wwwroot" "..\..\..\..\Oqtane.Server\wwwroot\_content\%ProjectName%" /E /XD _content

REM Copy UI framework assets (example: MudBlazor)
@REM IF EXIST "..\Server\wwwroot\_content\MudBlazor" (
@REM     ROBOCOPY "..\Server\wwwroot\_content\MudBlazor" "..\..\..\..\Oqtane.Server\wwwroot\_content\MudBlazor" /E
@REM )
