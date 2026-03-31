using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConsoleReflection.Interfaces;

namespace ConsoleReflection.Implementation
{
    public class CustomCSVDeserializer : IDeserializer
    {
        /// <summary>
        /// Десериализует строку в формате CSV в объект указанного типа.
        /// </summary>
        /// <param name="text">Строка в формате CSV</param>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <returns>Объект десериализованного типа или null если десериализация не удалась</returns>
        public T? Deserialize<T>(string text) where T : class
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var fields = type.GetFields();
            var rows = text.Split("\n");
            if (rows.Length != 2)
                return null;

            var textFields = rows[0].Split(";");
            var textValues = rows[1].Split(";");
            if (textFields.Length != textValues.Length)
                return null;

            var usedFields = new HashSet<int>();
            var selectedProperties = FillProperyes(properties, textFields, textValues, usedFields);
            var selectedFields = FillFields(fields, textFields, textValues, usedFields);
            var obj = Activator.CreateInstance(type);
            foreach (var kvp in selectedProperties)
            {
                kvp.Key.SetValue(obj, kvp.Value);
            }
            foreach (var kvp in selectedFields)
            {
                kvp.Key.SetValue(obj, kvp.Value);
            }
            return obj as T;
        }

        /// <summary>
        /// Заполняет поля объекта значениями из CSV.
        /// </summary>
        /// <param name="fields">Массив информации о полях</param>
        /// <param name="textColumns">Названия колонок из CSV</param>
        /// <param name="textValues">Значения из CSV</param>
        /// <param name="userd">Множество уже использованных индексов</param>
        /// <returns>Словарь с информацией о полях и их значениями</returns>
        private Dictionary<FieldInfo, object> FillFields(FieldInfo[] fields, string[] textColumns, string[] textValues, HashSet<int> userd)
        {
            var selectedFieldData = new Dictionary<FieldInfo, object>();
            foreach (var field in fields)
            {
                var type = field.FieldType;
                for (int i = 0; i < textColumns.Length; i++)
                {
                    if (userd.Contains(i))
                        continue;

                    if (!TryConvertToType(type, textValues[i], out var outValue) || outValue is null)
                        continue;

                    selectedFieldData.Add(field, outValue);
                    userd.Add(i);
                    break;
                }
            }
            return selectedFieldData;
        }

        /// <summary>
        /// Заполняет свойства объекта значениями из CSV.
        /// </summary>
        /// <param name="properties">Массив информации о свойствах</param>
        /// <param name="textColumns">Названия колонок из CSV</param>
        /// <param name="textValues">Значения из CSV</param>
        /// <param name="userd">Множество уже использованных индексов</param>
        /// <returns>Словарь с информацией о свойствах и их значениями</returns>
        private Dictionary<PropertyInfo, object> FillProperyes(PropertyInfo[] properties, string[] textColumns, string[] textValues, HashSet<int> userd)
        {
            var selectedPropertyData = new Dictionary<PropertyInfo, object>();
            foreach (var property in properties)
            {
                var type = property.PropertyType;
                for (int i = 0; i < textColumns.Length; i++)
                {
                    if (userd.Contains(i))
                        continue;

                    if (!TryConvertToType(type, textValues[i], out var outValue) || outValue is null)
                        continue;

                    selectedPropertyData.Add(property, outValue);
                    userd.Add(i);
                    break;
                }
            }
            return selectedPropertyData;
        }

        /// <summary>
        /// Пытается преобразовать строковое значение в указанный тип.
        /// </summary>
        /// <param name="type">Целевой тип</param>
        /// <param name="value">Строковое значение для преобразования</param>
        /// <param name="outValue">Преобразованное значение</param>
        /// <returns>true если преобразование успешно, false в противном случае</returns>
        private bool TryConvertToType(Type type, string value, out object? outValue)
        {
            outValue = null;
            if (type.IsSubclassOf(typeof(IEnumerable)))
            {
                var genericTypes = type.GenericTypeArguments;
                if (genericTypes.Count() != 1)
                {
                    return false;
                }
                var elementType = genericTypes.First();
                if (value[0] != '[' || value.Last() != ']')
                {
                    return false;
                }

                var tvalue = value.Substring(1, value.Length - 2);
                if (string.IsNullOrWhiteSpace(tvalue) || string.IsNullOrEmpty(tvalue))
                {
                    outValue = Activator.CreateInstance(type);
                    return true;
                }

                var values = tvalue.Split("|");
                if (value.Length == 0)
                {
                    outValue = Activator.CreateInstance(type);
                    return true;
                }
                outValue = Activator.CreateInstance(type);
                var method = type.GetMethod("Add");
                if(method is null)
                {
                    return false;
                }
                foreach (var fvalue in values)
                {
                    var cvalue = Convert.ChangeType(fvalue, elementType);
                    method.Invoke(outValue, [cvalue]);
                }
                return true;
            }

            outValue = Convert.ChangeType(value, type);
            return true;
        }
    }
}