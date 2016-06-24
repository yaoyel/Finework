using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AppBoot.TestCommon
{
    public static class TestUtil
    {
        public static TArgumentException ForArg<TArgumentException>(
            this TArgumentException argumentException, String argumentName)
            where TArgumentException : ArgumentException
        {
            if (argumentException == null) throw new ArgumentNullException("argumentException");
            if (String.IsNullOrEmpty(argumentName)) throw new ArgumentNullException("argumentName");
            Assert.AreEqual(argumentName, argumentException.ParamName, "Unexpected argument.");
            return argumentException;
        }

        public static void MessageContains<TException>(this TException exception, String messageFragment)
            where TException : Exception
        {
            if (exception == null) throw new ArgumentNullException("exception");
            StringAssert.Contains(messageFragment, exception.Message);
        }

        public static void MessageStartsWith<TException>(this TException exception, String messageFragment)
            where TException : Exception
        {
            if (exception == null) throw new ArgumentNullException("exception");
            StringAssert.StartsWith(messageFragment, exception.Message);
        }

        public static bool ElementEquals<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            if (a == null) throw new ArgumentNullException("a");
            if (b == null) throw new ArgumentNullException("b");

            if (ReferenceEquals(a, b)) return true;

            var arrA = a.ToArray();
            var arrB = b.ToArray();

            if (arrA.Length != arrB.Length) return false;
            if (arrA.Except(arrB).Any()) return false;
            if (arrB.Except(arrA).Any()) return false;

            return true;
        }
    }
}
