using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Remotely.Desktop.XPlat.ViewModels;
using Remotely.Desktop.XPlat.Views;

namespace Remotely.Desktop.XPlat.Views
{
    public class HostNamePrompt : Window
    {
        public HostNamePrompt()
        {
            Owner = MainWindow.Current;
            InitializeComponent();
        }

        public HostNamePromptViewModel ViewModel => DataContext as HostNamePromptViewModel;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
#if !DEBUG
            if (!string.IsNullOrEmpty(ViewModel.Host))
            {
                this.FindControl<TextBlock>("HostLabel").IsVisible = false;
                this.FindControl<TextBox>("HostTextBox").IsVisible = false;
            }
#endif
        }
    }
}
