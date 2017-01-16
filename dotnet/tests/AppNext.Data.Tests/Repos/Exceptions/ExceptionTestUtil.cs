using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace AppBoot.Repos.Exceptions
{
    public static class ExceptionTestUtil
    {
        private const String m_Message = "This is an exception message.";
        private const String m_InnerMessage = "This is an inner exception message.";

        /// <summary>
        /// Checks the exception type matches the guidelines in
        /// http://msdn.microsoft.com/en-us/library/vstudio/ms229064(v=vs.100
        /// ).aspx
        /// </summary>
        /// <typeparam name="T"> The exception type. </typeparam>
        public static void CheckGuidelines<T>()
            where T : Exception
        {
            CheckDefaultConstructor<T>();
            CheckConstructorTakesMessage<T>();
            CheckConstructorTakesMessageAndInner<T>();
            CheckSerialization<T>();
        }

        /// <summary> Checks an exception type has a default constructor. </summary>
        public static void CheckDefaultConstructor<T>()
            where T : Exception
        {
            var exception = Activator.CreateInstance<T>();
            Assert.NotNull(exception);
        }

        /// <summary> Checks an exception type has a constructor that 
        /// takes an exception message. </summary>
        public static void CheckConstructorTakesMessage<T>()
            where T : Exception
        {
            var exception = (T)Activator.CreateInstance(typeof(T), m_Message);
            Assert.AreEqual(m_Message, exception.Message);
        }

        /// <summary> Checks an exception type has a constructor that 
        /// takes an exception message and an inner exception. </summary>
        public static void CheckConstructorTakesMessageAndInner<T>()
            where T : Exception
        {
            var innerException = new Exception(m_InnerMessage);
            T ex = (T)Activator.CreateInstance(typeof(T), m_Message, innerException);
            Assert.AreEqual(m_Message, ex.Message);
            Assert.AreSame(innerException, ex.InnerException);
        }

        public static void CheckSerialization<T>()
            where T : Exception
        {
            var innerException = new Exception(m_InnerMessage);
            T ex = (T)Activator.CreateInstance(typeof(T), m_Message, innerException);

            var deserialized = Duplicate(ex);
            Assert.NotNull(deserialized);
            Assert.AreEqual(m_Message, deserialized.Message);
            Assert.NotNull(deserialized.InnerException);
            Assert.AreEqual(m_InnerMessage, deserialized.InnerException.Message);
        }

        /// <summary> Creates a clone of an object using serialization-deserialization. </summary>
        public static T Duplicate<T>(T obj)
        {
            if (ReferenceEquals(obj, null)) throw new ArgumentNullException("obj");

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                ms.Seek(0, 0);
                var result = (T)bf.Deserialize(ms);
                return result;
            }
        }
    }
}
