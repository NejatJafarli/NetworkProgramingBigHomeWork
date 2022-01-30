using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {

        static void Main(string[] args)
        {
            Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
            DataClassesDataContext DbContext = new DataClassesDataContext();

            Console.WriteLine("I Am Main Server");
            var ip = IPAddress.Loopback;
            var port = 1111;
            var Server = new TcpListener(ip, port);
            Server.Start(100);

            while (true)
            {
                var client = Server.AcceptTcpClient();
                Console.WriteLine($"{client.Client.RemoteEndPoint} Client Connected");

                var clientStream = client.GetStream();

                BinaryReader ClientReader = new BinaryReader(clientStream);

                var clientText = ClientReader.ReadString();

                TcpClient CacheServer = new TcpClient();
                CacheServer.Connect(ip, port + 1);

                BinaryWriter CacheServerWriter = new BinaryWriter(CacheServer.GetStream());

                CacheServerWriter.Write(clientText);

                BinaryReader CacheServerReader = new BinaryReader(CacheServer.GetStream());

                var CacheData = CacheServerReader.ReadString();

                BinaryWriter ClientWriter = new BinaryWriter(clientStream);
                if (CacheData == "null")
                {
                    Console.WriteLine("We Send Him Database Data");

                    var PersonValue = DbContext.Kvalues.Where(x => x.Key == clientText).Select(x => x.Value).FirstOrDefault();
                    try
                    {
                        var Value = keyValuePairs[clientText];
                        keyValuePairs[clientText] = Value + 1;
                        if (keyValuePairs[clientText] >= 3)
                        {
                            TcpClient AddValueCacheServerClient = new TcpClient();
                            AddValueCacheServerClient.Connect(ip, port + 1);
                            BinaryWriter AddValueCacheServerWriter = new BinaryWriter(AddValueCacheServerClient.GetStream());
                            AddValueCacheServerWriter.Write($"Add-{clientText}-{PersonValue}");

                        }
                    }
                    catch (Exception)
                    {
                        keyValuePairs[clientText] = 1;
                    }
                    //send database and catch
                    ClientWriter.Write(PersonValue.ToString());

                }
                else
                {
                    Console.WriteLine("We Send Him Cache Data");
                    ClientWriter.Write(CacheData);
                }
            }


        }
    }
}
