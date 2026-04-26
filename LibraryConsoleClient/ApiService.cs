using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using LibraryConsoleClient.Models;

namespace LibraryConsoleClient;

// ── 1. Собственный тип делегата ──────────────────────────────────────────────
// Вызывается после каждого HTTP-запроса.
public delegate void OnRequestCompleted(string endpoint, int statusCode, long elapsedMs);

public class ApiService
{
    private readonly HttpClient _http = new();
    private const string Base = "http://localhost:5001/api";

    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    // ── 2. Многоадресный делегат: к нему подключаются 2 обработчика ──────────
    public OnRequestCompleted? RequestCompleted;

    // Вспомогательный метод: выполняет запрос, замеряет время, поднимает делегат
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
    {
        var sw = Stopwatch.StartNew();
        var response = await _http.SendAsync(req);
        sw.Stop();
        RequestCompleted?.Invoke(req.RequestUri!.PathAndQuery,
                                 (int)response.StatusCode,
                                 sw.ElapsedMilliseconds);
        return response;
    }

    // ── 3. Func<T, TResult> — получение одного объекта по id ─────────────────
    public Func<int, Task<Book?>> GetBook => async id =>
    {
        var resp = await SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{Base}/Books/{id}"));
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<Book>(_json);
    };

    // ── 3. Action<T> — вывод списка (не возвращает результат напрямую,
    //       передаёт список в callback)
    public async Task<List<Book>> GetBooks()
    {
        var resp = await SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{Base}/Books"));
        if (!resp.IsSuccessStatusCode) return new();
        return await resp.Content.ReadFromJsonAsync<List<Book>>(_json) ?? new();
    }

    // CRUD-операции через Action/Func ─────────────────────────────────────────

    // Func<Book, Task<Book?>> — создание книги
    public Func<Book, Task<Book?>> CreateBook => async book =>
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"{Base}/Books")
        {
            Content = JsonContent.Create(book)
        };
        var resp = await SendAsync(req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<Book>(_json);
    };

    // Action<int, Book> — обновление (возвращает bool через Func)
    public Func<int, Book, Task<bool>> UpdateBook => async (id, book) =>
    {
        book.Id = id;
        var req = new HttpRequestMessage(HttpMethod.Put, $"{Base}/Books/{id}")
        {
            Content = JsonContent.Create(book)
        };
        var resp = await SendAsync(req);
        return resp.IsSuccessStatusCode;
    };

    // Action<int> — удаление
    public Func<int, Task<bool>> DeleteBook => async id =>
    {
        var resp = await SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"{Base}/Books/{id}"));
        return resp.IsSuccessStatusCode;
    };
}
