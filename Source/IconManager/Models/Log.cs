using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace IconManager
{
    /// <summary>
    /// An extremely simple, thread-safe log of messages.
    /// </summary>
    public class Log
    {
        private List<string> _messages = new List<string>();
        private Lock _messagesLock = new();

        /// <summary>
        /// Gets a value indicating whether the log is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                bool isEmpty;

                lock (_messagesLock)
                {
                    isEmpty = this._messages.Count == 0;
                }

                return isEmpty;
            }
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Error(string message)
        {
            lock (_messagesLock)
            {
                _messages.Add($"Error: {message}");
            }

            return;
        }

        /// <summary>
        /// Logs a general message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Message(string message)
        {
            lock (_messagesLock)
            {
                _messages.Add(message);
            }

            return;
        }

        /// <summary>
        /// Exports the log of messages to the given file path.
        /// Existing files will be overwritten, directories will automatically be created.
        /// </summary>
        /// <param name="filePath">The destination file path to write the log to.</param>
        public void Export(string filePath)
        {
            string log;

            lock (_messagesLock)
            {
                log = string.Join(Environment.NewLine, _messages);
            }

            if (string.IsNullOrWhiteSpace(filePath) == false)
            {
                if (File.Exists(filePath))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(filePath);
                }

                string? directoryName = Path.GetDirectoryName(filePath);
                if (directoryName is not null &&
                    Directory.Exists(directoryName) == false)
                {
                    Directory.CreateDirectory(directoryName);
                }

                using (var fileStream = File.OpenWrite(filePath))
                {
                    fileStream.Write(Encoding.UTF8.GetBytes(log));
                }
            }

            return;
        }
    }
}
