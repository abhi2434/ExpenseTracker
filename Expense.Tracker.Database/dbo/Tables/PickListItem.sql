CREATE TABLE [dbo].[PickListItem] (
    [PickListItemId] UNIQUEIDENTIFIER NOT NULL,
    [DisplayText]    NVARCHAR (500)   NULL,
    [DisplayCode]    NVARCHAR (50)    NULL,
    [Active]         BIT              NULL,
    [ParentTypeCode] NVARCHAR (100)   NULL,
    PRIMARY KEY CLUSTERED ([PickListItemId] ASC)
);

