using System;
using Microsoft.Extensions.Logging;

namespace FineWork.Logging
{
    /// <summary> <see cref="LogManager"/> simulates <c>log4net</c>'s <c>LogManager</c>
    /// to minimize the migration effort. </summary>
    public static class LogManager
    {
        private static ILoggerFactory m_DefaultFactory;

        public static void SetFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException("loggerFactory");
            m_DefaultFactory = loggerFactory;
        }

        public static ILoggerFactory Factory
        {
            get { return m_DefaultFactory ?? (m_DefaultFactory = new LoggerFactory()); }
        }

        public static ILogger GetLogger(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return Factory.CreateLogger(type.FullName);
        }
    }
}
