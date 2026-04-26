using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookLoansController : ControllerBase
{
    private readonly LibraryDbContext _context;
    private readonly IDatabase _redis;

з
    private static readonly Counter LoansTotal = Metrics
        .CreateCounter("library_loans_total", "Общее количество выдач книг");

з
    private static readonly Counter ReturnsTotal = Metrics
        .CreateCounter("library_returns_total", "Общее количество возвратов книг");

з
    private static readonly Counter LoanDeniedTotal = Metrics
        .CreateCounter("library_loan_denied_total", "Количество отказов в выдаче (нет доступных экземпляров)");

    public BookLoansController(LibraryDbContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _redis = redis.GetDatabase();
    }

    [HttpGet]
    public async Task<IEnumerable<BookLoan>> GetAll()
    {
        return await _context.BookLoans
            .Include(x => x.Book)
            .Include(x => x.Reader)
            .ToListAsync();
    }

з
    [HttpPost("loan")]
    public async Task<IActionResult> LoanBook(int bookId, int readerId)
    {
        var book = await _context.Books.FindAsync(bookId);

        if (book == null)
            return NotFound("Книга не найдена");

        if (book.AvailableCopies <= 0)
        {
            LoanDeniedTotal.Inc();
            return BadRequest("Нет доступных экземпляров");
        }

        var reader = await _context.Readers.FindAsync(readerId);

        if (reader == null)
            return NotFound("Читатель не найден");

        var loan = new BookLoan
        {
            BookId = bookId,
            ReaderId = readerId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        book.AvailableCopies--;

        _context.BookLoans.Add(loan);
        await _context.SaveChangesAsync();

    
        LoansTotal.Inc();

      
        try { await _redis.KeyDeleteAsync("books"); } catch { /* Redis недоступен — кэш устареет сам */ }

        return Ok("Книга выдана");
    }

    [HttpPost("return")]
    public async Task<IActionResult> ReturnBook(int loanId)
    {
        var loan = await _context.BookLoans.FindAsync(loanId);

        if (loan == null)
            return NotFound("Запись не найдена");

        if (loan.ReturnDate != null)
            return BadRequest("Книга уже возвращена");

        loan.ReturnDate = DateTime.UtcNow;

        var book = await _context.Books.FindAsync(loan.BookId);
        if (book != null)
        {
            book.AvailableCopies++;
        }

        await _context.SaveChangesAsync();

      
        ReturnsTotal.Inc();

  
        try { await _redis.KeyDeleteAsync("books"); } catch { /* Redis недоступен — кэш устареет сам */ }

        return Ok("Книга возвращена");
    }
}
