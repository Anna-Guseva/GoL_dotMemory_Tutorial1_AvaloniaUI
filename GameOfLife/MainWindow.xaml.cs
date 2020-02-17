using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace GameOfLife
{
    public class MainWindow : Window
    {
        private readonly Grid _mainGrid;
        private readonly DispatcherTimer _timer;
        private int _genCounter;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _mainGrid = new Grid(this.FindControl<Canvas>("MainCanvas"));
            _timer = new DispatcherTimer();
            _timer.Tick += OnTimer;
            _timer.Interval = TimeSpan.FromMilliseconds(200);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var buttonStart = this.FindControl<Button>("ButtonStart");
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                buttonStart.Content = "Stop";
            }
            else
            {
                _timer.Stop();
                buttonStart.Content = "Start";
            }
        }

        private void OnTimer(object sender, EventArgs e)
        {
            _mainGrid.Update();
            _genCounter++;
            this.FindControl<TextBlock>("lblGenCount").Text = "Generations: " + _genCounter;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            _mainGrid.Clear();
        }
    }
}
