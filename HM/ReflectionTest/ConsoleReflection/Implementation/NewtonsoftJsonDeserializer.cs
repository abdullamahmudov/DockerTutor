using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleReflection.Interfaces;

namespace ConsoleReflection.Implementation
{
    public class NewtonsoftJsonDeserializer : IDeserializer
    {
        /// <summary>
        /// Десериализует JSON строку в объект указанного типа с использованием библиотеки Newtonsoft.Json.
        /// </summary>
        /// <param name="text">JSON строка для десериализации</param>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <returns>Объект десериализованного типа или null если произошла ошибка</returns>
        public T? Deserialize<T>(string text) where T: class
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(text);
            }
            catch
            {
                Console.WriteLine("Couldn't deserealize object!");
                return null;
            }
        }
    }
}