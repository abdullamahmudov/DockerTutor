using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatternBase.Interfaces
{
    public interface IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        long Id { get; }
        /// <summary>
        /// Наименование
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Цена
        /// </summary>
        decimal Price { get; }
    }
}