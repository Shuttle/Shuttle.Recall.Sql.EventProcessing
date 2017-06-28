if exists (select SequenceNumber from [dbo].[ProjectionPosition] where [Name] = @Name)
	update [dbo].[ProjectionPosition] set SequenceNumber = @SequenceNumber where [Name] = @Name
else
	insert into [dbo].[ProjectionPosition] (Name, SequenceNumber) values (@Name, @SequenceNumber)
