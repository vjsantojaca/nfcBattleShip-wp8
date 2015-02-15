using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NdefLibrary.Ndef;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using Windows.Networking.Proximity;

namespace NFCBattleShip
{
    public partial class JuegoPage : PhoneApplicationPage
    {
        private int turno;
        int[][] mapaBarcos;
        int[][] mapaBarcosContrincante = new int[10][];
        private ProximityDevice _device;
        String resultado;
        private long _subscriptionIdNdef;
        private int hundidosMiMapa;
        private int hundidosSuMapa;
        private int ganado;
        private int fila;
        private int columna;
        private int x;
        private int y;
        private Image img;
        private Boolean cerca;

        public JuegoPage()
        {
            InitializeComponent();
            _device = ProximityDevice.GetDefault();
            limpiarMatriz();
            cerca = false;
            ganado = -1;
            hundidosMiMapa = 0;
            hundidosSuMapa = 0;

            if (_device == null)
            {
                MessageBox.Show("Ha habido un fallo con la conexión de proximidad (NFC).");
            }
            else
            {
                MessageBox.Show("Cada vez que vaya a elegir una posición acerque el teléfono al de su contrincante, por favor. Haga lo mismo cuando elija la posición su contrincante.");
                TB_BarcosHundidos.Text = String.Format("Has hundido {0} barcos de tu contrincante. Te quedan {1} barcos por hundir.", hundidosSuMapa, (10 - hundidosSuMapa));
                _device.DeviceArrived += DeviceArrived;
                _device.DeviceDeparted += DeviceDeparted;
            }
        }

        private void DeviceDeparted(ProximityDevice sender)
        {
            cerca = false;
            _device.StopSubscribingForMessage(_subscriptionIdNdef);
        }

        private void DeviceArrived(ProximityDevice sender)
        {
            cerca = true;
            _subscriptionIdNdef = _device.SubscribeForMessage("NDEF", MessageReceivedHandler);
        }

