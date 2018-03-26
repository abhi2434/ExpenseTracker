CREATE TABLE [dbo].[Feature] (
    [FeatureId]              UNIQUEIDENTIFIER CONSTRAINT [DF_Feature_FeatureId] DEFAULT (newid()) NOT NULL,
    [FeatureName]            NVARCHAR (50)    NOT NULL,
    [ParentFeatureId]        UNIQUEIDENTIFIER NULL,
    [Controller]             NVARCHAR (MAX)   NULL,
    [Action]                 NVARCHAR (MAX)   NULL,
    [Description]            NVARCHAR (500)   NULL,
    [Active]                 BIT              NULL,
    [ORDERING]               INT              NULL,
    [IsListing]              BIT              NULL,
    [AutoAudit_CreatedDate]  DATETIME         CONSTRAINT [Feature_AutoAudit_CreatedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_CreatedBy]    NVARCHAR (128)   CONSTRAINT [Feature_AutoAudit_CreatedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_ModifiedDate] DATETIME         CONSTRAINT [Feature_AutoAudit_ModifiedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_ModifiedBy]   NVARCHAR (128)   CONSTRAINT [Feature_AutoAudit_ModifiedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_RowVersion]   INT              CONSTRAINT [Feature_AutoAudit_RowVersion_df] DEFAULT ((1)) NULL,
    [FeatureGroup]           NVARCHAR (200)   NULL,
    [FeatureStyle]           NVARCHAR (200)   NULL,
    [FeatureBadgeStyle]      NVARCHAR (200)   NULL,
    CONSTRAINT [PK_Feature] PRIMARY KEY CLUSTERED ([FeatureId] ASC),
    CONSTRAINT [FK_Feature_Feature] FOREIGN KEY ([ParentFeatureId]) REFERENCES [dbo].[Feature] ([FeatureId])
);

