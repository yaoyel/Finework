using System;
using System.Collections.Generic;
using System.Linq;

namespace AppBoot.Security.Passwords
{
    public class PasswordAlgorithmRegistry
    {
        private readonly IDictionary<string, Lazy<IPasswordAlgorithm>> m_Registry = new Dictionary<String, Lazy<IPasswordAlgorithm>>();

        public void Register(String name, Func<IPasswordAlgorithm> algorithm)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("name is null or empty.", "name");
            if (algorithm == null) throw new ArgumentNullException("algorithm");

            m_Registry.Add(name, new Lazy<IPasswordAlgorithm>(algorithm));
        }

        public void Unregistry(String name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("name is null or empty.", "name");

            m_Registry.Remove(name);
        }

        public IPasswordAlgorithm Lookup(String name, bool throwIfNotFound)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("name is null or empty.", "name");

            Lazy<IPasswordAlgorithm> result;
            if (m_Registry.TryGetValue(name, out result))
            {
                return result.Value;
            }
            if (!(throwIfNotFound))
            {
                return null;
            }
            throw new ArgumentException(String.Format("The algorithm [{0}] not found.", name));
        }

        public String[] GetNames()
        {
            return m_Registry.Keys.ToArray();
        }
    }
}