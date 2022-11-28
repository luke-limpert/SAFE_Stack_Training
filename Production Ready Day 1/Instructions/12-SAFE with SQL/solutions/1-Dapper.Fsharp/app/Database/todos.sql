CREATE TABLE [dbo].[todos]
(
	[TodoId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Description] NVARCHAR(50) NOT NULL,
    [AssigneeId] UNIQUEIDENTIFIER NULL,
    [Status] NVARCHAR(50) NOT NULL CHECK (Status IN('NotStarted','InProgress','Done')),
    [Weekly] NVARCHAR(10) NULL CHECK ([Weekly] IN('Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday')),
    [Custom] DATETIME2 NULL,
    CONSTRAINT [FK_todos_user] FOREIGN KEY ([AssigneeId]) REFERENCES [users]([userId]),
    CONSTRAINT [CK_todos_RepeatStatus] CHECK (
        (Weekly IS NULL AND Custom IS NOT NULL) OR
        (Weekly IS NOT NULL AND Custom IS NULL) OR
        (Weekly IS NULL AND Custom IS NULL)
    )
)