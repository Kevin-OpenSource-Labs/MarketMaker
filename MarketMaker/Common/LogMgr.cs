using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Common
{
    public class LogMgr : SingleTon<LogMgr>
    {
        public void Info(string msg)
        {
            lock (this)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(string.Format("[{0}] Info: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm.fff"), msg));
                Console.ForegroundColor = originalColor;
            }
        }
        public void Warn(string msg)
        {
            lock (this)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(string.Format("[{0}] Warn: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm.fff"), msg));
                Console.ForegroundColor = originalColor;
            }
        }
        public void Error(string msg)
        {
            lock (this)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("[{0}] Error: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm.fff"), msg));
                Console.ForegroundColor = originalColor;
            }
        }
    }
}
