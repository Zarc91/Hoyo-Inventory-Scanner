using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System;

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
            if (int.TryParse(Speed.Text, out int value)) {
                if (value < 100 || value > 9999) {
                    Speed.Background = System.Windows.Media.Brushes.LightCoral; // invalide
                }
                else {
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
            bool is_geforce_now = GN.IsChecked ?? false;
            System.Drawing.Point top_left_of_game = new System.Drawing.Point(0, 0);
            Process[] processes;
            string game_process = "GenshinImpact";
            Stopwatch stopwatch = new Stopwatch();

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
                    scanner = new ScannerGenshin(scan_speed, is_geforce_now, top_left_of_game.X + 179, top_left_of_game.Y + 254, 146, 175, 8, 4, new System.Drawing.Size(492, 781), top_left_of_game.X + 1308, top_left_of_game.Y + 178);
                    break;
                case "HSR":
                    scanner = new ScannerHSR(scan_speed, is_geforce_now, top_left_of_game.X + 187, top_left_of_game.Y + 268, 125, 149, 9, 5, new System.Drawing.Size(440, 625), top_left_of_game.X + 1402, top_left_of_game.Y + 194);
                    break;
                default:
                    scanner = new ScannerGenshin(scan_speed, is_geforce_now, top_left_of_game.X + 179, top_left_of_game.Y + 254, 146, 175, 8, 4, new System.Drawing.Size(492, 781), top_left_of_game.X + 1308, top_left_of_game.Y + 178);
                    break;
            }

            Logger.LogDebug("Game : " + game);
            Logger.LogDebug("Scan speed : " + scan_speed + "ms");
            Logger.LogDebug("Geforce Now : " + is_geforce_now);
            Logger.LogDebug("Starting scanning process now.");
            stopwatch.Start();
            scanner.scan();
            stopwatch.Stop();
            Logger.LogDebug("End of scanning process.");
            Logger.LogDebug("Execution time : " + stopwatch.Elapsed.TotalMinutes.ToString()[0] + "min " + stopwatch.Elapsed.Seconds + "s");
            Logger.LogResult(scanner.generateExport());
            Logger.LogDebug("Generated the output.");
        }

        private void Export_Click(object sender, RoutedEventArgs e) {
            string game = (Game.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!ResultBox.Text.Equals("")) {
                File.WriteAllText("./saved_exports/" + game + DateTime.Now.ToString(" dd-MM-yy HH-mm-ss") + ".json", ResultBox.Text);
                Process.Start("explorer.exe", @".\saved_exports");
                WindowHelper.FocusWindow("explorer");
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e) {
            ResultBox.SelectAll();
            ResultBox.Copy();
        }
    }
}
