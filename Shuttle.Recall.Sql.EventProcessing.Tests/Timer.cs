using System;
using System.Diagnostics;

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
	public class Timer : IDisposable
	{
		private Stopwatch _sw = new Stopwatch();
		private readonly string _what;

		private Timer(string what)
		{
			_what = what;

			_sw.Start();
		}

		public static Timer Time(string what)
		{
			return new Timer(what);
		}

		public void Dispose()
		{
			_sw.Stop();

			Console.WriteLine("[{0}] : elapsed ms = {1}", _what, _sw.ElapsedMilliseconds);
		}
	}
}