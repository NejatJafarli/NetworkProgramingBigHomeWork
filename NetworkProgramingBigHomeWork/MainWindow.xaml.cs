using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace NetworkProgramingBigHomeWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TcpClient Client { get; set; } = new TcpClient();
        public MainWindow()
        {
            InitializeComponent();




        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Client=new TcpClient();
            var ip = IPAddress.Loopback;
            var port = 1111;
            Client.Connect(ip, port);
            if (!string.IsNullOrWhiteSpace(txt.Text))
            {
                BinaryWriter br = new BinaryWriter(Client.GetStream());
                br.Write(txt.Text);
                BinaryReader binaryReader = new BinaryReader(Client.GetStream());
                var Value = binaryReader.ReadString();
                MessageBox.Show($"{txt.Text} -- {Value}");
            }
            else
            {
                MessageBox.Show("Text Is empty");
            }
        }
    }
}
