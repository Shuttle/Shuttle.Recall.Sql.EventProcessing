using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class Columns
    {
        public static readonly MappedColumn<Guid> Id = new MappedColumn<Guid>("Id", DbType.Guid);
        public static readonly MappedColumn<string> Name = new MappedColumn<string>("Name", DbType.AnsiString);
        public static readonly MappedColumn<long> SequenceNumber = new MappedColumn<long>("SequenceNumber", DbType.Int64);
    }
}