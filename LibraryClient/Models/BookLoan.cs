namespace LibraryClient.Models;

public class BookLoan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public Book? Book { get; set; }
    public Reader? Reader { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public string BookTitle => Book?.Title ?? $"Книга #{BookId}";
    public string ReaderName => Reader?.FullName ?? $"Читатель #{ReaderId}";
    public string Status => ReturnDate.HasValue
        ? "✅ Возвращена"
        : (DateTime.UtcNow > DueDate ? "⚠️ Просрочена" : "📖 На руках");
}
