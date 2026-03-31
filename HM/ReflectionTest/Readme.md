### Вывод выполнения программы в консоль
```textplain
OS Description: Darwin 23.5.0 Darwin Kernel Version 23.5.0: Wed May  1 20:09:52 PDT 2024; root:xnu-10063.121.3~5/RELEASE_X86_64
OS Version (Environment.OSVersion): Unix 14.5.0
OS Architecture: X64
Process Architecture: X64
Machine Name: MacBook-Pro-User
Processor Count: 12
64-bit OS: True

Serializer: ConsoleReflection.Implementation.CustomCSVSerializer
Результат сериализации: 

i1;i2;i3;i4;i5
1;2;3;4;5

Время вывода результата: 0 ms

-------------------------------------------------
|    №     |  Количество итераций  |     время      |
-------------------------------------------------
|    1     |          100          |      0 ms      |
|    2     |         1000          |      3 ms      |
|    3     |         10000         |     28 ms      |
|    4     |        100000         |     323 ms     |
-------------------------------------------------

Serializer: ConsoleReflection.Implementation.NewtonsoftJsonSerializer
Результат сериализации: 

{"i1":1,"i2":2,"i3":3,"i4":4,"i5":5}

Время вывода результата: 0 ms

-------------------------------------------------
|    №     |  Количество итераций  |     время      |
-------------------------------------------------
|    1     |          100          |      0 ms      |
|    2     |         1000          |      3 ms      |
|    3     |         10000         |     35 ms      |
|    4     |        100000         |     369 ms     |
-------------------------------------------------

Deserializer: ConsoleReflection.Implementation.CustomCSVDeserializer

-------------------------------------------------
|    №     |  Количество итераций  |     время      |
-------------------------------------------------
|    1     |          100          |     369 ms     |
|    2     |         1000          |      4 ms      |
|    3     |         10000         |     48 ms      |
|    4     |        100000         |     422 ms     |
-------------------------------------------------

Deserializer: ConsoleReflection.Implementation.NewtonsoftJsonDeserializer

-------------------------------------------------
|    №     |  Количество итераций  |     время      |
-------------------------------------------------
|    1     |          100          |      0 ms      |
|    2     |         1000          |      6 ms      |
|    3     |         10000         |     68 ms      |
|    4     |        100000         |     414 ms     |
-------------------------------------------------
```

Сериализуемый класс: class F { int i1, i2, i3, i4, i5;}  
код сериализации-десериализации:  
Код сериализации  
```csharp
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
```
Код десериализации
```csharp
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
```
количество замеров: 1000 итераций  
мой рефлекшен:  
Время на сериализацию = 3 мс  
Время на десериализацию = 4 мс  
стандартный механизм (NewtonsoftJson):  
Время на сериализацию = 3 мс  
Время на десериализацию = 6 мс  

количество замеров: 10000 итераций  
мой рефлекшен:  
Время на сериализацию = 28 мс  
Время на десериализацию = 48 мс  
стандартный механизм (NewtonsoftJson):  
Время на сериализацию = 35 мс  
Время на десериализацию = 68 мс  