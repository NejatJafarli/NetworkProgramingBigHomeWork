using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cache_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();

            Console.WriteLine("I Am Cache Server");


            var ip = IPAddress.Loopback;
            var port = 1111;
            var Server = new TcpListener(ip, port + 1);
            Server.Start(100);

            while (true)
            {

                var client = Server.AcceptTcpClient();
                Console.WriteLine($"{client.Client.RemoteEndPoint} Client Connected");

                BinaryReader reader = new BinaryReader(client.GetStream());

                var ClientData = reader.ReadString();
                if (ClientData.Split('-')[0] == "Add")
                {
                    Console.WriteLine("Value Added ",ClientData.Split('-')[1]);
                    map[ClientData.Split('-')[1]] = int.Parse(ClientData.Split('-')[2]);
                }
                else
                {
                    BinaryWriter binaryWriter = new BinaryWriter(client.GetStream());
                    try
                    {
                        var CacheData = map[ClientData];
                        Console.WriteLine("Cache Data Found And We Returned Server");
                        binaryWriter.Write(CacheData.ToString());

                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Cache Data Not Found");
                        binaryWriter.Write("null");
                    }
                }


            }



        }
    }
}
