using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Common
{
    [TestFixture]
    public class CollectionUtilTests
    {
        [Test]
        public void ArrayEquals_throws_when_elementComparer_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtil.ArrayEquals<Object>(null, null, null))
                .ForArg("elementComparer");
        }

        [Test]
        public void ArrayEquals_returns_true_when_two_arrays_are_null()
        {
            bool areEqual = CollectionUtil.ArrayEquals<Object>(null, null);
            Assert.IsTrue(areEqual);
        }

        [Test]
        public void ArrayEquals_returns_true_when_two_arrays_are_empty()
        {
            String[] a = {};
            String[] b = {};
            bool areEqual = CollectionUtil.ArrayEquals(a, b);
            Assert.IsTrue(areEqual);
        }

        [Test]
        public void ArrayEquals_returns_false_when_only_one_array_is_null()
        {
            bool areEqual = CollectionUtil.ArrayEquals(new string[] {}, null);
            Assert.IsFalse(areEqual);
        }

        [Test]
        public void ArrayEquals_returns_false_when_length_not_match()
        {
            String[] a = { };
            String[] b = { "" };
            bool areEqual = CollectionUtil.ArrayEquals(a, b);
            Assert.IsFalse(areEqual);
        }


        [Test]
        public void ArrayEquals_returns_false_when_first_elements_not_match()
        {
            String[] a = { "x" };
            String[] b = { "" };
            bool areEqual = CollectionUtil.ArrayEquals(a, b);
            Assert.IsFalse(areEqual);
        }

        [Test]
        public void ArrayEquals_returns_false_when_last_elements_not_match()
        {
            String[] a = { "a", "a" };
            String[] b = { "a", "A" };
            bool areEqual = CollectionUtil.ArrayEquals(a, b);
            Assert.IsFalse(areEqual);
        }
        
        [Test]
        public void AddAll_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CollectionUtil.AddAll<Object>(null, null)).ForArg("collection");
            Assert.Throws<ArgumentNullException>(() => CollectionUtil.AddAll<Object>(new List<object>(), null)).ForArg("source");
        }

        [Test]
        public void AddAll_adds_all_items()
        {
            ICollection<String> collection = new List<String>() {"a", "b"};
            ICollection<String> source = new List<String>() {"A", "B", "C"};

            collection.AddAll(source);

            Assert.AreEqual(new String[] {"a","b","A","B","C"}, collection.ToArray());
        }
    }
}
