using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatternBase.Models
{
    public class Installation : CService
    {
        public List<Furniture> Furnitures { get; set; } = new List<Furniture>();
        public override decimal Price => Furnitures.Sum(f => f.InstalationPrice);
        public Installation(long id, string name, decimal price) : base(id, name, price)
        {
        }

        public override object Clone()
        {
            var clone = new Installation(this.Id, this.Name, this.Price);
            clone.Furnitures.AddRange(this.Furnitures);
            return clone;
        }
    }
}