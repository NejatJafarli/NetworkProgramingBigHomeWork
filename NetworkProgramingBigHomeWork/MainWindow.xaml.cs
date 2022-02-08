using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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

namespace NetworkProgramingBigHomeWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public HttpClient Client { get; set; } = new HttpClient();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Client = new HttpClient();
            var ServerIp = "http://localhost:45679/";

            // BinaryWriter br = new BinaryWriter(Client.GetStream());
            // br.Write(txt.Text);
            // BinaryReader binaryReader = new BinaryReader(Client.GetStream());
            // var Value = binaryReader.ReadString();
            // MessageBox.Show($"{txt.Text} -- {Value}");
            if (!string.IsNullOrWhiteSpace(txt.Text))
            {
                if (txt.Text.ToUpper().Split('-')[0] == "GET")
                {
                    var message = new HttpRequestMessage(HttpMethod.Post, ServerIp);
                    message.Content = new StringContent(txt.Text, Encoding.UTF8, "text/plain");
                    var response = Client.SendAsync(message).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        MessageBox.Show(content);
                    }
                    else
                    {
                        txt.Text = "Not Found";
                    }
                }
                else if (txt.Text.ToUpper().Split('-')[0] == "POST")
                {
                    var message = new HttpRequestMessage(HttpMethod.Post, ServerIp);
                    message.Content = new StringContent(txt.Text.Split('-')[1] + "-" + txt.Text.Split('-')[2], Encoding.UTF8, "text/plain");
                    var response = Client.SendAsync(message).Result;
                    if (response.StatusCode == HttpStatusCode.Created)
                        MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                        MessageBox.Show("Not Found");
                }
                else if (txt.Text.ToUpper().Split('-')[0] == "DELETE")
                {
                    var message = new HttpRequestMessage(HttpMethod.Delete, ServerIp);
                    message.Content = new StringContent(txt.Text.Split('-')[1], Encoding.UTF8, "text/plain");
                    var response = Client.SendAsync(message).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                        MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                        MessageBox.Show("Not Found");
                }
                else if (txt.Text.ToUpper().Split('-')[0] == "PUT")
                {
                    var message = new HttpRequestMessage(HttpMethod.Put, ServerIp);
                    message.Content = new StringContent(txt.Text.Split('-')[1] + "-" + txt.Text.Split('-')[2], Encoding.UTF8, "text/plain");
                    var response = Client.SendAsync(message).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                        MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                        MessageBox.Show("Not Found");
                }
                else
                {
                    txt.Text = "Write Correct HTTP Method";
                    MessageBox.Show("Template: Method-PersonName-Value");
                }
            }
            else
            {
                MessageBox.Show("Text Is empty");
            }
        }
    }
}
