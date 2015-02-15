using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;

namespace NFCBattleShip
{
    public partial class FinPage : PhoneApplicationPage
    {
        public FinPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            String Text = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("Ganador", out Text))
            {
                int ganado = Convert.ToInt32(Text);

                if( ganado == 1 )
                {
                    Tb_Ganar.Text = "HAS GANADO!!!";
                }
                else
                {
                    Tb_Ganar.Text = "HAS PERDIDO!!!";
                }
            }
            base.OnNavigatedTo(e);
        }

        private void Btn_Salir_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Volver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/InfoPage.xaml?", UriKind.Relative));
        }

    }
}