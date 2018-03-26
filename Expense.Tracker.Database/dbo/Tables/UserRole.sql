CREATE TABLE [dbo].[UserRole] (
    [UserRoleId]             UNIQUEIDENTIFIER CONSTRAINT [DF_UserRole_UserRoleId] DEFAULT (newid()) NOT NULL,
    [UserId]                 UNIQUEIDENTIFIER NOT NULL,
    [RoleId]                 UNIQUEIDENTIFIER NOT NULL,
    [AutoAudit_CreatedDate]  DATETIME         CONSTRAINT [OrgRole_AutoAudit_CreatedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_CreatedBy]    NVARCHAR (128)   CONSTRAINT [OrgRole_AutoAudit_CreatedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_ModifiedDate] DATETIME         CONSTRAINT [OrgRole_AutoAudit_ModifiedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_ModifiedBy]   NVARCHAR (128)   CONSTRAINT [OrgRole_AutoAudit_ModifiedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_RowVersion]   INT              CONSTRAINT [OrgRole_AutoAudit_RowVersion_df] DEFAULT ((1)) NULL,
    CONSTRAINT [PK_OrgRole] PRIMARY KEY CLUSTERED ([UserRoleId] ASC),
    CONSTRAINT [FK_UserRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role] ([RoleId]),
    CONSTRAINT [FK_UserRole_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
);

