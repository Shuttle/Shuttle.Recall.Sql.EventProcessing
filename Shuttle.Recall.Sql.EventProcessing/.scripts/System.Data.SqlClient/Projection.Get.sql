select
	Name,
	SequenceNumber 
from 
	[dbo].[ProjectionPosition] 
where 
	[Name] = @Name