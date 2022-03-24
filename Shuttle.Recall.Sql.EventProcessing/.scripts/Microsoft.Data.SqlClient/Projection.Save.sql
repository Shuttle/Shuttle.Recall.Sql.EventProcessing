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
		SequenceNumber = @SequenceNumber
	where
		[Name] = @Name
else
	insert into [dbo].[Projection]
	(
		[Name],
		SequenceNumber
	)
	values
	(
		@Name,
		@SequenceNumber
	)