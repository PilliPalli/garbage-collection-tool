using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Garbage_Collector.Utilities;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand CleanupCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void Cleanup(object obj) => CurrentView = new CleanupVM();
        private void Setting(object obj) => CurrentView = new SettingsVM();

        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            CleanupCommand = new RelayCommand(Cleanup);
            SettingsCommand = new RelayCommand(Setting);

           
            CurrentView = new HomeVM();
        }
    }
}