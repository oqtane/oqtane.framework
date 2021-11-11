XCOPY "..\Client\bin\Debug\net6.0\[Owner].[Theme].Client.Oqtane.dll" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net6.0\" /Y
XCOPY "..\Client\bin\Debug\net6.0\[Owner].[Theme].Client.Oqtane.pdb" "..\..\[RootFolder]\Oqtane.Server\bin\Debug\net6.0\" /Y
XCOPY "..\Client\wwwroot\*" "..\..\[RootFolder]\Oqtane.Server\wwwroot\" /Y /S /I
