CREATE TABLE [BasicEntities] (
    [Id] int NOT NULL IDENTITY,
    [RecordDate] datetime2 NOT NULL,
    [Active] bit NOT NULL,
    [Description] nvarchar(512) NOT NULL,
    CONSTRAINT [PK_BasicEntities] PRIMARY KEY ([Id])
);