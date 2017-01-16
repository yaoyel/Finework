using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Common
{
    /// <summary> Represents a persistent entity. </summary>
    /// <typeparam name="TKey"> The type of primary key. </typeparam>
    public abstract class EntityBase<TKey>
    {
        /// <summary> Creates a new instance. </summary>
        protected EntityBase()
        {
        }

        /// <summary> Creates a new instance. </summary>
        /// <param name="id"> The primary key value. </param>
        protected EntityBase(TKey id)
        {
            this.m_Id = id;
        }

        #region Id

        private TKey m_Id;

        /// <summary> Gets or sets the primary key. </summary>
        public virtual TKey Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        #endregion
    } 
}
