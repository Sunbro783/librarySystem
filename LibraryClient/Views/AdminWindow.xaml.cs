using System.Windows;
using LibraryClient.Models;
using LibraryClient.Services;

namespace LibraryClient.Views;

public partial class AdminWindow : Window
{
    private readonly ApiService _api = new ApiService();

    public AdminWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAll();
    }

    private async Task LoadAll()
    {
        await LoadBooks();
        await LoadReaders();
        await LoadLoans();
    }

    // ── BOOKS ──────────────────────────────────────────
    private async Task LoadBooks()
    {
        try { BooksGrid.ItemsSource = await _api.GetBooks(); }
        catch { MessageBox.Show("Не удалось загрузить книги. Проверьте что API запущен.", "Ошибка"); }
    }

    private async void LoadBooks_Click(object sender, RoutedEventArgs e) => await LoadBooks();

    private async void AddBook_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BookTitle.Text) || string.IsNullOrWhiteSpace(BookAuthor.Text))
        {
            MessageBox.Show("Заполните хотя бы Название и Автора.", "Ошибка");
            return;
        }

        var book = new Book
        {
            Title = BookTitle.Text,
            Author = BookAuthor.Text,
            ISBN = BookISBN.Text,
            PublicationYear = int.TryParse(BookYear.Text, out var y) ? y : 0,
            Genre = BookGenre.Text,
            TotalCopies = int.TryParse(BookTotal.Text, out var t) ? t : 1,
            AvailableCopies = int.TryParse(BookAvail.Text, out var a) ? a : 1
        };

        var ok = await _api.AddBook(book);
        if (ok)
        {
            BookTitle.Text = BookAuthor.Text = BookISBN.Text =
            BookYear.Text = BookGenre.Text = BookTotal.Text = BookAvail.Text = "";
            await LoadBooks();
            MessageBox.Show("Книга добавлена!", "Успех");
        }
        else MessageBox.Show("Не удалось добавить книгу.", "Ошибка");
    }

    private async void DeleteBook_Click(object sender, RoutedEventArgs e)
    {
        if (BooksGrid.SelectedItem is not Book book) { MessageBox.Show("Выберите книгу."); return; }
        if (MessageBox.Show($"Удалить «{book.Title}»?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

        var ok = await _api.DeleteBook(book.Id);
        if (ok) await LoadBooks();
        else MessageBox.Show("Не удалось удалить книгу.", "Ошибка");
    }

    // ── READERS ────────────────────────────────────────
    private async Task LoadReaders()
    {
        try { ReadersGrid.ItemsSource = await _api.GetReaders(); }
        catch { MessageBox.Show("Не удалось загрузить читателей.", "Ошибка"); }
    }

    private async void LoadReaders_Click(object sender, RoutedEventArgs e) => await LoadReaders();

    private async void AddReader_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ReaderName.Text))
        {
            MessageBox.Show("Введите ФИО читателя.", "Ошибка");
            return;
        }

        var reader = new Reader
        {
            FullName = ReaderName.Text,
            Email = ReaderEmail.Text,
            Phone = ReaderPhone.Text
        };

        var ok = await _api.AddReader(reader);
        if (ok)
        {
            ReaderName.Text = ReaderEmail.Text = ReaderPhone.Text = "";
            await LoadReaders();
            MessageBox.Show("Читатель добавлен!", "Успех");
        }
        else MessageBox.Show("Не удалось добавить читателя.", "Ошибка");
    }

    private async void DeleteReader_Click(object sender, RoutedEventArgs e)
    {
        if (ReadersGrid.SelectedItem is not Reader reader) { MessageBox.Show("Выберите читателя."); return; }
        if (MessageBox.Show($"Удалить читателя «{reader.FullName}»?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

        var ok = await _api.DeleteReader(reader.Id);
        if (ok) await LoadReaders();
        else MessageBox.Show("Не удалось удалить читателя.", "Ошибка");
    }

    // ── LOANS ──────────────────────────────────────────
    private async Task LoadLoans()
    {
        try { LoansGrid.ItemsSource = await _api.GetLoans(); }
        catch { MessageBox.Show("Не удалось загрузить выдачи.", "Ошибка"); }
    }

    private async void LoadLoans_Click(object sender, RoutedEventArgs e) => await LoadLoans();
}
