using Garbage_Collector.Utilities;

namespace Garbage_Collector.ViewModel
{
    public class HomeVM : ViewModelBase
    {
        private string _welcomeMessage;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                _welcomeMessage = value;
                OnPropertyChanged();
            }
        }

        public HomeVM()
        {
            // Setze die Willkommensnachricht mit dem aktuellen Benutzernamen
            WelcomeMessage = $"Welcome, {LoginVM.CurrentUserName}!";
        }
    }

}
