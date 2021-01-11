using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public class Logger
    {
        private static readonly TraceSource _source = new TraceSource("JHDBBookListConsole");

        public static void OutputInformation(string message)
        {
            Console.WriteLine(message); ;
            _source.TraceEvent(TraceEventType.Information, 0, message);
            _source.Flush();
        }
        public static void OutputInformation(string message, string parameter)
        {
            string fullMessage = string.Format(message, parameter);
            Console.WriteLine(fullMessage);
            _source.TraceEvent(TraceEventType.Information, 0, message);
            _source.Flush();
        }
        public static void OutputError(string message)
        {
            Console.WriteLine(message);
            _source.TraceEvent(TraceEventType.Error, 0, message);
            _source.Flush();
        }
        public static void OutputError(string message, string parameter)
        {
            string fullMessage = string.Format(message, parameter);
            Console.WriteLine(fullMessage);
            _source.TraceEvent(TraceEventType.Error, 0, fullMessage);
            _source.Flush();
        }
        public static void OutputError(string message, int intPar, string strPar)
        {
            string fullMessage = string.Format(message, intPar, strPar);
            Console.WriteLine(fullMessage);
            _source.TraceEvent(TraceEventType.Error, 0, fullMessage);
            _source.Flush();
        }
    }
}
