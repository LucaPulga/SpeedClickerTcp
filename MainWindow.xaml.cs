/*
 * Sviluppatore: Pulga Luca;
 * Classe: 4^L
 * Data di consegna: 2021/05/17
 * Scopo: Utilizzando le classi TCPClient e TCPListener realizzare un semplice gioco 
 * (carte, dadi o altro) utilizzando il protocollo TCP (che a differenza dell'UDP 
 * visto oggi richiede la creazione di una connessione).
 * APP: SPEED CLICKER with TCP protocol
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace TcpGameClient
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient client;
        Socket socket;
        bool sessionToken;
        int time = 10000;
        float cps;
        int clicks;

        public MainWindow()
        {
            InitializeComponent();

            txtDestPort.Text = "55000";
            txtIpAdd.Text = GetLocalIPAddress();
            txtTime.Text = "0";
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            ClientSide(); // Ricezione.
        }

        private async void ClientSide()
        {
            try
            {
                await Task.Run(() =>
                {
                    client = new TcpClient("127.0.0.1", 55000);
                    //client.Connect("127.0.0.1", 55000);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (client.Connected) // Controllo se i 2 host sono connessi.
                        {
                            lblConnection.Content = "CONNECTED"; 
                            btnConnect.IsEnabled = false;
                        }
                    }));
                    
                    while (true)
                    {
                        string textToTransmit = "";
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            textToTransmit = lblMine.Content.ToString();
                        }));
                        socket = client.Client; // Parte client.

                        byte[] ba = Encoding.ASCII.GetBytes(textToTransmit); // string to byte to send.
                        

                        byte[] bb = new byte[100];
                        int j = socket.Receive(bb, bb.Length, 0); // Ricezione dati.

                        if(j > 0)
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            lblAvversario.Content = "Opponent: " + Encoding.ASCII.GetString(bb);
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore:\n" + ex.Message, "Client", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        private void Session()
        {
            while (true)
            {
                if (sessionToken == true)
                {
                    Thread.Sleep(time);
                    sessionToken = false;
                    cps = time / 1000;
                    cps = clicks / cps;

                    string n = "";

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lblMine.Content = "Click: " + clicks.ToString() + ", clicks per second: " + cps.ToString();
                    }));

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        n = lblMine.Content.ToString();
                    }));

                    Thread.Sleep(100);
                    SendData(n);

                    Thread.Sleep(2000);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        btnStart.Visibility = Visibility.Visible;
                    }));
                }
            }
        }

        /// <summary>
        /// Invio dei dati.
        /// </summary>
        /// <param name="n"></param>
        private void SendData(string n)     
        {
           socket.Send(Encoding.ASCII.GetBytes(n));
        }

        /// <summary>
        /// Creazione window e avvio thread per gestione click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread sn = new Thread(Session) { IsBackground = true }; // Se rilascia il button, incremento numero di click.
            sn.Start();
        }

        /// <summary>
        /// elease del click del mouse per incremento numero click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                clicks++;
                lblClicks.Content = "Click: " + clicks.ToString();
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtTime.Text, out int t))
                    throw new Exception("Inserire un tempo valido.");
                time = t * 1000;
                if (sessionToken == false)
                {
                    clicks = 0;
                    sessionToken = true;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        btnStart.Visibility = Visibility.Hidden;
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore :\n" + ex.Message, "Client", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }

        /// <summary>
        /// get automated local ip.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            try
            {
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST  
                // Get the IP  
                string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString(); // Get local ip address

                Uri uri = new Uri("http://" + myIP);

                return uri.Host.ToString(); // return dell'ip dell'host.
            }
            catch (Exception ex)
            {
                throw new Exception("No network adapters with an IPv4 address in the system!");
            }
        }

        /// <summary>
        /// control ipv4 address.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static bool IsIPv4(string ipAddress)
        {
            return Regex.IsMatch(ipAddress, @"^\d{1,3}(\.\d{1,3}){3}$") && ipAddress.Split('.').SingleOrDefault(s => int.Parse(s) > 255) == null; // Regex per controllare ip.
        }

    }
}
