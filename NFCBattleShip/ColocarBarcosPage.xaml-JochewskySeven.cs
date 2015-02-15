using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NdefLibrary.Ndef;
using Windows.Networking.Proximity;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading;
using System.ComponentModel;

namespace NFCBattleShip
{
    public partial class ColocarBarcosPage : PhoneApplicationPage
    {
        int[][] mapaBarcos = new int[10][];
        int[] numBarcos = new int[4];
        private int fila;
        private int columna;
        private Boolean recibido;
        private Boolean colocar;
        private Boolean enviado;
        private Boolean cerca;
        private int valorRecibido;
        private int orientacion; //0 es horizontal 1 es vertical
        private int numeroAleatorio;
        private ProximityDevice _device;
        private ProximityMessage mensajeRecibido;
        private long _subscriptionIdNdef;

        public ColocarBarcosPage()
        {
            InitializeComponent();
            _device = ProximityDevice.GetDefault();
            
            Random r = new Random();
            cerca = false;
            numeroAleatorio = r.Next();
            orientacion = 0;
            recibido = false;
            enviado = false;
            colocar = true;
            mensajeRecibido = null;
            Cb_barcos.SelectedIndex = 0;
            
            if (_device == null)
            {
                MessageBox.Show("Ha habido un fallo con la conexión de proximidad (NFC).");
            }
            else
            {
                crearLimpiarArray();
                TxtB_Barcos.Text = String.Format("Quedan {0} barcos por colocar.", numBarcos[Cb_barcos.SelectedIndex]);
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
            //Supuestamente con esto nos hemos suscrito a los mensajes NDEF que recibamos y se correrá el método MessageReceivedHandler  
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

        private void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            if (_device != null)
            {
                mensajeRecibido = message;
                read();

                recibido = true;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (enviado == true)
                    {
                        pasarPagina();
                    }
                });
            }
        }

        private void read()
        {
            if (mensajeRecibido != null)
            {
                var rawMsg = mensajeRecibido.Data.ToArray();
                var ndefMessage = NdefMessage.FromByteArray(rawMsg);

                var ndefRecordOK = ndefMessage[0]; //No nos interesa saber lo que tiene
                var ndefRecordValor = ndefMessage[1];

                byte[] payloadValor = ndefRecordValor.Payload;
                //Ya tenemos el payload

                String lang = "es";
                System.Text.UTF8Encoding codificador = new System.Text.UTF8Encoding();
                byte[] langBytes = codificador.GetBytes(lang);
                int languageCodeLength = langBytes.Length;

                String payloadVal = codificador.GetString(payloadValor, languageCodeLength + 1, payloadValor.Length - languageCodeLength - 1);

                valorRecibido = Convert.ToInt32(payloadVal);

           }
        }


        private void elegirOrientacion(object sender, RoutedEventArgs e)
        {
            if (Rb_horizontal.IsChecked.Value)
            {
                orientacion = 0;
            }
            else
            {
                orientacion = 1;
            }
        }

        private void crearLimpiarArray()
        {
            for (int i = 0; i < mapaBarcos.Length; i++)
            {
                mapaBarcos[i] = new int[10];
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    mapaBarcos[i][j] = 0; //Inicializamos todo a cero para que sea todo agua en un principio.
                }
            }

