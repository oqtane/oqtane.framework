/*  

Version 1.0.1 Notification migration script

*/

ALTER TABLE [dbo].[Notification] ADD
	[SendOn] [datetime] NULL
GO