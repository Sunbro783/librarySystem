using LibraryConsoleClient;
using LibraryConsoleClient.Models;

// ── Обработчики для многоадресного делегата ───────────────────────────────────

// Обработчик 1: вывод в консоль
void ConsoleLogger(string endpoint, int status, long ms) =>
    Console.WriteLine($"  [HTTP {status}] {endpoint} ({ms} мс)");

// Обработчик 2: логирование в файл
string logPath = "requests.log";
void FileLogger(string endpoint, int status, long ms) =>
    File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} | {status} | {endpoint} | {ms} мс\n");

// ─────────────────────────────────────────────────────────────────────────────

var api = new ApiService();

// ── 2. Подключаем оба обработчика к многоадресному делегату (+=) ──────────────
api.RequestCompleted += ConsoleLogger;
api.RequestCompleted += FileLogger;

Console.WriteLine("=== Библиотечная система — консольный клиент ===\n");
Console.WriteLine("Обработчики: ConsoleLogger + FileLogger\n");

// ── Операция 1: создание книги ────────────────────────────────────────────────
Console.WriteLine("▶ [1] Создание книги...");
var created = await api.CreateBook(new Book
{
    Title = "Мастер и Маргарита",
    Author = "Михаил Булгаков",
    ISBN = "978-5-17-090000-0",
    PublicationYear = 1967,
    Genre = "Роман",
    TotalCopies = 3,
    AvailableCopies = 3
});

if (created != null)
    Console.WriteLine($"  Создана книга: Id={created.Id}, \"{created.Title}\"");
else
    Console.WriteLine("  Не удалось создать книгу (API недоступен?)");

Console.WriteLine();

// ── Операция 2: получение списка книг ─────────────────────────────────────────
Console.WriteLine("▶ [2] Получение списка книг...");
var books = await api.GetBooks();
Console.WriteLine($"  Книг в системе: {books.Count}");
foreach (var b in books.Take(3))
    Console.WriteLine($"    • [{b.Id}] {b.Title} — {b.Author}");

Console.WriteLine();

// ── Операция 3: получение книги по id (через Func<int, Task<Book?>>) ──────────
int targetId = created?.Id ?? 1;
Console.WriteLine($"▶ [3] Получение книги по Id={targetId}...");
var found = await api.GetBook(targetId);
Console.WriteLine(found != null
    ? $"  Найдена: \"{found.Title}\", автор: {found.Author}"
    : "  Книга не найдена.");

Console.WriteLine();

// ── 4. Динамическая отписка FileLogger после операции 3 ──────────────────────
Console.WriteLine("--- Отписываем FileLogger (-=) ---\n");
api.RequestCompleted -= FileLogger;

// ── Операция 4: обновление книги ──────────────────────────────────────────────
Console.WriteLine("▶ [4] Обновление книги (FileLogger уже не пишет в файл)...");
if (created != null)
{
    created.Genre = "Классика";
    var updated = await api.UpdateBook(created.Id, created);
    Console.WriteLine(updated ? "  Книга обновлена." : "  Не удалось обновить.");
}
else
{
    Console.WriteLine("  Пропуск (книга не была создана).");
}

Console.WriteLine();

// ── Операция 5: удаление книги ────────────────────────────────────────────────
Console.WriteLine("▶ [5] Удаление книги...");
if (created != null)
{
    var deleted = await api.DeleteBook(created.Id);
    Console.WriteLine(deleted ? "  Книга удалена." : "  Не удалось удалить.");
}
else
{
    Console.WriteLine("  Пропуск (книга не была создана).");
}

Console.WriteLine();
Console.WriteLine("=== Готово ===");
Console.WriteLine($"Лог первых трёх операций записан в файл: {Path.GetFullPath(logPath)}");
Console.WriteLine("Операции 4 и 5 — только в консоль (FileLogger был отписан).");
