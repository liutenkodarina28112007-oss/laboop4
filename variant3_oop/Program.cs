namespace Variant3OOP;

// Делегат для пошуку індексу першого входження символу в рядок.
public delegate int FindCharacterDelegate(string text, char character);

// Аргументи події переповнення стеку.
public sealed class StackOverflowEventArgs : EventArgs
{
    public int Capacity { get; }
    public int Count { get; }
    public object? AttemptedItem { get; }

    public StackOverflowEventArgs(int capacity, int count, object? attemptedItem)
    {
        Capacity = capacity;
        Count = count;
        AttemptedItem = attemptedItem;
    }
}

// Стек обмеженої місткості. Клас генерує подію, якщо стек заповнений.
public sealed class BoundedStack<T>
{
    private readonly Stack<T> _items = new();

    public int Capacity { get; }
    public int Count => _items.Count;
    public bool IsEmpty => _items.Count == 0;

    public event EventHandler<StackOverflowEventArgs>? Overflow;

    public BoundedStack(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Місткість має бути більшою за нуль.");

        Capacity = capacity;
    }

    public bool Push(T item)
    {
        if (_items.Count >= Capacity)
        {
            OnOverflow(item);
            return false;
        }

        _items.Push(item);
        Console.WriteLine($"Елемент '{item}' додано. Кількість елементів: {Count}/{Capacity}.");
        return true;
    }

    public bool TryPop(out T? item)
    {
        if (_items.Count == 0)
        {
            item = default;
            Console.WriteLine("Стек порожній — видаляти нічого.");
            return false;
        }

        item = _items.Pop();
        Console.WriteLine($"Видалено елемент: '{item}'. Кількість елементів: {Count}/{Capacity}.");
        return true;
    }

    private void OnOverflow(T attemptedItem)
    {
        Overflow?.Invoke(this, new StackOverflowEventArgs(Capacity, Count, attemptedItem));
    }
}

// Окремий клас із методом-обробником події.
public sealed class StackEventHandler
{
    public void HandleOverflow(object? sender, StackOverflowEventArgs e)
    {
        Console.WriteLine("\n!!! ПОДІЯ: ПЕРЕПОВНЕННЯ СТЕКУ !!!");
        Console.WriteLine($"Місткість: {e.Capacity}; поточна кількість: {e.Count}.");
        Console.WriteLine($"Не вдалося додати елемент: '{e.AttemptedItem}'.");
    }
}

internal static class Program
{
    private static int FindFirstCharacter(string text, char character)
    {
        return text.IndexOf(character);
    }

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("ЛАБОРАТОРНА РОБОТА 3.4 — ВАРІАНТ 3");
        Console.WriteLine("Делегати, події та лямбда-вирази\n");

        DemonstrateDelegates();
        Console.WriteLine("\n" + new string('-', 55) + "\n");
        DemonstrateStackAndEvent();

        Console.WriteLine("\nРоботу завершено.");
    }

    private static void DemonstrateDelegates()
    {
        Console.WriteLine("1. ПОШУК ПЕРШОГО ВХОДЖЕННЯ СИМВОЛУ ЧЕРЕЗ ДЕЛЕГАТ");
        Console.Write("Введіть рядок: ");
        string text = Console.ReadLine() ?? string.Empty;
        Console.Write("Введіть символ для пошуку: ");
        string input = Console.ReadLine() ?? string.Empty;

        if (input.Length == 0)
        {
            Console.WriteLine("Символ не введено. Використано символ 'а'.");
            input = "а";
        }
        char character = input[0];

        // Звичайний метод.
        FindCharacterDelegate find = FindFirstCharacter;
        PrintSearchResult("Звичайний метод", find(text, character), character);

        // Анонімний метод.
        FindCharacterDelegate anonymousFind = delegate (string source, char target)
        {
            return source.IndexOf(target);
        };
        PrintSearchResult("Анонімний метод", anonymousFind(text, character), character);

        // Лямбда-вираз.
        FindCharacterDelegate lambdaFind = (source, target) => source.IndexOf(target);
        PrintSearchResult("Лямбда-вираз", lambdaFind(text, character), character);
    }

    private static void PrintSearchResult(string methodName, int index, char character)
    {
        string result = index >= 0
            ? $"символ '{character}' знайдено за індексом {index} (позиція {index + 1})"
            : $"символ '{character}' у рядку не знайдено";
        Console.WriteLine($"{methodName}: {result}.");
    }

    private static void DemonstrateStackAndEvent()
    {
        Console.WriteLine("2. СТЕК ОБМЕЖЕНОЇ МІСТКОСТІ ТА ПОДІЯ ПЕРЕПОВНЕННЯ");
        var stack = new BoundedStack<string>(3);
        var handler = new StackEventHandler();

        // Підписка на подію.
        stack.Overflow += handler.HandleOverflow;

        stack.Push("Перший");
        stack.Push("Другий");
        stack.Push("Третій");
        stack.Push("Четвертий"); // Тут генерується подія переповнення.

        Console.WriteLine("\nВидалення елемента зі стеку:");
        stack.TryPop(out _);

        Console.WriteLine("\nПовторне додавання після звільнення місця:");
        stack.Push("Четвертий");

        // Скасування підписки на подію.
        stack.Overflow -= handler.HandleOverflow;
        Console.WriteLine("\nПідписку обробника на подію скасовано.");
    }
}
