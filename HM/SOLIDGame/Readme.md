# SOLIDGame — Демонстрация SOLID принципов

Консольное приложение для угадывания чисел, разработанное с соблюдением всех **SOLID** принципов проектирования.

---

## 📦 Структура проекта

```
ConsoleGame/
├── Interfaces/
│   ├── IInputOutput.cs                    # Интерфейс для ввода/вывода
│   └── IRandomNumberGenerator.cs          # Интерфейс для генерации чисел
├── Implementations/
│   ├── ConsoleInputOutput.cs              # Реализация для консоли
│   ├── RandomNumberGenerator.cs           # Генератор случайных чисел
│   ├── GuessNumberGame.cs                 # Логика игры
│   └── RulesConfigurator.cs               # Конфигурация диапазона
└── Program.cs                             # Точка входа
```

---

## 🏛️ SOLID принципы в проекте

### **S** — Single Responsibility Principle
**Каждый класс отвечает за одну функцию**

| Класс | Ответственность |
|-------|-------------------|
| `RandomNumberGenerator` | Генерирует случайные числа |
| `ConsoleInputOutput` | Работает с консольным вводом/выводом |
| `GuessNumberGame` | Логика игры (сравнение, подсказки) |
| `RulesConfigurator` | Конфигурация диапазона чисел |

✅ **Преимущество:** Легко тестировать, поддерживать и расширять отдельные компоненты.

---

### **O** — Open/Closed Principle
**Открыт для расширения, закрыт для изменения**

Благодаря интерфейсам можно добавлять новые реализации **без изменения** существующего кода:

```csharp
// Текущая реализация
IInputOutput inputOutput = new ConsoleInputOutput();

// Можно легко добавить новые реализации:
// IInputOutput inputOutput = new FileInputOutput();
// IInputOutput inputOutput = new NetworkInputOutput();
```

✅ **Преимущество:** Расширяемость без риска сломать существующий код.

---

### **L** — Liskov Substitution Principle
**Подкласс безопасно заменяет базовый класс**

Класс `GuessNumberGame` получает интерфейсы в конструкторе и работает с абстракциями:

```csharp
public GuessNumberGame(
    IRandomNumberGenerator numberGenerator, 
    IInputOutput inputOutput, 
    int min, 
    int max)
{
    _numberGenerator = numberGenerator;
    _inputOutput = inputOutput;
    // ...
}
```

Любая реализация этих интерфейсов будет работать корректно, так как они соответствуют контракту.

✅ **Преимущество:** Гибкость в замене реализаций.

---

### **I** — Interface Segregation Principle
**Специфичные интерфейсы вместо универсальных**

Интерфейсы узкие и сфокусированные:

**IInputOutput.cs** — только 3 метода:
```csharp
/// <summary>
/// Интерфейс для ввода/вывода (ISP, DIP)
/// </summary>
public interface IInputOutput
{
    /// <summary>
    /// Написать сообщение в новой строке
    /// </summary>
    /// <param name="message"></param>
    void WriteLine(string message);
    
    /// <summary>
    /// Написать сообщение
    /// </summary>
    /// <param name="message"></param>
    void Write(string message);
    
    /// <summary>
    /// Прочитать сообщение
    /// </summary>
    /// <returns></returns>
    string ReadLine();
}
```

**IRandomNumberGenerator.cs** — 1 метод:
```csharp
/// <summary>
/// Интерфейс для генерации случайного числа (DIP)
/// </summary>
public interface IRandomNumberGenerator
{
    /// <summary>
    /// Генерация случайного числа
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    int Generate(int min, int max);
}
```

✅ **Преимущество:** Классы реализуют только нужные методы.

---

### **D** — Dependency Inversion Principle
**Зависимость от абстракций, не от конкретных классов**

В `Program.cs` используется **Dependency Injection**:

```csharp
static void Main(string[] args)
{
    // Создание зависимостей (DIP)
    IRandomNumberGenerator numberGenerator = new RandomNumberGenerator();
    IInputOutput inputOutput = new ConsoleInputOutput();
    var configurator = new RulesConfigurator(inputOutput);
    
    if(!configurator.Configure()) return;

    var game = new GuessNumberGame(
        numberGenerator, 
        inputOutput, 
        configurator.MinimalNumber, 
        configurator.MaximalNumber);
    
    game.Play();
}
```

Все классы зависят от интерфейсов, а не друг от друга.

✅ **Преимущество:** Слабая связанность, легко тестировать.

---

## 📚 Документация

Проект полностью документирован с использованием **XML-комментариев**:

- Все интерфейсы и классы имеют описания
- Методы документированы с параметрами и возвращаемыми значениями
- Используются `<inheritdoc/>` для наследования комментариев

Это обеспечивает:
- Автодополнение в IDE
- Генерацию документации (например, через DocFX)
- Лучшую читаемость кода

---

## 🎮 Как запустить

```bash
cd ConsoleGame
dotnet run
```

Или с передачей входных данных (Linux/Mac):

```bash
echo -e "1\n100\n50\n30\n25\n27\n26" | dotnet run
```

Для Windows PowerShell:

```powershell
@"
1
100
2
98
30
75
98
"@ | dotnet run
```

---

## 🧪 Примеры расширения

### Добавить логирование в файл:
```csharp
public class FileInputOutput : IInputOutput
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText("log.txt", message + Environment.NewLine);
    }
    // ...
}
```

### Использовать криптографически стойкий генератор:
```csharp
public class SecureRandomNumberGenerator : IRandomNumberGenerator
{
    private readonly RNGCryptoServiceProvider _rng = new();
    
    public int Generate(int min, int max)
    {
        // Реализация с использованием RNGCryptoServiceProvider
    }
}
```

---

## ✨ Итог

Этот проект демонстрирует, как SOLID принципы делают код:
- 🔧 **Гибким** — легко добавлять новые функции
- 🧪 **Тестируемым** — каждый компонент независим
- 📖 **Понятным** — каждый класс имеет одну ответственность
- 🛡️ **Надежным** — изменения не ломают существующий код
- 📚 **Документированным** — XML-комментарии для всех элементов
