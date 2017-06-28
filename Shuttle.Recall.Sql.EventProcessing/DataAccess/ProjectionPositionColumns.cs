using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public class ProjectionPositionColumns
	{
		public static readonly MappedColumn<string> Name = new MappedColumn<string>("Name", DbType.AnsiString, 65);
		public static readonly MappedColumn<long> SequenceNumber = new MappedColumn<long>("SequenceNumber", DbType.Int64);
	}
}