using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace DownloadFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WebClient client;

        public MainWindow()
        {
            InitializeComponent();

            client = new WebClient();
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadStringCompleted += Client_DownloadStringCompleted;

            Copyright.Content = string.Format("By Rostislav Z © {0}", DateTime.Now.Year);
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = Source.Text;

            if (!string.IsNullOrEmpty(url))
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        Uri uri = new Uri(url);
                        client.DownloadStringAsync(uri);

                        VisibilityElement(false, "Downloading...");
                    }
                    catch (Exception error)
                    {
                        VisibilityElement(true, string.Format("ERROR: {0}", error.Message));
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                thread.Start();
            }
            else
            {
                MessageBox.Show("Invalid empty URL!", "Oops", MessageBoxButton.OK, MessageBoxImage.Warning);
                VisibilityElement(true, "");
            }
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Thread th = new Thread(() =>
                {
                    VisibilityElement(true, e.Result);
                });

                th.Start();
            }
            catch (Exception error)
            {
                VisibilityElement(true, string.Format("ERROR: {0}", error.Message));
                MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() => 
            {
                double receive = double.Parse(e.BytesReceived.ToString());
                double total = double.Parse(e.TotalBytesToReceive.ToString());
                ProgressBar.Value = receive / total * 100;
            });
        }

        private void VisibilityElement(bool show, string text)
        {
            Regex rError = new Regex("^ERROR:");
            Regex rDownloading = new Regex("^Downloading...$");

            Dispatcher.Invoke(() =>
            {
                Download.IsEnabled = show;
                ProgressBar.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
                Text.Text = text;

                if (rError.IsMatch(text)) Text.Foreground = Brushes.Red;
                else if (rDownloading.IsMatch(text)) Text.Foreground = Brushes.Green;
                else
                {
                    Text.Foreground = Brushes.Blue;
                    ProgressBar.Value = 0;
                }
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.WindowState = this.WindowState;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Save(); 
        }
    }
}
