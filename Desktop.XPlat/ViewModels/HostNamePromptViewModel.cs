using Avalonia.Controls;
using ReactiveUI;
using Remotely.Desktop.XPlat.Services;
using System.Windows.Input;

namespace Remotely.Desktop.XPlat.ViewModels
{
    public class HostNamePromptViewModel : BrandedViewModelBase
    {
        public string _host = "https://";

        private string _alias = "";

        private string _group = "";

        public string Host
        {
            get => _host;
            set => this.RaiseAndSetIfChanged(ref _host, value);
        }

        public string Alias
        {
            get => _alias;
            set => this.RaiseAndSetIfChanged(ref _alias, value);
        }

        public string Group
        {
            get => _group;
            set => this.RaiseAndSetIfChanged(ref _group, value);
        }

        public ICommand OKCommand => new Executor((param) =>
        {
            (param as Window).Close();
        });
    }
}
