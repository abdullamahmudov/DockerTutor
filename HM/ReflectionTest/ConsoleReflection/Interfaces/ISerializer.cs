using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleReflection.Interfaces
{
    public interface ISerializer
    {
        /// <summary>
        /// Сериализует объект указанного типа в строку.
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns>Строковое представление объекта</returns>
        string Serialize<T>(T obj);
    }
}