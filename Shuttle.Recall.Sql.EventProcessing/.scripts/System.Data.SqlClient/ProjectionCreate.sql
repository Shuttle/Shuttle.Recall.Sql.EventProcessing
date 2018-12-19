

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Projection]') AND type in (N'U'))
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
SET ANSI_PADDING OFF
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_Projection_SequenceNumber]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Projection] ADD  CONSTRAINT [DF_Projection_SequenceNumber]  DEFAULT ((0)) FOR [SequenceNumber]
END

GO
