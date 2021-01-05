XCOPY "..\Client\bin\Debug\net5.0\[Owner].[Module].Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Client\bin\Debug\net5.0\[Owner].[Module].Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Server\bin\Debug\net5.0\[Owner].[Module].Server.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Server\bin\Debug\net5.0\[Owner].[Module].Server.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Shared\bin\Debug\net5.0\[Owner].[Module].Shared.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Shared\bin\Debug\net5.0\[Owner].[Module].Shared.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Server\wwwroot\*" "..\..\[RootFolder]\Oqtane.Server\wwwroot\" /Y /S /I
