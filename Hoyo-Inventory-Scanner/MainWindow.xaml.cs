using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;

namespace Hoyo_Inventory_Scanner
{
    public partial class MainWindow : Window {
        Scanner scanner;

        public MainWindow() {
            InitializeComponent();

            Logger.DebugLog = (msg) =>
            {
                DebugBox.Dispatcher.Invoke(() =>
                {
                    DebugBox.AppendText(msg + "\n");
                    DebugBox.ScrollToEnd();
                });
            };

            Logger.ResultLog = (msg) =>
            {
                ResultBox.Dispatcher.Invoke(() =>
                {
                    ResultBox.AppendText(msg + "\n");
                    ResultBox.ScrollToEnd();
                });
            };

            Logger.ClearDebugAction = () =>
            {
                DebugBox.Dispatcher.Invoke(() =>
                {
                    DebugBox.Clear();
                });
            };

            Logger.ClearResultAction = () =>
            {
                ResultBox.Dispatcher.Invoke(() =>
                {
                    ResultBox.Clear();
                });
            };
        }

        private void Speed_TextChanged(object sender, TextChangedEventArgs e) {
            if (int.TryParse(Speed.Text, out int value))
            {
                if (value < 100 || value > 9999)
                {
                    Speed.Background = System.Windows.Media.Brushes.LightCoral; // invalide
                }
                else
                {
                    Speed.Background = System.Windows.Media.Brushes.White; // valide
                }
            }
        }

        private void Speed_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Scan_Click(object sender, RoutedEventArgs e) {
            string game = (Game.SelectedItem as ComboBoxItem)?.Content.ToString();
            int scan_speed = int.TryParse(Speed.Text, out int v) ? v : 500;
            bool scan_four_stars = Four_stars.IsChecked ?? false;
            bool is_geforce_now = GN.IsChecked ?? false;
            System.Drawing.Point top_left_of_game = new System.Drawing.Point(0, 0);
            Process[] processes;
            string game_process = "GenshinImpact";

            if (game.Equals("HSR"))
                game_process = "StarRail";
            if (game.Equals("ZZZ"))
                game_process = "ZenlessZoneZero";
            if (is_geforce_now)
                game_process = "GeForceNOW";
                
            processes = Process.GetProcessesByName(game_process);
            foreach (Process proc in processes) {
                string windowTitle = proc.MainWindowTitle;
                if (!string.IsNullOrEmpty(windowTitle))
                    top_left_of_game = WindowHelper.GetTopLeft(windowTitle);
            }

            Logger.ClearDebug();
            Logger.ClearResult();

            switch (game) {
                case "Genshin Impact":
                    scanner = new ScannerGenshin(scan_speed, scan_four_stars, is_geforce_now, top_left_of_game.X + 179, top_left_of_game.Y + 254, 146, 175, 8, 4, new System.Drawing.Size(492, 781), top_left_of_game.X + 1308, top_left_of_game.Y + 178);
                    break;
                default:
                    scanner = new ScannerGenshin(scan_speed, scan_four_stars, is_geforce_now, top_left_of_game.X + 179, top_left_of_game.Y + 254, 146, 175, 8, 4, new System.Drawing.Size(492, 781), top_left_of_game.X + 1308, top_left_of_game.Y + 178);
                    break;
            }

            Logger.LogDebug("Game : " + game);
            Logger.LogDebug("Scan speed : " + scan_speed + "ms");
            Logger.LogDebug("Geforce Now : " + is_geforce_now);
            Logger.LogDebug("Scan 4* : " + scan_four_stars);
            Logger.LogDebug("Starting scanning process now.");
            scanner.test();
            // scanner.scan();
            Logger.LogDebug("End of scanning process.");
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
