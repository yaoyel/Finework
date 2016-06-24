using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBoot.Repos
{
    public class ResolvableSessionProvider<TSession> : ISessionProvider<TSession> 
        where TSession : class, ISession
    {
        public ResolvableSessionProvider(IServiceProvider underlyingServiceProvider)
        {
            if (underlyingServiceProvider == null) throw new ArgumentNullException(nameof(underlyingServiceProvider));
            m_UnderlyingServiceProvider = underlyingServiceProvider;
        }

        private IServiceProvider m_UnderlyingServiceProvider;

        public TSession GetSession()
        {
            var result = m_UnderlyingServiceProvider.GetService(typeof (TSession));
            return (TSession) result;
        }
    }
}
