using System;
using System.IO;

namespace Paiz
{
    class Logger
    {
        private static readonly string LoggerLocation = "C:\\Users\\Chris\\AppData\\Roaming\\Paiz\\Logger";
        private static DateTime _now = DateTime.Now;
        private static readonly string Filename = _now.Month.ToString() + " - " + _now.Year.ToString() + " - log.txt";

        public static void Write(string message)
        {
            try
            {
                string Filepath = Path.Combine(LoggerLocation, Filename);
                if (!File.Exists(Filepath))
                {
                    File.Create(Filepath);
                }
                using (StreamWriter writer = File.AppendText(Filepath))
                {
                    Log(message, writer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static private void Log(string msg, TextWriter writer)
        {
            try
            {
                writer.Write(Environment.NewLine);
                writer.Write("[{0} {1}]\t{2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
