using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Hoyo_Inventory_Scanner
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Game_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GN_isChecked(object sender, RoutedEventArgs e)
        {

        }

        private void Four_stars_isChecked(object sender, RoutedEventArgs e)
        {

        }

        private void Speed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(Speed.Text, out int value))
            {
                if (value < 100 || value > 9999)
                {
                    Speed.Background = Brushes.LightCoral; // invalide
                }
                else
                {
                    Speed.Background = Brushes.White; // valide
                }
            }
        }

        private void Speed_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); // tout sauf chiffre
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
