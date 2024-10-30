if exists
(
	select
		null
	from
		[{schema}].[Projection]
	where
		[Name] = @Name
)
	update
		[{schema}].[Projection]
	set
		SequenceNumber = @SequenceNumber
	where
		[Name] = @Name
else
	insert into [{schema}].[Projection]
	(
		[Name],
		SequenceNumber
	)
	values
	(
		@Name,
		@SequenceNumber
	)