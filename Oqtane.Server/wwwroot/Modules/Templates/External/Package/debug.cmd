XCOPY "..\Client\bin\Debug\netstandard2.1\[Owner].[Module]s.Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Client\bin\Debug\netstandard2.1\[Owner].[Module]s.Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\[Owner].[Module]s.Server.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\[Owner].[Module]s.Server.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\[Owner].[Module]s.Shared.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\[Owner].[Module]s.Shared.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\wwwroot\Modules\[Owner].[Module]s\*" "..\..\[RootFolder]\Oqtane.Server\wwwroot\Modules\[Owner].[Module]s\" /Y /S /I
