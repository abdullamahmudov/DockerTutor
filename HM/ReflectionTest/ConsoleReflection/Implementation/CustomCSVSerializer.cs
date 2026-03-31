using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ConsoleReflection.Interfaces;

namespace ConsoleReflection.Implementation
{
    public class CustomCSVSerializer : ISerializer
    {
        /// <summary>
        /// Сериализует объект в формат CSV.
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns>Строка в формате CSV с названиями полей и их значениями</returns>
        public string Serialize<T>(T obj)
        {
            if (obj is null)
                return "null";

            var type = obj.GetType();
            var properties = type.GetProperties();
            var map = new Dictionary<string, string>();
            for (int i = 0; i < properties.Length; i++)
            {
                var name = properties[i].Name;
                var value = VaueToString(properties[i].GetValue(obj));
                map.Add(name, value);
            }
            var fields = type.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var name = fields[i].Name;
                var value = VaueToString(fields[i].GetValue(obj));
                map.Add(name, value);
            }
            var result = string.Empty;
            result += string.Join(";", map.Keys);
            result += "\n";
            result += string.Join(";", map.Values);
            return result;
        }
        /// <summary>
        /// Преобразует значение в строку с учетом типа значения.
        /// </summary>
        /// <param name="value">Значение для преобразования</param>
        /// <returns>Строковое представление значения</returns>
        private string VaueToString(object? value)
        {
            if (value is null)
                return "null";

            switch (value)
            {
                case String strValue:
                    return strValue;
                case IEnumerable valueEnum:
                    var type = valueEnum.GetType();
                    var genericTypes = type.GenericTypeArguments;
                    if(genericTypes.Count() == 1)
                    {
                        var str = string.Empty;
                        ArrayList array = [.. valueEnum];
                        str +="[" + string.Join("|", array.ToArray()) + "]";
                        return str;
                    }
                    return valueEnum.ToString();
                default:
                    return value.ToString();
            }
        }
    }
}