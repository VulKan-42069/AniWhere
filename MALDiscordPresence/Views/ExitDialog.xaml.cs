using System.Windows;

namespace MALDiscordPresence.Views;

public enum ExitAction
{
    Cancel,
    Minimize,
    Exit
}

public partial class ExitDialog : Window
{
    public ExitAction SelectedAction { get; private set; } = ExitAction.Cancel;

    public ExitDialog()
    {
        InitializeComponent();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        SelectedAction = ExitAction.Minimize;
        DialogResult = true;
        Close();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        SelectedAction = ExitAction.Exit;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        SelectedAction = ExitAction.Cancel;
        DialogResult = false;
        Close();
    }
}
