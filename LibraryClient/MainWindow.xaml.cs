using System.Windows;

namespace LibraryClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Admin_Click(object sender, RoutedEventArgs e)
    {
        new Views.AdminWindow().Show();
        Close();
    }

    private void User_Click(object sender, RoutedEventArgs e)
    {
        new Views.UserWindow().Show();
        Close();
    }
}