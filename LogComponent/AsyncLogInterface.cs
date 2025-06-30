namespace LogTest
{
	using System;
	using System.Collections.Generic;
	using System.IO;
    using System.Linq;
    using System.Text;
	using System.Threading;

	public class AsyncLogInterface : LogInterface, IDisposable
	{
		private Thread _runThread;
		private List<LogLine> _lines = new List<LogLine>();

		private StreamWriter _writer;

		private bool _exit;
		private readonly string basePath = @"./LogTest";
        private readonly string logPath = "/Log";
		private bool _QuitWithFlush = false;
		DateTime _curDate = DateTime.Now;

        public AsyncLogInterface()
		{
			if (!Directory.Exists(basePath))
				Directory.CreateDirectory(basePath);

			this._writer = File.AppendText(basePath + logPath + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

			this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

			this._writer.AutoFlush = true;

			this._runThread = new Thread(this.MainLoop);
			this._runThread.Start();
		}



		private void MainLoop()
		{
			while (!this._exit)
			{
				if (this._lines.Count > 0)
				{
					int f = 0;
					List<LogLine> _handled = new List<LogLine>();

					foreach (LogLine logLine in this._lines.ToList())
					{
						f++;

						if (f > 5)
							continue;

						if (!this._exit || this._QuitWithFlush)
						{
							_handled.Add(logLine);

							StringBuilder stringBuilder = new StringBuilder();

							if ((DateTime.Now - _curDate).Days != 0)
							{
								_curDate = DateTime.Now;

								this._writer = File.AppendText(basePath + logPath + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

								this._writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

								stringBuilder.Append(Environment.NewLine);

								this._writer.Write(stringBuilder.ToString());

								this._writer.AutoFlush = true;
							}

							stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
							stringBuilder.Append("\t");
							stringBuilder.Append(logLine.LineText());
							stringBuilder.Append("\t");

							stringBuilder.Append(Environment.NewLine);

							this._writer.Write(stringBuilder.ToString());
						}
					}

					for (int y = 0; y < _handled.Count; y++)
					{
						this._lines.Remove(_handled[y]);
					}

					if (this._QuitWithFlush == true && this._lines.Count == 0)
						this._exit = true;

					Thread.Sleep(50);
				}
			}
		}

		public void StopWithoutFlush()
		{
			this._exit = true;
		}

		public void StopWithFlush()
		{
			this._QuitWithFlush = true;
		}

		public void WriteLog(string s)
		{
			this._lines.Add(new LogLine() {Text = s, Timestamp = DateTime.Now});
		}

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}