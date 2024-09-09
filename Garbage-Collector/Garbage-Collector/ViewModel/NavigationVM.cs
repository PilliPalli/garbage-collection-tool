using Garbage_Collector.Utilities;
using Garbage_Collector.View;
using System.Windows;
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
        public ICommand InformationCommand { get; set; }
        public ICommand AdministrationCommand { get; set; }
        public ICommand LogoutCommand { get; }


        private void Home(object obj) => CurrentView = new HomeVM();
        private void Cleanup(object obj) => CurrentView = new CleanupVM();
        private void Settings(object obj) => CurrentView = new SettingsVM();
        private void Information(object obj) => CurrentView = new InformationVM();
        private void Administration(object obj) => CurrentView = new AdministrationVM();

        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            CleanupCommand = new RelayCommand(Cleanup);
            SettingsCommand = new RelayCommand(Settings);
            InformationCommand = new RelayCommand(Information);
            AdministrationCommand = new RelayCommand(Administration);
            LogoutCommand = new RelayCommand(Logout);


            CurrentView = new HomeVM();
        }
        private void Logout(object parameter)
        {
            var loginWindow = new Login();
            loginWindow.Show();

            Application.Current.MainWindow.Close();
        }
    }
}