            for (int i = 4; i > 0; i--)
            {
                numBarcos[4 - i] = i; //Así la primera posición valdrá 4 (ya que habrá 4 barcos de 1 casilla)
            }
        }

        private void BotonGo_Click(object sender, RoutedEventArgs e)
        {
            if (_device == null) { return; }
            if (cerca == false) 
            {
                MessageBox.Show("No hay ningún móvil cerca.");
                return; 
            }

            int total = 0;
            for (int i = 0; i < 4; i++)
            {
                if (numBarcos[i] == 0)
                {
                    total++;
                }
            }
            if (total == 4)
            {
                //Enviamos primero un Ok y luego el número aleatorio.
                String textOk = "OK";
                String lang = "es";
                System.Text.UTF8Encoding codificador = new System.Text.UTF8Encoding();
                byte[] textBytesOK = codificador.GetBytes(textOk);
                byte[] langBytes =  codificador.GetBytes(lang);
                int langLength = langBytes.Length;
                int textLengthOK = textBytesOK.Length;

                byte[] payloadOK = new byte[1 + langLength + textLengthOK];
                payloadOK[0] = (byte)langLength;

                System.Array.Copy(langBytes, 0, payloadOK, 1, langLength);
                System.Array.Copy(textBytesOK, 0, payloadOK, 1 + langLength, textLengthOK);

                var recordOK = new NdefRecord();
               
                recordOK.Id = new byte[0];
                recordOK.Payload = payloadOK;
                recordOK.TypeNameFormat = NdefRecord.TypeNameFormatType.NfcRtd;
                recordOK.Type = new byte[] { (byte)'T' };

                String textValor = numeroAleatorio.ToString();
                byte[] textBytesValor = codificador.GetBytes(textValor);
                int textLengthValor = textBytesValor.Length;
                byte[] payloadValor = new byte[1 + langLength + textLengthValor]; ;
                payloadValor[0] = (byte)langLength;

                System.Array.Copy(langBytes, 0, payloadValor, 1, langLength);
                System.Array.Copy(textBytesValor, 0, payloadValor, 1 + langLength, textLengthValor);


                var recordValor = new NdefRecord();
                
                recordValor.Id = new byte[0];
                recordValor.Payload = payloadValor;
                recordValor.TypeNameFormat = NdefRecord.TypeNameFormatType.NfcRtd;
                recordValor.Type = new byte[] { (byte) 'T'};

                var message = new NdefMessage { recordOK, recordValor };

                _device.PublishBinaryMessage("NDEF", message.ToByteArray().AsBuffer(), messageTransmittedHandler);
                

                enviado = true;

                if (recibido == true)
                {
                    pasarPagina();
                }
            }
            else
            {
                MessageBox.Show("Aún le quedan barcos por colocar.");
            }
        }

        private void messageTransmittedHandler(ProximityDevice sender, long messageId)
        {
            _device.StopPublishingMessage(messageId);
        }

        private void pasarPagina()
        {
            _device.StopSubscribingForMessage(_subscriptionIdNdef);
            _device.DeviceArrived -= DeviceArrived;
            _device.DeviceDeparted -= DeviceDeparted;

            int turno = -1;
            if( numeroAleatorio > valorRecibido ) turno = 1; //Empeza primero
            else                                  turno = 0; //Empieza después

            MapaBarco mapaBarco = new MapaBarco(mapaBarcos, turno);
            NavigationService.Navigate("/JuegoPage.xaml", mapaBarco);            
        }

        
        private void clickCoordenada(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Rb_horizontal.IsChecked.Value && !Rb_vertical.IsChecked.Value)
            {
                MessageBox.Show("Elija primero una orientación para su barco por favor.");
            }
            else
            {
                Image imagen = (Image)sender; //Sabemos que eso es una imagen
                String imagenNombre = imagen.Name;

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        String nombreCuadrante = String.Format("Fondo{0}{1}", j, i); //A cada imagen la he llamado fondo y luego el número de columna y el número de fila
                        if (nombreCuadrante.Equals(imagenNombre))
                        {
                            fila = i;
                            columna = j;
                            comprobarBarcos(imagen);
                        }
                    }
                }
            }
        }

        private void comprobarBarcos(Image imagen)
        {
            colocar = true;
            //Primero comprobamos si el barco cabe en esa posición
            if (mapaBarcos[fila][columna] == 0)
            {
                if (numBarcos[Cb_barcos.SelectedIndex] == 0)
                {
                    //No hay más barcos
                    MessageBox.Show("Ya no puede" +
                            " colocar más barcos de ese tamaño");
                    colocar = false;
                }
                else if ((columna + Cb_barcos.SelectedIndex > 9 && orientacion == 0))
                {
                    //No entra en la fila
                    MessageBox.Show("No puede" +
                                " colocar el barco ahí, no hay hueco en esa fila");
                    colocar = false;

                }
                else if (fila + Cb_barcos.SelectedIndex > 9 && orientacion == 1)
                {
                    //No entra en la columna
                    MessageBox.Show("No puede colocar el barco ahí, no hay hueco en esa columna");

                    colocar = false;
                }
                else
                {
                    for (int i = 0; i < Cb_barcos.SelectedIndex + 1 && colocar == true; i++)
                    {
                        if ((orientacion == 0) &&
                                        (((fila == 9) && ( (columna + i == 9 && mapaBarcos[fila - 1][columna - 1 + i] == 1)

                                                        || (columna + i == 0 && mapaBarcos[fila - 1][columna + 1 + i] == 1) 

                                                        || (columna + i != 0 && columna + i != 9 && (mapaBarcos[fila - 1][columna - 1 + i] == 1

                                                                                        || mapaBarcos[fila - 1][columna + 1 + i] == 1))
                                                                                  
                                                        || (mapaBarcos[fila - 1][columna + i] == 1)))

                                        || ((fila == 0) && (((columna + i == 9 && mapaBarcos[fila + 1][columna - 1 + i] == 1))

                                                            || (columna + i == 0 && mapaBarcos[fila + 1][columna + 1 + i] == 1)

                                                            || (columna + i != 0 && columna + i != 9 && (mapaBarcos[fila + 1][columna - 1 + i] == 1

                                                                                    || mapaBarcos[fila + 1][columna + 1 + i] == 1))
                                                                              
                                                            || (mapaBarcos[fila + 1][columna + i] == 1) ))

                                        || ((fila != 9 && fila != 0) && (((columna + i == 0 && (mapaBarcos[fila - 1][columna + 1 + i] == 1

                                                                                            || mapaBarcos[fila + 1][columna + 1 + i] == 1))

                                                                                || (columna + i == 0 && mapaBarcos[fila + 1][columna + i] == 1)

                                                                                    || (columna + i == 0 && mapaBarcos[fila - 1][columna + i] == 1)
                                                                                    
                                                                                    || (columna + i == 0 && mapaBarcos[fila][columna + i + 1] == 1)

                                                                                        || (columna + i == 9 && (mapaBarcos[fila - 1][columna + i - 1] == 1

                                                                                                            || mapaBarcos[fila + 1][columna + i - 1] == 1))

                                                                                                || (columna + i == 9 && mapaBarcos[fila + 1][columna + i] == 1)

                                                                                                    || (columna + i == 9 && mapaBarcos[fila - 1][columna + i] == 1)
                                                                                                    
                                                                                                    || (columna + i == 9 && mapaBarcos[fila][columna + i - 1] == 1) )))
                                        
                                        || ((fila == 0 || fila == 9) && ( columna + i != 0 && columna + i != 9)

                                                            && (( mapaBarcos[fila][columna - 1 + i] == 1 )

                                                                || ( mapaBarcos[fila][columna + 1 + i] == 1 )))

                                        || ((fila != 0 && fila != 9 && columna + i != 0 && columna + i != 9)

                                                && (mapaBarcos[fila + 1][columna + 1 + i] == 1

                                                    || mapaBarcos[fila - 1][columna - 1 + i] == 1

                                                        || mapaBarcos[fila + 1][columna - 1 + i] == 1

                                                            || mapaBarcos[fila - 1][columna + 1 + i] == 1

                                                                || mapaBarcos[fila][columna + 1 + i] == 1

                                                                    || mapaBarcos[fila][columna - 1 + i] == 1

                                                                        || mapaBarcos[fila + 1][columna + i] == 1

                                                                            || mapaBarcos[fila - 1][columna + i] == 1 
                                                                            
                                                                                || (mapaBarcos[fila + 1][columna + i] == 1)

                                                                                    || (mapaBarcos[fila][columna + 1 + i] == 1)
                                                                                        
                                                                                        || (mapaBarcos[fila -1] [columna + i ] == 1)
                                                                                            
                                                                                            || (mapaBarcos[fila][columna - 1 + i] == 1) ) )

                                        || (mapaBarcos[fila][columna + i] == 1)))
                        {
                            //horizontal

                            MessageBox.Show("No puede" +
                                    " colocar el barco ahí, choca con otro barco");

                            colocar = false;
                        }
                        else if ((orientacion == 1) &&

                                (((fila + i == 9) && ((columna == 9 && mapaBarcos[fila - 1 + i][columna - 1] == 1)

                                                        || (columna == 0 && mapaBarcos[fila - 1 + i][columna + 1] == 1)

                                                        || (columna != 0 && columna != 9 && (mapaBarcos[fila - 1 + i][columna - 1] == 1

                                                                                || mapaBarcos[fila - 1 + i][columna + 1] == 1 ))
                                                        
                                                        || (mapaBarcos[fila - 1 + i][columna] == 1)))

                                || ((fila + i == 0) && ((columna == 9 && mapaBarcos[fila + 1 + i][columna - 1] == 1)

                                                        || (columna == 0 && mapaBarcos[fila + 1 + i][columna + 1] == 1)

                                                        || (columna != 0 && columna != 9 && (mapaBarcos[fila + 1 + i][columna - 1] == 1

                                                                            || mapaBarcos[fila + 1 + i][columna + 1] == 1))

                                                        || (mapaBarcos[fila + 1 + i][columna] == 1)))

                                || ((fila + i != 9 && fila + i != 0) && ((( columna == 0 && (mapaBarcos[fila - 1 + i][columna + 1] == 1

                                                                                        || mapaBarcos[fila + 1 + i][columna + 1] == 1 ))

                                                                        || (columna == 0 && mapaBarcos[fila + 1 + i][columna] == 1)
                                                
                                                                            || (columna == 0 && mapaBarcos[fila - 1 + i][columna] == 1)
                                                        
                                                                    || (columna == 0 && mapaBarcos[fila + i][columna + 1] == 1))

                                                                || ((columna == 9 && (mapaBarcos[fila - 1 + i][columna - 1] == 1

                                                                            || mapaBarcos[fila + 1 + i][columna - 1] == 1))

                                                                        || (columna == 9 && mapaBarcos[fila + 1 + i][columna] == 1)

                                                                            || (columna == 9 && mapaBarcos[fila - 1 + i][columna] == 1)

                                                                                || (columna == 9 && mapaBarcos[fila + i][columna - 1] == 1))))

                                || ((fila + i == 0 || fila + i== 9) && ( columna != 0 && columna != 9)

                                                            && (( mapaBarcos[fila + i][columna - 1] == 1 )

                                                                || ( mapaBarcos[fila + i][columna + 1] == 1 )))

                                || ((fila + i != 0 && fila + i != 9 && columna != 0 && columna != 9)

                                        && (mapaBarcos[fila + 1 + i][columna + 1] == 1

                                            || mapaBarcos[fila - 1 + i][columna - 1] == 1

                                                || mapaBarcos[fila + 1 + i][columna - 1] == 1

                                                    || mapaBarcos[fila - 1 + i][columna + 1] == 1

                                                        || mapaBarcos[fila + i][columna + 1] == 1

                                                            || mapaBarcos[fila + i][columna - 1] == 1

                                                                || mapaBarcos[fila + 1 + i][columna] == 1

                                                                    || mapaBarcos[fila - 1 + i][columna] == 1
 
                                                                        || (mapaBarcos[fila + i + 1][columna] == 1)

                                                                            || (mapaBarcos[fila + i][columna + 1] == 1)

                                                                                || (mapaBarcos[fila + i - 1][columna] == 1)

                                                                                    || (mapaBarcos[fila + i][columna - 1] == 1) ) )

                                || (mapaBarcos[fila + i][columna] == 1) ))
                        {
                            //vertical

                            MessageBox.Show("No puede" +

                                    " colocar el barco ahí, choca con otro barco");

                            colocar = false;
                        }

                    }

                }
                if (colocar == true)
                {
                    numBarcos[Cb_barcos.SelectedIndex] -= 1;
                    TxtB_Barcos.Text = String.Format("Quedan {0} barcos por colocar.", numBarcos[Cb_barcos.SelectedIndex]);
                    imagen.Source = new BitmapImage(new Uri("Resources/fondoBarco.png", UriKind.RelativeOrAbsolute));
                    mapaBarcos[fila][columna] = 1;

                    if (orientacion == 0)
                    {
                        for (int i = 1; i < (Cb_barcos.SelectedIndex + 1); i++)
                        {
                            int col = columna + i;
                            var nombreImagen = "Fondo" + col + fila;
                            Image img = (Image) FindName(nombreImagen);
                            img.Source = new BitmapImage(new Uri("Resources/fondoBarco.png", UriKind.RelativeOrAbsolute));
                            mapaBarcos[fila][columna + i] = 1;
                        }
                    }
                    else if (orientacion == 1)
                    {
                        for (int i = 1; i < (Cb_barcos.SelectedIndex + 1); i++)
                        {
                            int fil = fila + i;
                            var nombreImagen = "Fondo" + columna + fil;
                            Image img = (Image)FindName(nombreImagen);
                            img.Source = new BitmapImage(new Uri("Resources/fondoBarco.png", UriKind.RelativeOrAbsolute));
                            mapaBarcos[fila + i][columna] = 1;
                        }
                    }
                }
            }
        }
    }
}