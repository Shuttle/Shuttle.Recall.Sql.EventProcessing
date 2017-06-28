SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectionPosition]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ProjectionPosition](
	[Name] [varchar](650) NOT NULL,
	[SequenceNumber] [bigint] NOT NULL,
 CONSTRAINT [PK_ProjectionPosition] PRIMARY KEY NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_ProjectionPosition_SequenceNumber]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ProjectionPosition] ADD  CONSTRAINT [DF_ProjectionPosition_SequenceNumber]  DEFAULT ((0)) FOR [SequenceNumber]
END

GO
