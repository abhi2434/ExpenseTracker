CREATE TABLE [dbo].[Comment] (
    [CommentId]              UNIQUEIDENTIFIER NOT NULL,
    [ParentId]               UNIQUEIDENTIFIER NULL,
    [ParentType]             NVARCHAR (200)   NULL,
    [CommentTitle]           NVARCHAR (1000)  NULL,
    [CommentText]            NVARCHAR (MAX)   NULL,
    [IsActive]               BIT              NULL,
    [AutoAudit_CreatedDate]  DATETIME         NULL,
    [AutoAudit_CreatedBy]    NVARCHAR (128)   NULL,
    [AutoAudit_ModifiedDate] DATETIME         NULL,
    [AutoAudit_ModifiedBy]   NVARCHAR (128)   NULL,
    [AutoAudit_RowVersion]   INT              NULL,
    PRIMARY KEY CLUSTERED ([CommentId] ASC)
);

