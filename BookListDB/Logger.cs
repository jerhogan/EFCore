using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public class Logger
    {
        private static readonly TraceSource _source = new TraceSource("JHDBBookListConsole");
        private static string ReleaseLogFileName = "JHDBBookListRelease.Log";
        public static void logToRelease(string logString)
        {
            if (!File.Exists(ReleaseLogFileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(ReleaseLogFileName))
                {
                    sw.WriteLine("Creating new Release Log FiLe.");
                    sw.WriteLine("");
                }
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(ReleaseLogFileName))
            {
                sw.WriteLine(logString);
            }
        }
        public static void OutputInformation(string message)
        {
            Console.WriteLine(message);
            logToRelease(message);
            _source.TraceEvent(TraceEventType.Information, 0, message);
            _source.Flush();
        }
        public static void OutputInformation(string message, string parameter)
        {
            string fullMessage = string.Format(message, parameter);
            Console.WriteLine(fullMessage);
            logToRelease(fullMessage);
            _source.TraceEvent(TraceEventType.Information, 0, message);
            _source.Flush();
        }
        public static void OutputError(string message)
        {
            Console.WriteLine(message);
            logToRelease(message);
            _source.TraceEvent(TraceEventType.Error, 0, message);
            _source.Flush();
        }
        public static void OutputError(string message, string parameter)
        {
            string fullMessage = string.Format(message, parameter);
            Console.WriteLine(fullMessage);
            logToRelease(fullMessage);
            _source.TraceEvent(TraceEventType.Error, 0, fullMessage);
            _source.Flush();
        }
        public static void OutputError(string message, int intPar, string strPar)
        {
            string fullMessage = string.Format(message, intPar, strPar);
            Console.WriteLine(fullMessage);
            logToRelease(fullMessage);
            _source.TraceEvent(TraceEventType.Error, 0, fullMessage);
            _source.Flush();
        }
    }
}
