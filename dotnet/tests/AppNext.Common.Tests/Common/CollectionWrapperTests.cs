using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Common
{
    [TestFixture]
    public class CollectionWrapperTests
    {
        private List<string> CreateList()
        {
            return new List<String> { "A", "B", "C" };
        }

        [Test]
        public void Constructor_throws_when_source_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CollectionWrapper<String, Object>(null)).ForArg("source");
        }

        [Test]
        public void Source_throws_when_value_is_null()
        {
            var w = new CollectionWrapper<String, Object>(new String[0]);
            Assert.Throws<ArgumentNullException>(() => w.Source = null).ForArg("value");
        }

        [Test]
        public void Source_can_be_changed()
        {
            var w = new CollectionWrapper<String, Object>(new String[0]);
            var s1 = w.Source;

            w.Source = new String[0];
            Assert.AreNotSame(s1, w.Source);
        }

        [Test]
        public void GetEnumerator_returns_IEnumerator()
        {
            var w = new CollectionWrapper<String, Object>(CreateList());
            var er = (w as IEnumerable).GetEnumerator();

            Assert.IsTrue(er.MoveNext());
            Assert.AreEqual("A", er.Current);
            
            Assert.IsTrue(er.MoveNext());
            Assert.AreEqual("B", er.Current);
            
            Assert.IsTrue(er.MoveNext());
            Assert.AreEqual("C", er.Current);

            Assert.IsFalse(er.MoveNext());
        }

        [Test]
        public void Add_adds_new_item_to_Source()
        {
            var w = new CollectionWrapper<String, Object>(CreateList());
            w.Add("D");
            Assert.IsTrue(CollectionUtil.ArrayEquals(new[] {"A", "B", "C", "D"}, w.Source.ToArray()));
        }

        [Test]
        public void Clear_clears_items()
        {
            var w = new CollectionWrapper<String, Object>(CreateList());
            w.Clear();
            Assert.AreEqual(0, w.Count);
        }

        [Test]
        public void Contains_returns_the_existence_of_an_element()
        {
            var w = new CollectionWrapper<String, Object>(CreateList());
            Assert.IsTrue(w.Contains("A"));
            Assert.IsFalse(w.Contains("D"));
        }

        [Test]
        public void CopyTo_copies_elements_to_array()
        {
            var w = new CollectionWrapper<String, Object>(CreateList());

            Object[] dest = new object[w.Count];
            w.CopyTo(dest, 0);

            Assert.IsTrue(CollectionUtil.ArrayEquals(new object[] {"A", "B", "C"}, dest));
        }

        [Test]
        public void Remove_removes_element_from_collection()
        {
            var w = new CollectionWrapper<String, Object>(CreateList());
            w.Remove("B");

            Assert.AreEqual(new Object[] {"A", "C"}, w.ToArray());
        }

        [Test]
        public void IsReadOnly_returns_the_readonly_status_of_its_source()
        {
            var w = new CollectionWrapper<String, Object>(CreateList());
            Assert.IsFalse(w.IsReadOnly);

            var w2 = new CollectionWrapper<String, Object>(new String[0]);
            Assert.IsTrue(w2.IsReadOnly);
        }
    }
}
