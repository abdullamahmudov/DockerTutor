using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatternBase.Models
{
    public class Furniture : Product
    {
        public decimal InstalationPrice { get; set; }
        public Furniture(string name) : base(name)
        {
        }
    }
}