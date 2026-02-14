using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatternBase.Interfaces
{
    public interface IEntity
    {
        long Id { get; }
        string Name { get; }
        decimal Price { get; }
    }
}