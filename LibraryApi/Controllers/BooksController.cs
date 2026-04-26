using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly LibraryDbContext _context;
    private readonly IDatabase _redis;

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public BooksController(LibraryDbContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _redis = redis.GetDatabase();
    }

    [HttpGet]
    public async Task<IEnumerable<Book>> GetAll()
    {
        try
        {
            var cache = await _redis.StringGetAsync("books");

            if (!cache.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<List<Book>>(cache.ToString(), _jsonOptions)
                       ?? new List<Book>();
            }
        }
        catch { /* Redis недоступен — читаем из БД */ }

        var books = await _context.Books.ToListAsync();

        try
        {
            // TTL 30 сек: страховка на случай если KeyDeleteAsync не сработает
            await _redis.StringSetAsync("books", JsonSerializer.Serialize(books),
                TimeSpan.FromSeconds(30));
        }
        catch { /* Redis недоступен — продолжаем без кэша */ }

        return books;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> Get(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound();

        return book;
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        try { await _redis.KeyDeleteAsync("books"); } catch { }

        return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Book updated)
    {
        if (id != updated.Id)
            return BadRequest();

        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        try { await _redis.KeyDeleteAsync("books"); } catch { }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        try { await _redis.KeyDeleteAsync("books"); } catch { }

        return NoContent();
    }
}
