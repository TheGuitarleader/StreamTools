using System.Diagnostics;
using System.Reflection;

namespace StreamTools.Logging
{
    public class Logger
    {
        //private static StreamWriter _logger;

        internal static string WorkingDir = Directory.GetCurrentDirectory();
        internal static string crashDir = Path.Combine(WorkingDir, "Crashes");
        internal static string logDir = Path.Combine(WorkingDir, "Logs");
        private static string appName;

        //public enum LogLevel { Debug, Error, Info, Warn }

        /// <summary>
        /// If the logger should print to the console.
        /// </summary>
        public static bool PrintToConsole { get; set; } = true;

        /// <summary>
        /// If the logger should write logs to a file.
        /// </summary>
        public static bool LogToFile { get; set; } = true;

        /// <summary>
        /// If the logger should log
        /// </summary>
        public static bool AssemblyEvents { get; set; } = false;

        /// <summary>
        /// If the logger should catch UnhandledException events.
        /// </summary>
        public static bool ExceptionEvents { get; set; } = true;

        /// <summary>
        /// If the logger should catch ProcessExit events.
        /// </summary>
        public static bool ExitEvents { get; set; } = true;

        /// <summary>
        /// If the logger should save the current log on exit.
        /// </summary>
        public static bool SaveLogOnExit { get; set; } = false;

        /// <summary>
        /// If the logger should log DEBUG events.
        /// </summary>
        public static bool IsDebugEnabled { get; set; } = true;

        /// <summary>
        /// If the logger should log INFO events.
        /// </summary>
        public static bool IsInfoEnabled { get; set; } = true;

        /// <summary>
        /// If the logger should log WARN events.
        /// </summary>
        public static bool IsWarnEnabled { get; set; } = true;

        /// <summary>
        /// If the logger should log ERROR events.
        /// </summary>
        public static bool IsErrorEnabled { get; set; } = true;

        /// <summary>
        /// Occurs when the process exits and <see cref="ExitEvents"/> are enabled.
        /// </summary>
        public static event EventHandler<EventArgs> Shutdown;

        #region Privates

        internal static string GetDate()
        {
            DateTime n = DateTime.Now;
            return n.ToString("yyyy-MM-dd HH-mm-ss");
        }

        internal static string GetFilename(int index)
        {
            try
            {
                StackTrace stack = new StackTrace(true);
                StackFrame frame = stack.GetFrame(index) != null ? stack.GetFrame(index) : stack.GetFrame(0);

                string[] path = frame.GetFileName().Split('\\');
                string filename = path[path.Length - 1];
                return filename.Substring(0, filename.IndexOf('.'));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Log($"Loaded assembly: {args.LoadedAssembly.FullName}", "DEBUG", GetFilename(2));
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Log($"Failed to load assembly '{args.Name}' ({args.RequestingAssembly.FullName})", "DEBUG", GetFilename(2));
            return args.RequestingAssembly;
        }

        private static Assembly? CurrentDomain_ReflectionOnlyAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            Log($"Failed to load assembly '{args.Name}' ({args.RequestingAssembly.FullName})", "DEBUG", GetFilename(2));
            return args.RequestingAssembly;
        }

        private static Assembly? CurrentDomain_ResourceResolve(object? sender, ResolveEventArgs args)
        {
            Log($"Failed to load resource: '{args.RequestingAssembly.FullName}'", "DEBUG", GetFilename(2));
            return args.RequestingAssembly;
        }

        private static Assembly? CurrentDomain_TypeResolve(object? sender, ResolveEventArgs args)
        {
            Log($"Failed to load type: '{args.RequestingAssembly.FullName}'", "DEBUG", GetFilename(2));
            return args.RequestingAssembly;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            if (e.IsTerminating)
            {
                Error(ex.GetBaseException());
                AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
                File.Move(Path.Combine(logDir, "latest.log"), Path.Combine(crashDir, $"{GetDate()}.log"));
            }
            else
            {
                Warn(ex.GetBaseException());
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Info($"{appName} exited.");
            if(SaveLogOnExit)
            {
                File.Move(Path.Combine(logDir, "latest.log"), Path.Combine(logDir, $"{GetDate()}.log"));
                File.Delete(Path.Combine(logDir, "latest.log"));
            }

            Shutdown?.Invoke(sender, e);
        }

        #endregion

        //public static void Initialize(AssemblyName app, bool keep = false)
        //{
        //    crashDir = Path.Combine(WorkingDir, "Crashes");
        //    logDir = Path.Combine(WorkingDir, "Logs");

        //    if (!Directory.Exists(crashDir)) Directory.CreateDirectory(crashDir);
        //    if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
        //    if (File.Exists(Path.Combine(logDir, "latest.log")) && keep == false)
        //    {
        //        Delete();
        //    }

        //    FileStream fs = File.Create(Path.Combine(logDir, "latest.log"));
        //    fs.Close();
        //    fs.Dispose();

        //    logline($"{app.Name} v{app.Version}", "INFO", 2, ConsoleColor.Gray);
        //}

        /// <summary>
        /// Initializes a new logger instance for the current process.
        /// </summary>
        /// <param name="app">The current <see cref="AssemblyName"/> for this process.</param>
        /// <param name="keep">True if the logger should append the current log.</param>
        public static void Initialize(AssemblyName app, bool keep = false)
        {
            if (!Directory.Exists(crashDir)) Directory.CreateDirectory(crashDir);
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            if (File.Exists(Path.Combine(logDir, "latest.log")) && keep == false)
            {
                Delete();
            }

            FileStream fs = File.Create(Path.Combine(logDir, "latest.log"));
            fs.Close();
            fs.Dispose();

            appName = app.Name;
            Log($"{app.Name} v{app.Version}", "INFO", GetFilename(2));
            Debug($"Logging saved to '{Path.Combine(logDir, "latest.log")}'");

            if (AssemblyEvents)
            {
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
                AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
                AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
            }

            if (ExceptionEvents)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            if (ExitEvents)
            {
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            }
        }

        /// <summary>
        /// Deletes the current log file.
        /// </summary>
        public static void Delete()
        {
            File.Delete(logDir + @"\latest.log");
        }

        public static void Exception(Exception exception)
        {
            //Announce("EXCEPTION THROWN", "ERROR");
            Error($"{exception.GetBaseException()}");
            //Announce("END", "ERROR");
        }

        public static void Log(object message, object type, string file, string instance = "main", ConsoleColor color = ConsoleColor.Gray)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:sstt");
            string filename = string.IsNullOrEmpty(file) ? "app" : " [" + file + "]";
            string format = $"[{timestamp}] [{filename}/{type.ToString()}]: {message.ToString()}";

            if (PrintToConsole)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(format);
                Console.ResetColor();
            }

