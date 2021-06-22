USE [sercompras]
GO

/****** Object:  Table [dbo].[documents]    Script Date: 6/20/2021 2:20:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[documents](
	[id] [int] IDENTITY (1,1) NOT NULL,
	[id_provider] [int] NULL,
	[name] [nvarchar](50) NULL,
	[path] [nvarchar](255) NULL,
	[size] [int] NULL,
	[content] [nvarchar](255) NULL,
	[created_at] [datetime] NULL,
	[updated_at] [datetime] NULL,
	[status] [tinyint] NULL,
 CONSTRAINT [PK_documents] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


