﻿using Serilog;
using Serilog.Core;
using System;
using ILogger = Serilog.ILogger;

namespace Arc4u.Diagnostics.Serilog
{
    public abstract class SerilogWriter : ILogWriter
    {
        private bool _isInitialized;
        private bool _disposed = false;
        private static object _locker = new object();
        private Logger _logger;

        public abstract void Configure(LoggerConfiguration configurator);

        public Logger Logger => _logger;

        public void Initialize()
        {
            lock (_locker)
            {
                if (_isInitialized) return;

                var configurator = new LoggerConfiguration()
                                             .MinimumLevel.Verbose()
                                             .Enrich.FromLogContext();

                Configure(configurator);

                _logger = configurator.CreateLogger();

                _isInitialized = true;
            }


        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                {
                    ((IDisposable)_logger)?.Dispose();
                    _disposed = true;
                }
        }
    }
}
