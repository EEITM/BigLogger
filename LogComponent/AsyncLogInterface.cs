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
        private readonly CancellationTokenSource _cancellationTokenSource;

        private StreamWriter _writer;

        private readonly string basePath = @"./LogTest";
        private readonly string logPath = "/Log";
        private bool _quitWithFlush = false;
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

                _cancellationTokenSource = new CancellationTokenSource();
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
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (_lines.TryDequeue(out LogLine logLine))
                    {
                        var stringBuilder = new StringBuilder();

                        if ((DateTime.Now.Date != _curDate.Date))
                        {
                            _curDate = DateTime.Now;
                            _writer.Dispose();
                            _writer.AutoFlush = true;
                            _writer = File.AppendText(basePath + logPath + DateTime.Now.ToString("yyyyMMdd HHmmss fff") + ".log");
                            _writer.WriteLine("Timestamp".PadRight(25, ' ') + "\t" + "Data".PadRight(15, ' ') + "\t");
                        }

                        stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                        stringBuilder.Append("\t");
                        stringBuilder.Append(logLine.LineText());
                        stringBuilder.Append("\t");
                        stringBuilder.Append(Environment.NewLine);

                        _writer.Write(stringBuilder.ToString());
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }

                // Flush remaining logs if necessary (q. 3¨)
                if (_quitWithFlush)
                {
                    while (_lines.TryDequeue(out LogLine logLine))
                    {
                        var stringBuilder = new StringBuilder();
                        stringBuilder.Append(logLine.Timestamp.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                        stringBuilder.Append("\t");
                        stringBuilder.Append(logLine.LineText());
                        stringBuilder.Append("\t");
                        stringBuilder.Append(Environment.NewLine);

                        _writer.Write(stringBuilder.ToString());
                    }
                }

                _writer.Dispose();
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error in main loop: " + ex.ToString());            
            }
        }

        public void StopWithoutFlush()
        {
            _cancellationTokenSource.Cancel();
        }

        public void StopWithFlush()
        {
            _quitWithFlush = true;
            _cancellationTokenSource.Cancel();
        }

        public void WriteLog(string s)
        {
            try
            {
                _lines.Enqueue(new LogLine() { Text = s, Timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _runThread.Join();
            _writer.Dispose();
        }
    }
}