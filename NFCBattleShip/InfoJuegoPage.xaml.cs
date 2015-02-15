using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NFCBattleShip.Resources;
using System.Windows.Media.Imaging;
using Microsoft.Phone.BackgroundAudio;

namespace NFCBattleShip
{
    public partial class InfoJuegoPage : PhoneApplicationPage
    {
        public InfoJuegoPage()
        {
            InitializeComponent();
            Tb_info.Text = "Bienvenidos a NFCBattleShip: Android vs WP8. Si quieres pasar un rato de diversión este es tu juego, es fácil y sencillo."
            + "Coloca tus barcos, espera que los de tu contrincante estén también colocados acercad los móviles y dad al botón Go!! y que comience la diversión.";
            Logo.Source = new BitmapImage(new Uri("Resources/logo.png", UriKind.RelativeOrAbsolute));

        }

        private void BotonSiguiente_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ColocarBarcosPage.xaml", UriKind.Relative));
        }
    }
}