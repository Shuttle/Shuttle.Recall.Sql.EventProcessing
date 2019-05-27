if exists
(
	select
		null
	from
		[dbo].[Projection]
	where
		[Name] = @Name
)
	update
		[dbo].[Projection]
	set
		SequenceNumber = @SequenceNumber,
		MachineName = @MachineName,
		BaseDirectory = @BaseDirectory
	where
		[Name] = @Name
else
	insert into [dbo].[Projection]
	(
		[Name],
		SequenceNumber,
		MachineName,
		BaseDirectory
	)
	values
	(
		@Name,
		@SequenceNumber,
		@MachineName,
		@BaseDirectory
	)