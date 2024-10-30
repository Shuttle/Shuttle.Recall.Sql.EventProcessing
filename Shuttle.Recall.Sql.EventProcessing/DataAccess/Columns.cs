using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class Columns
{
    public static readonly Column<Guid> Id = new("Id", DbType.Guid);
    public static readonly Column<string> Name = new("Name", DbType.AnsiString);
    public static readonly Column<long> SequenceNumber = new("SequenceNumber", DbType.Int64);
}