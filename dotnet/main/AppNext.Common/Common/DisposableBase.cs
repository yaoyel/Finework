using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBoot.Common
{
    /// <summary> Provides the default <see cref="IDisposable"/> implementation. </summary>
    /// <remarks>
    /// see http://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx
    /// </remarks>
    public abstract class DisposableBase
    {
        ~DisposableBase()
        {
            Dispose(false);
        }

        #region Dispose

        /// <seealso cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                DoDispose(disposing);
                m_IsDisposed = true;
            }
        }

        /// <summary> Disposes resources. </summary>
        /// <param name="disposing"><c>true</c> if called by the <see cref="Dispose()"/> explicitly, otherwise called by the finalizer.</param>
        protected virtual void DoDispose(bool disposing)
        {
        }

        #endregion

        #region IsDisposed

        private bool m_IsDisposed;

        /// <summary> Indicates if this object has been disposed. </summary>
        public bool IsDisposed
        {
            get { return m_IsDisposed; }
        }

        #endregion

        /// <summary> Throws <see cref="ObjectDisposedException"/> if this object has been disposed. </summary>
        protected internal void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                var message = String.Format("The object of type [{0}] has been disposed.", GetType().AssemblyQualifiedName);
                throw new ObjectDisposedException(message);
            }
        }
    }
}
