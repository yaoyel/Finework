using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Repos.Core
{
    [TestFixture]
    public class UpCastSetTests
    {
        #region Base/Derived

        public class Base
        {
            public Base(int value)
            {
                this.Value = value;
            }

            public int Value { get; set; }
        }

        public class Derive : Base
        {
            public Derive(int value)
                : base(value)
            {
            }
        }

        #endregion

        [Test]
        public void UpcastSet_adds_elements_from_source_collection()
        {
            var source = Derives(3);
            UpCastSet<Derive, Base> set = new UpCastSet<Derive, Base>(source);
            Assert.IsTrue(TestUtil.ElementEquals(source, set));
        }

        [Test]
        public void UpdateAll_adds_elements_from_source_collection()
        {
            var source = Derives(3);
            UpCastSet<Derive, Base> set = new UpCastSet<Derive, Base>();
            set.ResetItems(source);
            Assert.IsTrue(TestUtil.ElementEquals(source, set));
        }

        [Test]
        public void Explicit_GetEnumerator_returns_IEnumerable_of_TDecl()
        {
            UpCastSet<Derive, Base> set = new UpCastSet<Derive, Base>(Derives(2));
            using (var er = set.GetEnumerator())
            {
                Assert.NotNull(er);
                Assert.IsInstanceOf<IEnumerator<Derive>>(er);

                Assert.IsTrue(er.MoveNext());
                Assert.IsTrue(er.MoveNext());
                Assert.IsFalse(er.MoveNext());
            }
        }

        [Test]
        public void UpcastSet_implements_ICollection_of_Base()
        {
            ICollection<Base> upcasted = new UpCastSet<Derive, Base>();
            Assert.AreEqual(false, upcasted.IsReadOnly);
            Assert.AreEqual(0, upcasted.Count);

            //Adds an element
            var d = new Derive(1);
            upcasted.Add(d);
            Assert.AreEqual(1, upcasted.Count);
            Assert.AreEqual(true, upcasted.Contains(d));

            //CopyTo copies all elements to an array of Base
            var a1 = new Base[1];
            upcasted.CopyTo(a1, 0);
            Assert.AreSame(d, a1[0]);

            //Removes the element
            upcasted.Remove(d);
            Assert.AreEqual(0, upcasted.Count);
            Assert.AreEqual(false, upcasted.Contains(d));

            //CopyTo copies nothing after all elements are removed
            var a2 = new Base[0];
            upcasted.CopyTo(a2, 0);
            Assert.AreEqual(0, a2.Count());

            var b = new Base(1);
            Assert.Throws<InvalidCastException>(() => upcasted.Add(b));
            Assert.Throws<InvalidCastException>(() => upcasted.Remove(b));
        }

        [Test]
        public void UpcastSet_CopyTo_checks_arguments()
        {
            ICollection<Base> upcasted = new UpCastSet<Derive, Base>(Derives(1));
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => upcasted.CopyTo(null, 0)).ForArg("array");
            Assert.Throws<ArgumentOutOfRangeException>(() => upcasted.CopyTo(new Base[3], -1)).ForArg("arrayIndex");
            Assert.Throws<ArgumentException>(() => upcasted.CopyTo(new Base[1], 1)).ForArg("arrayIndex");
        }

        [Test]
        public void UpcastSet_throws_when_working_on_Base()
        {
            ICollection<Base> upcasted = new UpCastSet<Derive, Base>();
            var b = new Base(1);
            Assert.Throws<InvalidCastException>(() => upcasted.Add(b));
            Assert.Throws<InvalidCastException>(() => upcasted.Remove(b));
            Assert.Throws<InvalidCastException>(() => { var flag = upcasted.Contains(b); });
        }

        private static Derive[] Derives(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            Derive[] result = new Derive[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new Derive(i);
            }
            return result;
        }
    }
}
