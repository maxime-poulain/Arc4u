﻿using System;
using System.IO;
using System.Text;

namespace Arc4u.Diagnostics
{
    public class CommonLoggerProperties : BaseLoggerProperties, ILoggerProperties<CommonLoggerProperties>
    {
        internal CommonLoggerProperties(LoggerMessage loggerMessage) : base(loggerMessage) { }

        public CommonLoggerProperties Add(string key, int value)
        {
            AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties AddIf(bool condition, string key, int value)
        {
            if (condition) AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties Add(string key, double value)
        {
            AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties AddIf(bool condition, string key, double value)
        {
            if (condition) AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties Add(string key, bool value)
        {
            AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties AddIf(bool condition, string key, bool value)
        {
            if (condition) AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties Add(string key, long value)
        {
            AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties AddIf(bool condition, string key, long value)
        {
            if (condition) AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties Add(string key, string value)
        {
            AddProperty(key, value);
            return this;
        }

        public CommonLoggerProperties AddIf(bool condition, string key, string value)
        {
            if (condition) AddProperty(key, value);
            return this;
        }

        internal CommonLoggerProperties AddStacktrace(string stackTrace)
        {
            if (LoggerMessage.StackTrace == null)
                LoggerMessage.StackTrace = CleanupStackTrace(stackTrace);
            return this;
        }

        public CommonLoggerProperties AddStacktrace()
        {
            return AddStacktrace(Environment.StackTrace);
        }

        /// <summary>
        /// Remove internal traces, point of generation of the stacktrace.
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns>the cleaned-up stack trace</returns>
        private static string CleanupStackTrace(string stackTrace)
        {
            var traces = new StringReader(stackTrace);
            var output = new StringBuilder();
            string trace;
            while (null != (trace = traces.ReadLine()))
            {
                bool skip = trace.Contains("System.Environment")
                    || trace.Contains("Arc4u.Standard.Logging")
                    || trace.Contains("System.Diagnostics");
                if (!skip)
                    output.AppendLine(trace);
            }
            return output.ToString();
        }
    }
}
