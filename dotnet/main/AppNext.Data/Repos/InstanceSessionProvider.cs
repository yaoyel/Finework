using System;

namespace AppBoot.Repos
{
    public class InstanceSessionProvider<TSession> : ISessionProvider<TSession>
        where TSession : class, ISession
    {
        public InstanceSessionProvider(TSession instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            m_Instance = instance;
        }

        private readonly TSession m_Instance;

        public TSession GetSession()
        {
            return m_Instance;
        }
    }

    public static class SessionProviderHelper
    {
        public static InstanceSessionProvider<TSession> AsProvider<TSession>(this TSession session)
            where TSession : class, ISession
        {
            if (session == null) throw new ArgumentNullException("session");
            return new InstanceSessionProvider<TSession>(session);
        }
    }
}