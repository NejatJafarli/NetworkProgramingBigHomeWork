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
            var CacheIp = "http://localhost:45680/";


            var Server = new HttpListener();

            Server.Prefixes.Add(CacheIp);
            Server.Start();


            while (true)
            {

                var client = Server.GetContext();
                Console.WriteLine($"{client.Request.RemoteEndPoint} Client Connected");
                var Request = client.Request;
                var Response = client.Response;
                var ClientReader = new StreamReader(Request.InputStream);

                var Method = Request.HttpMethod;
                var ClientData = ClientReader.ReadToEnd();
                if (Method == "POST")
                {
                    if (ClientData.ToUpper().StartsWith("GET"))
                        Method = "GET";
                    if (ClientData.ToUpper().StartsWith("DELETE"))
                        Method = "DELETE";
                }

                switch (Method)
                {
                    case "GET":
                        System.Console.WriteLine("Cache Server Get Method");
                        try
                        {
                            ClientData = ClientData.Split('-')[1];
                            var CacheData = map[ClientData];
                            Console.WriteLine("Cache Data Found And We Returned Server");
                            Response.StatusCode = 200;
                            var ResponseWriter = new StreamWriter(Response.OutputStream);
                            ResponseWriter.WriteLine(ClientData + " " + CacheData);
                            ResponseWriter.Flush();
                            Response.Close();

                        }
                        catch (System.Exception)
                        {
                            Console.WriteLine("Cache Data Not Found");
                            Response.StatusCode = 404;
                            Response.Close();
                        }
                        break;
                    case "PUT":
                        System.Console.WriteLine("Cache Server Put Method");
                        try
                        {
                            var IsHave = map[ClientData.Split('-')[0]];
                            map[ClientData.Split('-')[0]] = int.Parse(ClientData.Split('-')[1]);
                            Response.StatusCode = 200;
                            Response.Close();

                        }
                        catch (Exception)
                        {
                            Response.StatusCode = 404;
                            Response.Close();
                        }
                        break;
                    case "POST":
                        System.Console.WriteLine("Cache Server Post Method");
                        try
                        {
                            var IsHave = map[ClientData.Split('-')[0]];
                            Response.StatusCode = 404;
                            Response.Close();

                        }
                        catch (Exception)
                        {
                            map[ClientData.Split('-')[0]] = int.Parse(ClientData.Split('-')[1]);
                            Response.StatusCode = 200;
                            Response.Close();
                        }
                        break;
                    case "DELETE":
                        ClientData = ClientData.Split('-')[1];
                         System.Console.WriteLine("Cache Server Delete Method");
                        try
                        {
                            var IsHave = map[ClientData];
                            map.Remove(ClientData);
                            Response.StatusCode = 200;
                            Response.Close();

                        }
                        catch (Exception)
                        {
                            Response.StatusCode = 404;
                            Response.Close();
                        }

                        break;
                }
            }
        }
    }
}
