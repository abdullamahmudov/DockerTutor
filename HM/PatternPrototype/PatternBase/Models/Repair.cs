using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatternBase.Models
{
    public class Repair : AService
    {
        /// <summary>
        /// Список мебели
        /// </summary>
        /// <returns></returns>
        public List<Furniture> Furnitures { get; private set; } = new List<Furniture>();
        /// <inheritdoc/>
        public override decimal Price => Furnitures.Sum(f => f.RepairPrice);
        public Repair(long id, string name) : base(id, name)
        {
        }

        /// <summary>
        /// Поверхностное клонирование реализациией интерфейса IClonable.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}