            if (LogToFile)
            {
                using StreamWriter logger = new(Path.Combine(logDir, "latest.log"), true);
                logger.WriteLineAsync(format);
                logger.Close();
            }
        }

        public static void LogClean(object message, object type, string file, string instance = "main", ConsoleColor color = ConsoleColor.Gray)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:sstt");
            string filename = string.IsNullOrEmpty(file) ? "app" : " [" + file + "]";
            string format = $"[{timestamp}] [{filename}/{type.ToString()}]: {message.ToString()}";

            //if(PrintToConsole)
            //{

            //}

            Console.ForegroundColor = color;
            Console.WriteLine(message.ToString());
            Console.ResetColor();

            if (LogToFile)
            {
                using StreamWriter logger = new(Path.Combine(logDir, "latest.log"), true);
                logger.WriteLineAsync(format);
                logger.Close();
            }
        }

        /// <summary>
        /// Logs a message as DEBUG.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
            if (!IsDebugEnabled) return;
            string file = GetFilename(2);
            Log(message, "DEBUG", file, "main", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Logs a message as DEBUG, but removes the timestamp and stack trace on the command line.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void DebugClean(object message)
        {
            if (!IsDebugEnabled) return;
            string file = GetFilename(2);
            LogClean(message, "DEBUG", file, "main", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Logs a message as INFO.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(object message)
        {
            if (!IsInfoEnabled) return;
            string file = GetFilename(2);
            Log(message, "INFO", file);
        }

        /// <summary>
        /// Logs a message as INFO, but removes the timestamp and stack trace on the command line.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void InfoClean(object message)
        {
            if (!IsInfoEnabled) return;
            string file = GetFilename(2);
            LogClean(message, "INFO", file);
        }

        /// <summary>
        /// Logs a message as WARN.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Warn(object message)
        {
            if (!IsWarnEnabled) return;
            string file = GetFilename(2);
            Log(message, "WARN", file, "main", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs a message as WARN, but removes the timestamp and stack trace on the command line.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void WarnClean(object message)
        {
            if (!IsWarnEnabled) return;
            string file = GetFilename(2);
            LogClean(message, "WARN", file, "main", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs a message as ERROR.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Error(object message)
        {
            if (!IsErrorEnabled) return;
            string file = GetFilename(2);
            Log(message, "ERROR", file, "main", ConsoleColor.Red);
        }

        /// <summary>
        /// Logs a message as ERROR, but removes the timestamp and stack trace on the command line.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void ErrorClean(object message)
        {
            if (!IsErrorEnabled) return;
            string file = GetFilename(2);
            LogClean(message, "ERROR", file, "main", ConsoleColor.Red);
        }

        public static void Message(object message, object from, ConsoleColor color = ConsoleColor.Gray)
        {
            StreamWriter logger = new StreamWriter(logDir + @"\latest.log", true);
            var timestamp = DateTime.Now;
            string format = $"[{timestamp.ToString("hh:mm:sstt")}] ({from}): {message}";

            Console.ForegroundColor = color;
            Console.WriteLine(format);
            Console.ResetColor();

            logger.WriteLine(format);
            logger.Flush();
            logger.Dispose();
        }

        /// <summary>
        /// Logs a message as INFO.
        /// </summary>
        /// <param name="message">The message to log.</param>
        [Obsolete("Please use a more specific logging level.")]
        public static void WriteLine(object message)
        {
            Log(message, "INFO", GetFilename(2));
        }
    }
}
