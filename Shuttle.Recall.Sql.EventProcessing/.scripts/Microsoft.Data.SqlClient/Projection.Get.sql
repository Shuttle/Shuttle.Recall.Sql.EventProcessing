select
	[Name],
	SequenceNumber
from 
	[dbo].[Projection]
where 
	[Name] = @Name