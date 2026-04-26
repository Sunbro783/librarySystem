using System.Net.Http;
using System.Text;
using System.Text.Json;
using LibraryClient.Models;

namespace LibraryClient.Services;

public class ApiService
{
    private readonly HttpClient _http = new HttpClient();
    private const string BASE = "http://localhost:5001/api";
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<Book>> GetBooks()
    {
        var json = await _http.GetStringAsync($"{BASE}/Books");
        return JsonSerializer.Deserialize<List<Book>>(json, _options) ?? new List<Book>();
    }

    public async Task<List<Reader>> GetReaders()
    {
        var json = await _http.GetStringAsync($"{BASE}/Readers");
        return JsonSerializer.Deserialize<List<Reader>>(json, _options) ?? new List<Reader>();
    }

    public async Task<List<BookLoan>> GetLoans()
    {
        var json = await _http.GetStringAsync($"{BASE}/BookLoans");
        return JsonSerializer.Deserialize<List<BookLoan>>(json, _options) ?? new List<BookLoan>();
    }

    public async Task<bool> AddBook(Book book)
    {
        var content = new StringContent(JsonSerializer.Serialize(book), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{BASE}/Books", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBook(int id)
    {
        var response = await _http.DeleteAsync($"{BASE}/Books/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddReader(Reader reader)
    {
        var content = new StringContent(JsonSerializer.Serialize(reader), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{BASE}/Readers", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteReader(int id)
    {
        var response = await _http.DeleteAsync($"{BASE}/Readers/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LoanBook(int bookId, int readerId)
    {
        var response = await _http.PostAsync($"{BASE}/BookLoans/loan?bookId={bookId}&readerId={readerId}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReturnBook(int loanId)
    {
        var response = await _http.PostAsync($"{BASE}/BookLoans/return?loanId={loanId}", null);
        return response.IsSuccessStatusCode;
    }
}
