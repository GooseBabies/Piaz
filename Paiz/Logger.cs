using System;
using System.IO;
using System.Text;

namespace Paiz
{
    class Logger
    {
        private static readonly string LoggerLocation = "C:\\Users\\Chris\\AppData\\Roaming\\Paiz\\Logger";
        private static DateTime _now = DateTime.Now;
        private static readonly string Filename = _now.Month.ToString() + " - " + _now.Year.ToString() + " - log.txt";
        private static StringBuilder LogAssembler = new();
        const int LinesBeforWrite = 300;

        public static void Write(string message)
        {
            try
            {
                //LogAssembler = LogAssembler.AppendLine(Environment.NewLine);
                LogAssembler = LogAssembler.AppendLine("[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "\t" + message);
                if(LogAssembler.Length > LinesBeforWrite)
                {
                    Log();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static public void Log()
        {
            try
            {
                string Filepath = Path.Combine(LoggerLocation, Filename);
                if (!File.Exists(Filepath))
                {
                    File.Create(Filepath);
                }
                using StreamWriter writer = File.AppendText(Filepath);
                writer.WriteLine(LogAssembler.ToString());
                LogAssembler.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
