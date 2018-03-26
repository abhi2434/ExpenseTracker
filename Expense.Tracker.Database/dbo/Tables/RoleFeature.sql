CREATE TABLE [dbo].[RoleFeature] (
    [RoleFeatureId]          UNIQUEIDENTIFIER CONSTRAINT [DF_RoleFeature_RoleFeatureId] DEFAULT (newid()) NOT NULL,
    [RoleId]                 UNIQUEIDENTIFIER NOT NULL,
    [FeatureId]              UNIQUEIDENTIFIER NOT NULL,
    [AutoAudit_CreatedDate]  DATETIME         CONSTRAINT [RoleFeature_AutoAudit_CreatedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_CreatedBy]    NVARCHAR (128)   CONSTRAINT [RoleFeature_AutoAudit_CreatedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_ModifiedDate] DATETIME         CONSTRAINT [RoleFeature_AutoAudit_ModifiedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_ModifiedBy]   NVARCHAR (128)   CONSTRAINT [RoleFeature_AutoAudit_ModifiedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_RowVersion]   INT              CONSTRAINT [RoleFeature_AutoAudit_RowVersion_df] DEFAULT ((1)) NULL,
    CONSTRAINT [PK_RoleFeature] PRIMARY KEY CLUSTERED ([RoleFeatureId] ASC),
    CONSTRAINT [FK_RoleFeature_Feature] FOREIGN KEY ([FeatureId]) REFERENCES [dbo].[Feature] ([FeatureId]),
    CONSTRAINT [FK_RoleFeature_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role] ([RoleId])
);

