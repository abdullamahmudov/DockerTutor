using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PatternBase.Interfaces;

namespace PatternBase.Models
{
    public class Furniture : AProduct, IMyCloneable<Furniture>
    {
        public Manufacturer Manufacturer { get; private set; }
        public decimal InstallationPrice { get; private set; }
        public decimal RepairPrice { get; private set; }
        public Furniture(long id, string name, decimal price, decimal installationPrice, decimal repairPrice, Manufacturer manufacturer) : base(id, name, price)
        {
            Manufacturer = manufacturer;
            InstallationPrice = installationPrice;
            RepairPrice = repairPrice;
        }

        /// <summary>
        /// Глубокое копирование реализацией интерфейса IMyClonable
        /// </summary>
        /// <returns></returns>
        public new Furniture Clone()
        {
            return new Furniture(Id, Name, Price, InstallationPrice, RepairPrice, Manufacturer.Clone());
        }
    }
}