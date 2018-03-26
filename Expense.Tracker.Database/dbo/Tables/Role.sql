CREATE TABLE [dbo].[Role] (
    [RoleId]                 UNIQUEIDENTIFIER CONSTRAINT [DF_Role_RoleId] DEFAULT (newid()) NOT NULL,
    [RoleName]               NVARCHAR (50)    NOT NULL,
    [IsEditAllowed]          BIT              NOT NULL,
    [AutoAudit_CreatedDate]  DATETIME         CONSTRAINT [Role_AutoAudit_CreatedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_CreatedBy]    NVARCHAR (128)   CONSTRAINT [Role_AutoAudit_CreatedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_ModifiedDate] DATETIME         CONSTRAINT [Role_AutoAudit_ModifiedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_ModifiedBy]   NVARCHAR (128)   CONSTRAINT [Role_AutoAudit_ModifiedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_RowVersion]   INT              CONSTRAINT [Role_AutoAudit_RowVersion_df] DEFAULT ((1)) NULL,
    [Description]            NVARCHAR (1000)  NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([RoleId] ASC)
);

