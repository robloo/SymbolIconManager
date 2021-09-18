using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IconManager
{
    /// <summary>
    /// An extremely simple, thread-safe log of messages.
    /// </summary>
    public class Log
    {
        private List<string> messages = new List<string>();
        private object messagesMutex = new object();

        /// <summary>
        /// Gets a value indicating whether the log is empty.
        /// </summary>
        public bool IsEmpty
        {
            get => this.messages.Count == 0;
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Error(string message)
        {
            lock (messagesMutex)
            {
                messages.Add($"Error: {message}");
            }

            return;
        }

        /// <summary>
        /// Logs a general message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Message(string message)
        {
            lock (messagesMutex)
            {
                messages.Add(message);
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
            string log = string.Join(Environment.NewLine, messages);

            if (string.IsNullOrWhiteSpace(filePath) == false)
            {
                if (File.Exists(filePath))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(filePath);
                }

                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
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
