﻿using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace Arc4u.Standard.UnitTest.Logging
{
    public class LoggerCategoryFixture : SinkContainerFixture
    {
        protected override Logger BuildLogger(IConfiguration configuration)
        {
            _loggerCategorySink = new LoggerCategorySinkTest();
            _loggerCategorySink.Initialize();

            return _loggerCategorySink.Logger;
        }

        private LoggerCategorySinkTest _loggerCategorySink;

        public override ILogEventSink Sink => _loggerCategorySink.Sink;

        public override string ConfigFile => @"Configs\Basic.json";
    }
}
