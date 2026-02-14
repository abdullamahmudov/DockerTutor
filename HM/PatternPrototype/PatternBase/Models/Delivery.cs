using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatternBase.Models
{
    public class Delivery : CService
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public override decimal Price => Products.Sum(p => p.DeliveryPrice);
        public Delivery(long id, string name, decimal price) : base(id, name, price)
        {
        }

        public override object Clone()
        {
            var clone = new Delivery(this.Id, this._name, this.Price);
            clone.Products.AddRange(this.Products);
            return clone;
        }
    }
}