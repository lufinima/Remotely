using Remotely.Desktop.Core.ViewModels;

namespace Remotely.Desktop.Win.ViewModels
{
    public class HostNamePromptViewModel : BrandedViewModelBase
    {
        private string _host = "https://";

        private string _alias = "";

        private string _group = "";

        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                FirePropertyChanged();
            }
        }

        public string Alias
        {
            get => _alias;
            set
            {
                _alias = value;
                FirePropertyChanged();
            }
        }

        public string Group
        {
            get => _group;
            set
            {
                _group = value;
                FirePropertyChanged();
            }
        }
    }
}
