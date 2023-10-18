XCOPY "..\Client\bin\Debug\net8.0\[Owner].Module.[Module].Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net8.0\" /Y
XCOPY "..\Client\bin\Debug\net8.0\[Owner].Module.[Module].Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net8.0\" /Y
XCOPY "..\Server\bin\Debug\net8.0\[Owner].Module.[Module].Server.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net8.0\" /Y
XCOPY "..\Server\bin\Debug\net8.0\[Owner].Module.[Module].Server.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net8.0\" /Y
XCOPY "..\Shared\bin\Debug\net8.0\[Owner].Module.[Module].Shared.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net8.0\" /Y
XCOPY "..\Shared\bin\Debug\net8.0\[Owner].Module.[Module].Shared.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net8.0\" /Y
XCOPY "..\Server\wwwroot\*" "..\..\[RootFolder]\Oqtane.Server\wwwroot\" /Y /S /I
