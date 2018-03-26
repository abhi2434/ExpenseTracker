CREATE TABLE [dbo].[Users] (
    [UserId]                 UNIQUEIDENTIFIER CONSTRAINT [DF_AppUser_AppUserId] DEFAULT (newid()) NOT NULL,
    [UserFullName]           NVARCHAR (50)    NOT NULL,
    [UserPassword]           NVARCHAR (50)    NOT NULL,
    [UserEmail]              NVARCHAR (100)   NOT NULL,
    [UserContactNumber]      NVARCHAR (50)    NULL,
    [IsActive]               BIT              CONSTRAINT [DF_AppUser_IsActive] DEFAULT ((0)) NULL,
    [ActivationCode]         UNIQUEIDENTIFIER NULL,
    [ForgetPasswordCode]     UNIQUEIDENTIFIER NULL,
    [LAST_ACCESS_IP]         NVARCHAR (50)    NULL,
    [AutoAudit_CreatedDate]  DATETIME         CONSTRAINT [AppUser_AutoAudit_CreatedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_CreatedBy]    NVARCHAR (128)   CONSTRAINT [AppUser_AutoAudit_CreatedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_ModifiedDate] DATETIME         CONSTRAINT [AppUser_AutoAudit_ModifiedDate_df] DEFAULT (getdate()) NULL,
    [AutoAudit_ModifiedBy]   NVARCHAR (128)   CONSTRAINT [AppUser_AutoAudit_ModifiedBy_df] DEFAULT (suser_sname()) NULL,
    [AutoAudit_RowVersion]   INT              CONSTRAINT [AppUser_AutoAudit_RowVersion_df] DEFAULT ((1)) NULL,
    [ProfilePic]             NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_AppUser] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

