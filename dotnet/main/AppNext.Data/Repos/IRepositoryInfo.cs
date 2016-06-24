using System;

namespace AppBoot.Repos
{
    /// <summary> Represents the persistence information of a repository. </summary>
    public interface IRepositoryInfo
    {
        /// <summary> Gets the entity type. </summary>
        Type EntityType { get; }

        /// <summary> Gets the primary key type. </summary>
        Type KeyType { get; }
    }
}