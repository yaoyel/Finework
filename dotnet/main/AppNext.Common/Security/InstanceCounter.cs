using System;
using System.Threading;

namespace AppBoot.Diagnostics
{
    /// <summary> The helper class for counting instances for a specific type. </summary>
    /// <remarks> 
    /// <list type="number">
    /// <item>How many instances are created ? </item>
    /// <item>How many instances are destructed by calling <see cref="IDisposable.Dispose"/> explicitly from code ? </item>
    /// <item>How many instances are destructed by Garbage Collector and so finalizers are called implicitly ? </item>
    /// <item>How many instances are still available, i.e. has not been destructed either explicitly nor implicitly ? </item>
    /// </list>
    /// This property is mainly for the diagnostic purpose. 
    /// </remarks>
    public class InstanceCounter
    {
        private readonly Object m_Lock = new Object();

        /// <summary> The count of instances created. </summary>
        private int m_CreatedCount;

        /// <summary> Gets the count of instances created. </summary>
        public int CreatedCount
        {
            get { return m_CreatedCount; }
        }

        /// <summary> Increase the count of constructor calls. </summary>
        /// <remarks> 
        /// The call to this method should be placed before any other lines in a constructor
        /// to ensure the <see cref="CreatedCount"/> is increased 
        /// even if the constructor throws an exception.
        /// <para> 
        /// A known issue is that this method will not be invoked
        /// if one of the constructor in any base class throws an exception.
        /// </para>
        /// </remarks>
        public void InstanceCreated()
        {
            Interlocked.Increment(ref m_CreatedCount);
        }

        private int m_DisposedCount;

        /// <summary> Gets the count of instances destructed explicitly with <see cref="IDisposable.Dispose"/>. </summary>
        public int DisposedCount
        {
            get { return m_DisposedCount; }
        }

        /// <summary> Increase the count of explicit <see cref="IDisposable.Dispose"/> calls. </summary>
        /// <remarks> 
        /// This method should be invoked only when the <c>disposing</c> argument is <c>true</c>
        /// When applying the standard Disposable pattern.
        /// </remarks>
        public void InstanceDisposed()
        {
            Interlocked.Increment(ref m_DisposedCount);
        }

        private int m_FinalizedCount;

        /// <summary> Gets the count of instances destructed implicitly with finalizers. </summary>
        /// <remarks> NOTE: The finalizer will run against object even if its constructor throws an exception. </remarks>
        public int FinalizedCount
        {
            get { return m_FinalizedCount; }
        }

        /// <summary> Increase the count of implicitly finalizer calls. </summary>
        /// <remarks> 
        /// This method should be invoked only when the <c>disposing</c> argument is <c>false</c>
        /// When applying the standard Disposable pattern.
        /// </remarks>
        public void InstanceFinalized()
        {
            Interlocked.Increment(ref m_FinalizedCount);
        }

        /// <summary> Gets the count of instances that is still alive. </summary>
        /// <remarks> 
        /// NOTE: The <see cref="CreatedCount"/> may not be increased by <see cref="InstanceCreated"/>
        /// if a constructor in any base class throws an exception.
        /// </remarks>
        public int LivingCount
        {
            get
            {
                lock (m_Lock)
                {
                    return m_CreatedCount - m_DisposedCount - m_FinalizedCount;
                }
            }
        }

        /// <summary> Reset all counters. </summary>
        public void Reset()
        {
            lock (m_Lock)
            {
                m_CreatedCount = 0;
                m_DisposedCount = 0;
                m_FinalizedCount = 0;
            }
        }
    }
}