        public void limpiarMatriz()
        {
            for (int i = 0; i < mapaBarcosContrincante.Length; i++)
            {
                mapaBarcosContrincante[i] = new int[10];
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    mapaBarcosContrincante[i][j] = 0;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            object myParameter = NavigationService.GetLastNavigationData();
            if (myParameter != null)
            {
                MapaBarco mapaBarco = (MapaBarco)myParameter;

                turno = mapaBarco.Turno;
                mapaBarcos = mapaBarco.MapaBarcos;

                if (turno == 0)
                {
                    MessageBox.Show("No es tu turno. Espere a que acabe su contrincante.");
                }
                else
                {

                    MessageBox.Show("Es tu turno");
                }
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            MessageBoxResult box = MessageBox.Show("¿Realmente quieres salir?", "SALIR", MessageBoxButton.OKCancel);

            if (box != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }
            else
            {
                Application.Current.Terminate();
            }
        }

        private void clickCoordenada(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if( turno == 1)
            {
                img = (Image)sender; //Sabemos que eso es una imagen
                String imagenNombre = img.Name;
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        String nombreCuadrante = String.Format("Fondo{0}{1}", j, i); //A cada imagen la he llamado fondo y luego el número de columna y el número de fila
                        if (nombreCuadrante.Equals(imagenNombre))
                        {
                            fila = i;
                            columna = j;
                        }
                    }
                }
                String mensaje = "¿Realmente quieres probar suerte con estas coordenadas [" + fila + "," + columna + "]?";

                MessageBoxResult box = MessageBox.Show(mensaje, "Comprobar", MessageBoxButton.OKCancel);

                if (box == MessageBoxResult.OK)
                {
                    MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                                        " ,Gracias!!.");
                    if (cerca == false)
                    {
                        MessageBox.Show("No hay ningún móvil cerca, acerquese a otro móvil y vuelva a intentarlo.");
                        return;
                    }

                    if (mapaBarcosContrincante[fila][columna] != 2 && mapaBarcosContrincante[fila][columna] != 1)
                    {
                        String x = Convert.ToString(fila);
                        String y = Convert.ToString(columna);
                        String lang = "es";

                        System.Text.UTF8Encoding codificador = new System.Text.UTF8Encoding();
                        byte[] textBytesX = codificador.GetBytes(x);
                        byte[] langBytes = codificador.GetBytes(lang);
                        int langLength = langBytes.Length;
                        int textLengthX = textBytesX.Length;

                        byte[] textBytesY = codificador.GetBytes(y);
                        int textLengthY = textBytesY.Length;
                        byte[] payloadY = new byte[1 + langLength + textLengthY]; ;
                        payloadY[0] = (byte)langLength;

                        byte[] payloadX = new byte[1 + langLength + textLengthX];
                        payloadX[0] = (byte)langLength;

                        System.Array.Copy(langBytes, 0, payloadX, 1, langLength);
                        System.Array.Copy(textBytesX, 0, payloadX, 1 + langLength, textLengthX);

                        System.Array.Copy(langBytes, 0, payloadY, 1, langLength);
                        System.Array.Copy(textBytesY, 0, payloadY, 1 + langLength, textLengthY);

                        var recordX = new NdefRecord();

                        recordX.Id = new byte[0];
                        recordX.Payload = payloadX;
                        recordX.TypeNameFormat = NdefRecord.TypeNameFormatType.NfcRtd;
                        recordX.Type = new byte[] { (byte)'T' };

                        var recordY = new NdefRecord();

                        recordY.Id = new byte[0];
                        recordY.Payload = payloadY;
                        recordY.TypeNameFormat = NdefRecord.TypeNameFormatType.NfcRtd;
                        recordY.Type = new byte[] { (byte)'T' };

                        var message = new NdefMessage { recordX, recordY };

                        _device.PublishBinaryMessage("NDEF", message.ToByteArray().AsBuffer(), messageTransmittedHandler);
                    }
                    else
                    {
                        MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                                        " ,Gracias!!.");
                    }
                }
                else
                {
                    MessageBox.Show("Elija otra coordenada. Suerte.");
                }
			}
            else
            {
                MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                               " ,Gracias!!.");
		    }
        }

        private void messageTransmittedHandler(ProximityDevice sender, long messageId)
        {
            _device.StopPublishingMessage(messageId);
        }

        private void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
                //Esto es lo que recibimos.
                String resultado = RecibirInfo(message);
                
                if( !resultado.Contains("coordenadas"))
                {
                    if (resultado.Contains("Agua"))
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            mapaBarcosContrincante[fila][columna] = 1;
                            MessageBox.Show("Ha sido agua.");
                            img.Source = new BitmapImage(new Uri("Resources/fondoAgua.png", UriKind.RelativeOrAbsolute)); //Fondo de agua
                            turno = 0;
                            MessageBox.Show("No es tu turno!!!!!");
                        });
                        
                    }
                    else if (resultado.Contains("Tocado") || resultado.Contains("Hundido"))
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            mapaBarcosContrincante[fila][columna] = 2;
                            if (resultado.Contains("Tocado"))
                            {
                                MessageBox.Show("Ha sido tocado!!!!!");
                            }
                            else
                            {
                                hundidosSuMapa = hundidosSuMapa + 1;
                                MessageBox.Show("Has hundido un barco de tu contrincante!!!!!");
                                TB_BarcosHundidos.Text = String.Format("Has hundido {0} barcos de tu contrincante. Te quedan {1} barcos por hundir.", hundidosSuMapa, (10 - hundidosSuMapa));
                            }
                            img.Source = new BitmapImage(new Uri("Resources/fondoTocado.png", UriKind.RelativeOrAbsolute)); //Fondo de tocado
                            turno = 1;
                            MessageBox.Show("Es tu turno!!!!!");
                        }); 
                        
                    }
                    else if (resultado.Contains("HAS GANADO"))
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ganado = 1;
                            pasarPagina();
                        });
                    }
                    else
                    {
                        return;
                    }
                }
        }

        public String RecibirInfo(ProximityMessage mensaje)
        {
            if (mensaje != null)
            {
                var rawMsg = mensaje.Data.ToArray();
                var ndefMessage = NdefMessage.FromByteArray(rawMsg);


                if (ndefMessage.LongCount() > 1)
                {
                    //Si recibimos un mensaje con más de 1 record es que hemos recibido las coordenadas
                    var ndefRecordX = ndefMessage[0];
                    var ndefRecordY = ndefMessage[1];

                    byte[] payloadX = ndefRecordX.Payload;
                    byte[] payloadY = ndefRecordY.Payload;

                    String lang = "es";
                    System.Text.UTF8Encoding codificador = new System.Text.UTF8Encoding();
                    byte[] langBytes = codificador.GetBytes(lang);
                    int languageCodeLength = langBytes.Length;

                    String payloadCoordX = codificador.GetString(payloadX, languageCodeLength + 1, payloadX.Length - languageCodeLength - 1);
                    String payloadCoordY = codificador.GetString(payloadY, languageCodeLength + 1, payloadY.Length - languageCodeLength - 1);

                    x = Convert.ToInt32(payloadCoordX);
                    y = Convert.ToInt32(payloadCoordY);

                    enviarResultado();
                    return ("coordenadas"); //Es decir solo hemos recibido la petición de querer saber si ha sido tocado o hundido.
                }
                else
                {
                    String resultado = null;
                    var ndefRecordRes = ndefMessage[0];
                    byte[] payloadRes = ndefRecordRes.Payload;

                    String lang = "es";
                    System.Text.UTF8Encoding codificador = new System.Text.UTF8Encoding();
                    byte[] langBytes = codificador.GetBytes(lang);
                    int languageCodeLength = langBytes.Length;

                    resultado = codificador.GetString(payloadRes, languageCodeLength + 1, payloadRes.Length - languageCodeLength - 1);

                    return (resultado);
                }
            }
            else
            {
                return(null);
            }
        }

        public void enviarResultado()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           if (mapaBarcos[x][y] == 0)
                           {
                               //Ha dado en agua, así que habrá que envíar un mensaje al contrincante diciendo que ha sido agua.

                               MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                       " ,Gracias!!.");
                               send("Agua");
                               turno = 1;
                               MessageBox.Show("Es tu turno!!!!!");
                           }
                           else if (mapaBarcos[x][y] == 1)
                           {
                               //Ha sido tocado, habrá que comprobar si ha sido hundido o simplemente ha sido tocado.
                               //Recorremos la fila
                               for (int i = 0; i < (10 - y); i++)
                               {
                                   if (mapaBarcos[x][y + i] == 1 && i > 0)
                                   {
                                       //Es que por ese lado hay más barco, se enviaría un tocado.
                                       MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                       " ,Gracias!!.");
                                       send("Tocado");
                                       MessageBox.Show("No es tu turno!!!!!");
                                       mapaBarcos[x][y] = 2;
                                       turno = 0; //Ha ganado el turno él así que sigue jugando
                                       break;
                                   }
                                   else if (mapaBarcos[x][y + i] == 0 && i > 0 || mapaBarcos[x][y + i] == 1 && (y + i) == 9)
                                   {
                                       //Es que por ese lado no hay más barco, hay que buscar por el otro lado
                                       for (int j = y; j > -1; j--)
                                       {
                                           if (mapaBarcos[x][j] == 1 && j != y)
                                           {
                                               //Es que por ese lado hay más barco, se enviaría un tocado.
                                               MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                               " ,Gracias!!.");
                                               mapaBarcos[x][y] = 2;
                                               send("Tocado");
                                               MessageBox.Show("No es tu turno!!!!!");
                                               turno = 0; //Ha ganado el turno él así que sigue jugando
                                               break;
                                           }
                                           else if (mapaBarcos[x][j] == 0 && j != y || mapaBarcos[x][j] == 1 && j == 0)
                                           {
                                               //Puede que esté hundido o que simplemente el barco esté en forma vertical
                                               for (int z = x; z < 10; z++) //primero vamos para abajo
                                               {
                                                   if (mapaBarcos[z][y] == 1 && z != x)
                                                   {
                                                       //TOCADO
                                                       MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                                       " ,Gracias!!.");
                                                       mapaBarcos[x][y] = 2;
                                                       send("Tocado");
                                                       MessageBox.Show("No es tu turno!!!!!");
                                                       turno = 0; //Ha ganado el turno él así que sigue jugando
                                                       break;
                                                   }
                                                   else if (mapaBarcos[z][y] == 0 && z != x || mapaBarcos[z][y] == 1 && z==9)
                                                   {
                                                       //Hay que comprobar para arriba
                                                       for (int k = x; k > -1; k--)
                                                       {
                                                           if (mapaBarcos[k][y] == 1 && k != x)
                                                           {
                                                               //TOCADO
                                                               MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                                               " ,Gracias!!.");
                                                               mapaBarcos[x][y] = 2;
                                                               send("Tocado");
                                                               MessageBox.Show("No es tu turno!!!!!");
                                                               turno = 0; //Ha ganado el turno él así que sigue jugando
                                                               break;
                                                           }
                                                           else if (mapaBarcos[k][y] == 0 && k != x || mapaBarcos[k][y] == 1 && k == 0)
                                                           {
                                                               //Es que por este lado ya no hay más barco y se enviará un tocado y hundido.

                                                               hundidosMiMapa = hundidosMiMapa + 1;
                                                               if (hundidosMiMapa == 10)
                                                               {
                                                                   MessageBox.Show("Su contrincante ha hundido el último de sus barcos.");
                                                                   MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                                                   " ,Gracias!!.");
                                                                   mapaBarcos[x][y] = 2;
                                                                   send("HAS GANADO"); //Has perdido y envías que el otro ha ganado
                                                                   ganado = 0;
                                                                   //Nos vamos a otra pantalla (una pantalla final)
                                                                   pasarPagina();
                                                               }
                                                               else
                                                               {
                                                                   MessageBox.Show("Por favor, mantenga su móvil pegado al de su contrincante" +
                                                                   " ,Gracias!!.");
                                                                   mapaBarcos[x][y] = 2;
                                                                   send("Hundido");
                                                                   MessageBox.Show("No es tu turno!!!!!");
                                                                   turno = 0; //Ha ganado el turno él así que sigue jugando
                                                               }
                                                               break;
                                                           }
                                                       }
                                                       break;
                                                   }
                                               }
                                               break;
                                           }
                                       }

                                       break;
                                   }
                               }
                           }
                       });
       	}

        private void pasarPagina()
        {
            _device.DeviceDeparted -= DeviceDeparted;
            _device.DeviceArrived -= DeviceArrived;
            _device.StopSubscribingForMessage(_subscriptionIdNdef);

            NavigationService.Navigate(new Uri("/JuegoPage.xaml?Ganar=" + ganado, UriKind.Relative)); //0 es que has perdido 1 que has ganado
        }

        public void send(String mensaje)
        {
            String lang = "es";
            System.Text.UTF8Encoding codificador = new System.Text.UTF8Encoding();
            byte[] textBytesMensaje = codificador.GetBytes(mensaje);
            byte[] langBytes = codificador.GetBytes(lang);
            int langLength = langBytes.Length;
            int textLengthMensaje = textBytesMensaje.Length;

            byte[] payloadMensaje = new byte[1 + langLength + textLengthMensaje];
            payloadMensaje[0] = (byte)langLength;

            System.Array.Copy(langBytes, 0, payloadMensaje, 1, langLength);
            System.Array.Copy(textBytesMensaje, 0, payloadMensaje, 1 + langLength, textLengthMensaje);

            var recordMensaje = new NdefRecord();

            recordMensaje.Id = new byte[0];
            recordMensaje.Payload = payloadMensaje;
            recordMensaje.TypeNameFormat = NdefRecord.TypeNameFormatType.NfcRtd;
            recordMensaje.Type = new byte[] { (byte)'T' };

            var message = new NdefMessage { recordMensaje };

            _device.PublishBinaryMessage("NDEF", message.ToByteArray().AsBuffer(), messageTransmittedHandler);
        }
    }
}