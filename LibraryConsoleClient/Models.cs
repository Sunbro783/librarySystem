namespace LibraryConsoleClient.Models;

public class Book
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string? Genre { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}

public class Reader
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime RegistrationDate { get; set; }
}
