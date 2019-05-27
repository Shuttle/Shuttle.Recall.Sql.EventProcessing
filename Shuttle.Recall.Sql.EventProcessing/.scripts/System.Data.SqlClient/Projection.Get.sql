select
	[Name],
	SequenceNumber,
	MachineName,
	BaseDirectory
from 
	[dbo].[Projection]
where 
	[Name] = @Name