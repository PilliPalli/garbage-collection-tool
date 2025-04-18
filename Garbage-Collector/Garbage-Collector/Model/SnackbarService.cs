using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Garbage_Collector.Utilities
{
    public static class SnackbarService
    {
        private static readonly Dictionary<Window, Snackbar> _snackbars = new();
        private static readonly Dictionary<Window, SnackbarMessageQueue> _queues = new();

        public static void Initialize(Window window)
        {
            if (_snackbars.ContainsKey(window)) return;

            var queue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            var snackbar = new Snackbar
            {
                MessageQueue = queue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Width = 300,
                Margin = new Thickness(10),
                Background = Brushes.DarkSlateGray,
                Foreground = Brushes.White
            };

            if (window.Content is Grid grid)
            {
                grid.Children.Add(snackbar);
            }
            else if (window.Content is FrameworkElement content)
            {
                var newGrid = new Grid();
                window.Content = newGrid;
                newGrid.Children.Add(content);
                newGrid.Children.Add(snackbar);
            }

            _snackbars[window] = snackbar;
            _queues[window] = queue;

            window.Closed += (s, e) =>
            {
                _snackbars.Remove(window);
                _queues.Remove(window);
            };
        }

        private static void Enqueue(Window? window, string message, Brush background)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (window == null) return;

                if (_snackbars.TryGetValue(window, out var snackbar))
                    snackbar.Background = background;

                if (_queues.TryGetValue(window, out var queue))
                    queue.Enqueue(message);
            });
        }

        public static void ShowSuccess(Window window, string msg) =>
            Enqueue(window, "✅ " + msg, Brushes.DarkGreen);

        public static void ShowError(Window window, string msg) =>
            Enqueue(window, "❌ " + msg, Brushes.DarkRed);
        public static Window? GetActiveWindow() =>
            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            
        public static void Show(string msg, string type)
        {
            var window = GetActiveWindow();
            if (window == null) return;
            
            switch (type.ToLower())
            {
                case "success":
                    ShowSuccess(window, msg);
                    break;
                case "error":
                    ShowError(window, msg);
                    break;
            }
        }
    }
}
