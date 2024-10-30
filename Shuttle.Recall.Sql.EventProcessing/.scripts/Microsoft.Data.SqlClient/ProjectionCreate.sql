IF OBJECT_ID (N'[{schema}].[Projection]', N'U') IS NULL
BEGIN
	CREATE TABLE [{schema}].[Projection](
		[Name] [varchar](650) NOT NULL,
		[SequenceNumber] [bigint] NOT NULL,
	 CONSTRAINT [PK_Projection] PRIMARY KEY NONCLUSTERED 
	(
		[Name] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF OBJECT_ID (N'[{schema}].[DF_Projection_SequenceNumber]', N'D') IS NULL
BEGIN
	ALTER TABLE [{schema}].[Projection] ADD  CONSTRAINT [DF_Projection_SequenceNumber]  DEFAULT ((0)) FOR [SequenceNumber]
END
GO

IF OBJECT_ID (N'[{schema}].[ProjectionPosition]', N'U') IS NOT NULL
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
			[{schema}].[ProjectionPosition]
END

IF OBJECT_ID (N'[{schema}].[ProjectionPosition]', N'U') IS NOT NULL
BEGIN
	DROP TABLE [{schema}].[ProjectionPosition]
END

IF COL_LENGTH(N'[{schema}].[Projection]', 'MachineName') IS NOT NULL
BEGIN
	ALTER TABLE [{schema}].[Projection] DROP COLUMN [MachineName]
END

IF COL_LENGTH(N'[{schema}].[Projection]', 'BaseDirectory') IS NOT NULL
BEGIN
	ALTER TABLE [{schema}].[Projection] DROP COLUMN [BaseDirectory]
END