using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms.PlatformConfiguration;

namespace Utils
{
    public enum eTypeLog
    {
        Full=0,
        Expanded=1,
        Error=2
    }
    public static class FileLogger
    {
        //AppDomain.CurrentDomain.BaseDirectory
        //public static string PathLog = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
        public static string _PathLog = null;
        private static string PathLog { get { return _PathLog ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Log"); } }
        private static eTypeLog TypeLog = eTypeLog.Full;

        private static Dictionary<int, Type> _types = new Dictionary<int, Type>();

        private static readonly object Locker = new object();
        private static readonly object DictionaryLocker = new object();

        static FileLogger()
        {
            CreateDir();
        }
        public static void CreateDir()
        {
            if (!Directory.Exists(PathLog))
                Directory.CreateDirectory(PathLog);
        }

        public static void ExtLogForClass(Type type, int hashCode, string message, string parameters = null)
        {
            if (!string.IsNullOrWhiteSpace(parameters))
                message += $" {parameters}";

            WriteLogMessage($"[{type} - {hashCode}] {message}");
        }

        public static void ExtLogForClassConstruct(Type type, int hashCode, string parameters = null)
        {
            lock (DictionaryLocker)
            {
                if (!_types.ContainsKey(hashCode))
                    _types.Add(hashCode, type);
            }

            var message = "";
            if (!string.IsNullOrWhiteSpace(parameters))
                message += $" {parameters}";

            WriteLogMessage($"[{type} - {hashCode}] constructed {message}");
        }

        public static void ExtLogForClassDestruct(int hashCode, string parameters = null)
        {
            Type type;
            lock (DictionaryLocker)
            {
                if (_types.TryGetValue(hashCode, out type))
                    _types.Remove(hashCode);
            }

            var message = "";
            if (!string.IsNullOrWhiteSpace(parameters))
                message += $" {parameters}";

            WriteLogMessage($"[{type} - {hashCode}] destructed {message}");
        }

        public static void WriteLogMessage(this string message, eTypeLog pTypeLog = eTypeLog.Full)
        {
            var date = DateTime.Now;
            var Message = $@"[{date:dd-MM-yyyy HH:mm:ss}] {Enum.GetName(typeof(eTypeLog), pTypeLog)} {message}{Environment.NewLine}";
#if DEBUG
            Message.WriteConsoleDebug();
            if (TypeLog > pTypeLog)
                return;
#endif
            Task.Run(() =>
            {
                var FileName = $"{Path.Combine(PathLog, $"LogBRB5_{date:yyyyMMdd}.log")}";
                lock (Locker)
                {
                    try
                    {
                        File.AppendAllText(FileName, Message);
                    }
                    catch (Exception e)
                    {e.Message.WriteConsoleDebug(); }
                }
            });
        }

        public static void WriteConsoleDebug(this string message)
        {
            Console.WriteLine($@"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] {message}");
            // Console.ReadKey();
        }
    }
}