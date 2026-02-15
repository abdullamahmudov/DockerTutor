using System.Data.Common;
using PatternBase.Interfaces;

namespace PatternBase.Models
{
    public abstract class AService : IEntity, ICloneable
    {
        /// <inheritdoc/>
        public long Id { get; protected set; }
        protected readonly string _name;
        /// <inheritdoc/>
        public string Name => _name;
        /// <inheritdoc/>
        public virtual decimal Price { get; protected set; }
        public AService(long id, string name)
        {
            Id = id;
            _name = name;
        }

        public abstract object Clone();
    }
}