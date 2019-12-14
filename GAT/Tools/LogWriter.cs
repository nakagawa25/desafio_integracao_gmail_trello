using System;
using System.IO;

namespace GAT.Tools
{
    public static class LogWriter
    {
        private static object syncObject = new object();
        public static void WriteLog(string message)
        {
            try
            {
                lock (syncObject)
                {
                    Console.WriteLine("Erro: " + message);
                    File.AppendAllText("Log.txt", message);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Erro ao Gravar Log: " + error.Message);
            }
        }
    }
}
