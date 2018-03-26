CREATE TABLE [dbo].[Configuration] (
    [ConfigurationId] UNIQUEIDENTIFIER NOT NULL,
    [ConfigKey]       NVARCHAR (50)    NULL,
    [ConfigVal]       NVARCHAR (MAX)   NULL,
    [ParentId]        UNIQUEIDENTIFIER NULL,
    [ParentEntity]    NVARCHAR (MAX)   NULL,
    [IsActive]        BIT              NULL,
    [CreatedOn]       DATETIME         NULL,
    [LastUpdatedOn]   DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([ConfigurationId] ASC)
);

