CREATE TABLE [dbo].[ETLControl] (
    [Id]        BIGINT        NOT NULL,
    [EtlName]   VARCHAR (50)  NULL,
    [EtlId]     VARCHAR (50)  NULL,
    [StartRun]  DATETIME2 (7) NULL,
    [FinishRun] DATETIME2 (7) NULL,
    [Status]    VARCHAR (50)  NULL,
    CONSTRAINT [PK_ETLControl] PRIMARY KEY CLUSTERED ([Id] ASC)
);

