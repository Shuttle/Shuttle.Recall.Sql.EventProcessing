select
	[Name],
	SequenceNumber
from 
	[{schema}].[Projection]
where 
	[Name] = @Name