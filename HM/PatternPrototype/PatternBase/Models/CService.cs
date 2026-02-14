using System.Data.Common;
using PatternBase.Interfaces;

namespace PatternBase.Models
{
    public abstract class CService : IEntity, ICloneable
    {
        public long Id { get; protected set; }
        protected readonly string _name;
        public string Name => _name;
        public virtual decimal Price { get; protected set; }
        public CService(long id, string name, decimal price)
        {
            Id = id;
            _name = name;
            Price = price;
        }

        public abstract object Clone();
    }
}