IF OBJECT_ID (N'[dbo].[Projection]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[Projection](
		[Name] [varchar](650) NOT NULL,
		[SequenceNumber] [bigint] NOT NULL,
	 CONSTRAINT [PK_Projection] PRIMARY KEY NONCLUSTERED 
	(
		[Name] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF OBJECT_ID (N'[dbo].[DF_Projection_SequenceNumber]', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Projection] ADD  CONSTRAINT [DF_Projection_SequenceNumber]  DEFAULT ((0)) FOR [SequenceNumber]
END
GO

IF OBJECT_ID (N'[dbo].[ProjectionPosition]', N'U') IS NOT NULL
BEGIN
	INSERT INTO Projection
	(
		[Name],
		SequenceNumber
	)
		SELECT
			[Name],
			SequenceNumber
		FROM
			[dbo].[ProjectionPosition]
END

IF OBJECT_ID (N'[dbo].[ProjectionPosition]', N'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[ProjectionPosition]
END

IF COL_LENGTH(N'[dbo].[Projection]', 'MachineName') IS NOT NULL
BEGIN
	ALTER TABLE [dbo].[Projection] DROP COLUMN [MachineName]
END

IF COL_LENGTH(N'[dbo].[Projection]', 'BaseDirectory') IS NOT NULL
BEGIN
	ALTER TABLE [dbo].[Projection] DROP COLUMN [BaseDirectory]
END