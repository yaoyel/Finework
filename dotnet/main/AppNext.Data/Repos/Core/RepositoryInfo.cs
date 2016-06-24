using System;

namespace AppBoot.Repos.Core
{
    /// <summary> Represents the persistence information of a repository. </summary>
    public class RepositoryInfo : IRepositoryInfo
    {
        public RepositoryInfo(Type entityType, Type keyType)
        {
            if (entityType == null) throw new ArgumentNullException("entityType");
            if (keyType == null) throw new ArgumentNullException("keyType");

            this.EntityType = entityType;
            this.KeyType = keyType;
        }

        /// <summary> Gets the entity type. </summary>
        /// <seealso cref="IRepositoryInfo.EntityType"/>
        public Type EntityType { get; private set; }

        /// <summary> Gets the primary key type. </summary>
        /// <seealso cref="IRepositoryInfo.KeyType"/>
        public Type KeyType { get; private set; }
    }
}