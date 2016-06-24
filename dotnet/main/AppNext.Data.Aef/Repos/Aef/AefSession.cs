using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace AppBoot.Repos.Aef
{
    /// <summary> Represents the base type for both sync/async sessions based on Entity Framework. </summary>
    public class AefSession : DbContext, ISession
    {
        /// <summary> Creates an instance. </summary>
        public AefSession(String nameOrConnectionString, bool writeChangesImmediately)
            : base(nameOrConnectionString)
        {
            this.m_WriteChangesImmediately = writeChangesImmediately;
        }

        protected AefSession()
            : base()
        {
            this.m_WriteChangesImmediately = false;
        }

        public DbContext DbContext
        {
            get { return this; }
        }

        private readonly bool m_WriteChangesImmediately;

        protected internal void HandleRepositoryChanged(IRepository repository)
        {
            if (m_WriteChangesImmediately)
            {
                SaveChanges();
            }
        }

        protected internal Task HandleRepositoryChangedAsync(IRepository repository)
        {
            if (m_WriteChangesImmediately)
            {
                return SaveChangesAsync();
            }
            return Task.FromResult(0);
        }

        Task ISession.SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
