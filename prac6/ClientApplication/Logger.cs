using System;
using System.IO;
using APIClasses;

namespace ClientApplication
{
    class Logger
    {
        private readonly string _logs_path;
        private string _clientIdentifier;

        public static Logger Instance
        {
            get;
        } = new Logger();

        private Logger()
        {
            _logs_path = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName,
                "client-logs.log");
            _clientIdentifier = "unknown";
        }

        /// <summary>
        /// Sets the client identifier (address:port) from which to log
        /// </summary>
        public void SetClient(ClientData client)
        {
            _clientIdentifier = $"{client}";
        }

        /// <summary>
        /// Logs given data to logs file for this client's identifier
        /// </summary>
        public void Log(string message)
        {
            //Add log number and increment log number
            StreamWriter logsFileWriter = File.AppendText(_logs_path);
            logsFileWriter.WriteLine($"[{_clientIdentifier}] {message}");
            logsFileWriter.Close();
        }
    }
}
