using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleReflection.Interfaces
{
    public interface IDeserializer
    {
        /// <summary>
        /// Десериализует строку в объект указанного типа.
        /// </summary>
        /// <param name="text">Строка для десериализации</param>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <returns>Объект десериализованного типа или null при ошибке</returns>
        T? Deserialize<T>(string text) where T : class;
    }
}