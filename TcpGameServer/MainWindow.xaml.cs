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

namespace TcpGameServer
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool sessionToken;
        int time = 10000;
        float cps;
        int clicks;
        IPAddress ipAd;
        TcpListener myList;
        Socket s;

        public MainWindow()
        {
            InitializeComponent();

            ipAd = IPAddress.Parse("127.0.0.1");
            myList = new TcpListener(ipAd, 55000);

            txtDestPort.Text = "55000";
            txtIpAdd.Text = GetLocalIPAddress();
        }

        private async void ServerSide()
        {
            try
            {
                await Task.Run(() =>
                {
                    myList.Start();
                    s = myList.AcceptSocket();

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (s.Connected) // Controllo se i 2 host sono connessi.
                        {
                            lblConnection.Content = "CONNECTED";
                            btnConnect.IsEnabled = false;
                        }
                    }));

                    
                    while (true)
                    {
                        byte[] b = new byte[100];
                        string n = "";
                        int k = 0;
                        if (s.Available > 0) // se ci sono byte disponibili, li legge.
                        {
                            k = s.Receive(b); // Ricezione info.
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lblAvversario.Content = "Opponent: " + Encoding.ASCII.GetString(b);
                                n = lblMine.Content.ToString();
                            }));
                        }
                    }

                });

                myList.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore:\n" + ex.Message, "Server", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            ServerSide(); // Ricezione.
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtTime.Text, out int t)) // validazione tempo.
                    throw new Exception("Inserire un tempo valido.");
                time = t * 1000; // tempo in secondi.
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
                MessageBox.Show("Errore:\n" + ex.Message, "Server", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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
                    // Calcolo click.
                    cps = time / 1000;
                    cps = clicks / cps;

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lblMine.Content = "Click: " + clicks.ToString() + ", clicks per second: " + cps.ToString(); // Info sessione clicker.
                    }));

                    string n = "";
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        n = lblMine.Content.ToString();
                    }));
                    Thread.Sleep(100);

                    // Se la stringa da inviare non è vuota, allora invio i dati.
                    if(n != "")
                        SendData(n);

                    Thread.Sleep(3000);
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
            s.Send(Encoding.ASCII.GetBytes(n));
        }

        /// <summary>
        /// Creazione window e avvio thread per gestione click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread sn = new Thread(Session) { IsBackground = true }; // Thread per la gestione dei click e il send della info.
            sn.Start();
        }

        /// <summary>
        /// Release del click del mouse per incremento numero click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) // Se rilascia il button, incremento numero di click.
            {
                clicks++;
                lblClicks.Content = "Click: " + clicks.ToString();
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
