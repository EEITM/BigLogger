namespace LogTest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class AsyncLogInterface : LogInterface, IDisposable
    {
        private Thread _runThread;
        private ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();

        private StreamWriter _writer;

        private bool _exit;
        private readonly string basePath = @"./LogTest";
        private readonly string logPath = "/Log";
        private bool _QuitWithFlush = false;
        DateTime _curDate = DateTime.Now;

        public AsyncLogInterface()
        {
            try
            {
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                _writer = File.AppendText(basePath + logPath + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

                _writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);

                _writer.AutoFlush = true;

                _runThread = new Thread(MainLoop);
                _runThread.Start();
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
            }
        }

        private void MainLoop()
        {
            while (!_exit)
            {
                try
                {
                    while (!_exit)
                    {
                        if (_lines.Count > 0)
                        {
                            int f = 0;
                            ConcurrentQueue<LogLine> _handled = new ConcurrentQueue<LogLine>();

                            foreach (LogLine logLine in _lines.ToList())
                            {
                                f++;
                                if (f > 5)
                                    continue;

                                try
                                {
                                    if (!_exit || _QuitWithFlush)
                                    {
                                        _handled.Enqueue(logLine);
                                        StringBuilder stringBuilder = new StringBuilder();

                                        if ((DateTime.Now.Date != _curDate.Date))
                                        {
                                            _curDate = DateTime.Now;

                                            _writer?.Dispose(); // Don't forget to dispose the old writer
                                            _writer = File.AppendText(basePath + logPath + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");

                                            _writer.Write("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t" + Environment.NewLine);
                                            _writer.AutoFlush = true;
                                        }

                                        stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                                        stringBuilder.Append("\t");
                                        stringBuilder.Append(logLine.LineText());
                                        stringBuilder.Append("\t");
                                        stringBuilder.Append(Environment.NewLine);

                                        _writer.Write(stringBuilder.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine("Error writing log line: " + ex.Message);
                                }
                            }

                            foreach (var handledLine in _handled)
                                _lines.TryDequeue(out LogLine dequeuedLine);

                            if (_QuitWithFlush && _lines.Count == 0)
                                _exit = true;

                            Thread.Sleep(50);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(ex.Message);
                }
            }
        }

        public void StopWithoutFlush()
        {
            _exit = true;
        }

        public void StopWithFlush()
        {
            _QuitWithFlush = true;
        }

        public void WriteLog(string s)
        {
            try
            {
                _lines.Enqueue(new LogLine() { Text = s, Timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}