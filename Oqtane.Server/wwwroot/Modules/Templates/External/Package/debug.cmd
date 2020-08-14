XCOPY "..\Client\bin\Debug\netstandard2.1\[Owner].[Module].Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Client\bin\Debug\netstandard2.1\[Owner].[Module].Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\[Owner].[Module].Server.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\[Owner].[Module].Server.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\[Owner].[Module].Shared.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\[Owner].[Module].Shared.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\wwwroot\Modules\[Owner].[Module]\*" "..\..\[RootFolder]\Oqtane.Server\wwwroot\Modules\[Owner].[Module]\" /Y /S /I
