using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interoperability
{
    /// <summary>
    /// Encapsulates method used for logging entries
    /// </summary>
    /// <param name="entry"></param>
    public delegate void LogEvent(LogEntry entry);

    /// <summary>
    /// Describes level of log entry.
    /// </summary>
    public enum LogLevels
    {
        /// <summary>
        /// Lowest log level. Used for informing user about editor internals.
        /// </summary>
        Message = 0,

        /// <summary>
        /// Some expected problem appeared. Editor should solve it, but operation need extra time.        
        /// </summary>
        Notification,

        /// <summary>
        /// Some expected problem appeared. Editor may not solve it.
        /// </summary>
        Warning,

        /// <summary>
        /// Error which cannot be solved appeared. Probably cant get all needed instances, types, exceptions,..
        /// </summary>
        Error
    }

    /// <summary>
    /// Implementation for log entry.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Level of log entry.
        /// </summary>
        public readonly LogLevels Level;

        /// <summary>
        /// Logged message.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Description of logged message.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Navigation position of log entry.
        /// </summary>
        public readonly Action Navigate;

        /// <summary>
        /// Create Log entry
        /// </summary>
        /// <param name="level">Level of log entry.</param>
        /// <param name="message">Logged message.</param>
        /// <param name="description">Description of logged message.</param>
        /// <param name="position">Navigation position of log entry.</param>
        public LogEntry(LogLevels level, string message, string description, Action navigate)
        {
            Level = level;
            Message = message;
            Description = description;
            Navigate = navigate;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = string.Format("[{0}]{1}{2}", Level, Environment.NewLine, Message);
            if (Description != null)
                result += Environment.NewLine + Environment.NewLine + Description;

            return result;
        }
    }


    /// <summary>
    /// Class for logging services.    
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Minimal output level of logging
        /// </summary>
        static LogLevels _minLevel = LogLevels.Message;

        /// <summary>
        /// Is called for every logged description above _minLevel. Here are attached all loggers.
        /// </summary>
        public event LogEvent OnLog;

        /// <summary>
        /// Set minimal output level for logged messages
        /// </summary>
        /// <param name="minLevel">Minimal level for logged messages</param>
        public void SetOutputLevel(LogLevels minLevel)
        {
            _minLevel = minLevel;
        }

        /// <summary>
        /// Log description level log entry
        /// </summary>
        /// <param name="formatedMessage">Formated string for logged description</param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        public void Message(string formatedMessage, params object[] args)
        {
            Write(LogLevels.Message, formatedMessage, args);
        }

        /// <summary>
        /// Log specified level log entry
        /// </summary>
        /// <param name="level">Log level of log entry</param>
        /// <param name="formatedMessage">Formated string for logged description</param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        public void Write(LogLevels level, string formatedMessage, params object[] args)
        {
            Entry(new LogEntry(level, String.Format(formatedMessage, args), null, null));
        }

        /// <summary>
        /// Log warning level log entry
        /// </summary>
        /// <param name="formatedMessage">Formated string for logged description</param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        public void Warning(string formatedMessage, params object[] args)
        {
            Write(LogLevels.Warning, formatedMessage, args);
        }

        /// <summary>
        /// Log error level log entry
        /// </summary>
        /// <param name="formatedMessage">Formated string for logged description</param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        public void Error(string formatedMessage, params object[] args)
        {
            Write(LogLevels.Error, formatedMessage, args);
        }

        /// <summary>
        /// Log specified entry
        /// </summary>
        /// <param name="entry">Entry which will be logged</param>
        public void Entry(LogEntry entry)
        {
            if (OnLog == null)
                //empty log
                return;

            if (entry.Level < _minLevel)
                //entry below level
                return;

            OnLog(entry);
        }
    }
}
