using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleReflection.Interfaces;

namespace ConsoleReflection.Implementation
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        /// <summary>
        /// Сериализует объект в JSON формат с использованием библиотеки Newtonsoft.Json.
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns>Строка в формате JSON</returns>
        public string Serialize<T>(T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
    }
}