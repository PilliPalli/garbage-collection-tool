using Garbage_Collector.Model;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Garbage_Collector.Utilities
{
    public class AdminVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentUsername)
            {
                using (var context = new GarbageCollectorDbContext())
                {
                    var user = context.Users
                        .Where(u => u.Username == currentUsername)
                        .Select(u => new
                        {
                            u.UserId,
                            Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                        })
                        .FirstOrDefault();

                    if (user != null && user.Roles.Contains("Admin"))
                    {
                        return Visibility.Visible;
                    }
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
