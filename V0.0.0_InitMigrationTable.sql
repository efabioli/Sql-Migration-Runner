IF (NOT EXISTS (SELECT * 
				FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' 
                AND  TABLE_NAME = 'Migration'))
BEGIN
	CREATE TABLE [dbo].[Migration](
		[Id] [nvarchar](50) NOT NULL,
		[Name] [nvarchar](200) NOT NULL,
		[Version] [nvarchar](max) NOT NULL,
		[Timestamp] [datetime] NOT NULL,
	 CONSTRAINT [PK_Migration] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE [dbo].[Migration] ADD  CONSTRAINT [DF_Migration_Id]  DEFAULT (newid()) FOR [Id]

	ALTER TABLE [dbo].[Migration] ADD  CONSTRAINT [DF_Migration_Timestamp]  DEFAULT (getdate()) FOR [Timestamp]

END