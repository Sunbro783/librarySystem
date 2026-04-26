using System.Windows;
using LibraryClient.Services;

namespace LibraryClient.Views;

public partial class UserWindow : Window
{
    private readonly ApiService _api = new ApiService();

    public UserWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadBooks();
    }

    private async Task LoadBooks()
    {
        try { BooksGrid.ItemsSource = await _api.GetBooks(); }
        catch { MessageBox.Show("Не удалось загрузить книги.\nПроверьте что API запущен.", "Ошибка"); }
    }

    private async void LoadBooks_Click(object sender, RoutedEventArgs e) => await LoadBooks();

    private async void LoadMyLoans_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(ReaderIdBox.Text, out var readerId))
        {
            MessageBox.Show("Введите ваш ID читателя.", "Ошибка");
            return;
        }
        await LoadMyLoans(readerId);
    }

    private async Task LoadMyLoans(int readerId)
    {
        try
        {
            var all = await _api.GetLoans();
            MyLoansGrid.ItemsSource = all.Where(l => l.ReaderId == readerId).ToList();
        }
        catch { MessageBox.Show("Не удалось загрузить выдачи.", "Ошибка"); }
    }

    private async void Loan_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(BookIdBox.Text, out var bookId))
        { MessageBox.Show("Введите корректный ID книги.", "Ошибка"); return; }

        if (!int.TryParse(ReaderIdBox.Text, out var readerId))
        { MessageBox.Show("Введите ваш ID читателя.", "Ошибка"); return; }

        var ok = await _api.LoanBook(bookId, readerId);
        if (ok)
        {
            BookIdBox.Text = "";
            await LoadBooks();
            await LoadMyLoans(readerId);
            MessageBox.Show("Книга успешно выдана!\nСрок возврата — 14 дней.", "Успех");
        }
        else
            MessageBox.Show("Не удалось выдать книгу.\nВозможно книга недоступна или ID неверный.", "Ошибка");
    }

    private async void Return_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(LoanIdBox.Text, out var loanId))
        { MessageBox.Show("Введите корректный ID выдачи.", "Ошибка"); return; }

        var ok = await _api.ReturnBook(loanId);
        if (ok)
        {
            LoanIdBox.Text = "";
            await LoadBooks();
            if (int.TryParse(ReaderIdBox.Text, out var readerId))
                await LoadMyLoans(readerId);
            MessageBox.Show("Книга успешно возвращена!", "Успех");
        }
        else
            MessageBox.Show("Не удалось вернуть книгу.\nВозможно она уже возвращена или ID неверный.", "Ошибка");
    }
}
