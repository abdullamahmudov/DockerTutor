using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatternBase.Models
{
    public class Delivery : AService
    {
        /// <summary>
        /// Список продуктов
        /// </summary>
        /// <returns></returns>
        public List<(AProduct product, double count)> Products { get; set; } = new List<(AProduct product, double count)>();
        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; private set; }
        public Delivery(long id, string name, decimal price, string address) : base(id, name)
        {
            Price = price;
            Address = address;
        }

        /// <summary>
        /// Глубокое клонирование реализациией интерфейса IClonable.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = new Delivery(this.Id, this._name, Price, this.Address);
            foreach (var product in Products)
            {
                clone.Products.Add((product.product.Clone(), product.count));
            }
            return clone;
        }
    }
}