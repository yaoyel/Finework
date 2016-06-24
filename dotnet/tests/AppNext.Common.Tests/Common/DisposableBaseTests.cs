using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace AppBoot.Common
{
    [TestFixture]
    public class DisposableBaseTests
    {
        public class CustomDisposeable : DisposableBase
        {
            public CustomDisposeable(Action disposeAction)
            {
                this.m_DisposeAction = disposeAction;
            }

            private Action m_DisposeAction;

            protected override void DoDispose(bool disposing)
            {
                if (m_DisposeAction != null)
                {
                    m_DisposeAction();
                }
                base.DoDispose(disposing);
            }
        }

        [Test]
        public void Dispose_disposes_instance()
        {
            bool isCalled = false;
            var obj = new CustomDisposeable(() => { isCalled = true; });
            obj.Dispose();

            Assert.IsTrue(obj.IsDisposed);
            Assert.IsTrue(isCalled);
        }

        [Test]
        public void Finalizer_disposes_instance()
        {
            bool isCalled = false;
            
            // ReSharper disable once NotAccessedVariable
            var obj = new CustomDisposeable(() => { isCalled = true; });
            obj = null; //make the obj can be collected

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void ThrowIfDisposed_throws_when_instance_is_disposed()
        {
            var obj = new CustomDisposeable(null);
            obj.Dispose();

            Assert.Throws<ObjectDisposedException>(() => obj.ThrowIfDisposed());
        }

        [Test]
        public void ThrowIfDisposed_passes_when_instance_is_not_disposed()
        {
            var obj = new CustomDisposeable(null);
            obj.ThrowIfDisposed();  //will NOT throw
            obj.Dispose();
        }
    }
}
