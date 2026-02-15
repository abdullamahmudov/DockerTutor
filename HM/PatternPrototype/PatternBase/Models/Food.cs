using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PatternBase.Interfaces;

namespace PatternBase.Models
{
    public class Food : AProduct, IMyCloneable<Food>
    {
        public Food(long id, string name, decimal price) : base(id, name, price)
        {
        }
        /// <summary>
        /// Поверхностное клонирование реализацией интерфейса IMyClonable
        /// </summary>
        /// <returns></returns>
        public new Food Clone()
        {
            return base.Clone() as Food ?? throw new Exception();
        }
    }
}