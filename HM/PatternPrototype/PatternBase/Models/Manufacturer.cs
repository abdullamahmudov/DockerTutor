using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PatternBase.Interfaces;

namespace PatternBase.Models
{
    public class Manufacturer : IMyCloneable<Manufacturer>
    {
        public long Id { get; private set; }

        public string Name { get; private set; }

        public Manufacturer(long id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Глубокое клонирование реализацией интерфейса IMyClonable
        /// </summary>
        /// <returns></returns>
        public Manufacturer Clone()
        {
            return new Manufacturer(Id, Name);
        }
    }
}