Create table Expenses
(
	ExpenseId uniqueidentifier primary key,
	ExpenseTitle nvarchar(200),
	ExpenseDetail nvarchar(max),
	ExpenseTimeStamp datetime,
	ExpenseValue decimal(18,2),
	AttachmentPath nvarchar(max),
	UserId uniqueidentifier,
	AutoAudit_CreatedDate datetime,
	AutoAudit_CreatedBy nvarchar(128),
	AutoAudit_ModifiedDate datetime,
	AutoAudit_ModifiedBy nvarchar(128),
	AutoAudit_RowVersion int)
GO	
	ALTER TABLE [dbo].[Expenses]  WITH CHECK ADD  CONSTRAINT [FK_Expenses_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

--ALTER TABLE [dbo].[Expenses] CHECK CONSTRAINT [FK_Expenses_User]
--GO
