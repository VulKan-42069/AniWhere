using System.Windows;

namespace MALDiscordPresence.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog(string message, string title = "CONFIRM ACTION")
    {
        InitializeComponent();
        MessageText.Text = message;
        HeaderStrip.Title = title;
    }

    private void Yes_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void No_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
