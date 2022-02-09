using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            var Server = new HttpListener();
            var ServerIp = "http://localhost:45679/";
            var CacheIp = "http://localhost:45680/";


            Server.Prefixes.Add(ServerIp);
            Server.Start();
            Console.WriteLine("I Am Main Server");

            while (true)
            {
                var client = Server.GetContext();
                Console.WriteLine($"{client.Request.RemoteEndPoint} Client Connected");
                var Request = client.Request;
                var Response = client.Response;
                var ClientReader = new StreamReader(Request.InputStream);


                var clientText = ClientReader.ReadToEnd();
                var Method = Request.HttpMethod;
                if (Request.HttpMethod == "POST")
                {
                    if (clientText.ToUpper().Split('-')[0] == "GET")
                        Method = "GET";
                    else if (clientText.ToUpper().Split('-')[0] == "DELETE")
                        Method = "DELETE";
                }


                var CacheServer = new HttpClient();
                switch (Method)
                {
                    case "GET":
                        var message = new HttpRequestMessage(HttpMethod.Post, CacheIp);

                        message.Content = new StringContent(clientText, Encoding.UTF8, "text/plain");

                        var CacheResponse = CacheServer.SendAsync(message).Result;
                        clientText = clientText.Split('-')[1];

                        if (CacheResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            System.Console.WriteLine("Cache Data Not Found");
                            var PersonValue = DbContext.Kvalues.Where(x => x.Key == clientText).FirstOrDefault();
                            if (PersonValue is null)
                            {
                                Console.WriteLine("Data Not Found In Database");
                                Response.StatusCode = 404;
                                Response.Close();
                                break;
                            }
                            System.Console.WriteLine("We Send Him Database Data");
                            try
                            {
                                var Value = keyValuePairs[clientText];
                                keyValuePairs[clientText] = Value + 1;
                                if (keyValuePairs[clientText] >= 3)
                                {
                                    var UpdateCacheDataResponse = CacheServer.PutAsync(CacheIp, new StringContent(PersonValue.Key + "-" + PersonValue.Value, Encoding.UTF8, "text/plain")).Result;
                                    if (UpdateCacheDataResponse.StatusCode == HttpStatusCode.OK)
                                        Console.WriteLine("Cache Updated");
                                    else if (UpdateCacheDataResponse.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        Console.WriteLine("Cache Update Not Successfull");
                                        var AddCacheDataResponse = CacheServer.PostAsync(CacheIp, new StringContent(PersonValue.Key + "-" + PersonValue.Value, Encoding.UTF8, "text/plain")).Result;
                                        if (AddCacheDataResponse.StatusCode == HttpStatusCode.OK)
                                            Console.WriteLine("Cache Added");

                                    }
                                }
                            }
                            catch (Exception)
                            {
                                keyValuePairs[clientText] = 1;
                            }


                            var ResponseWriter = new StreamWriter(Response.OutputStream);
                            ResponseWriter.WriteLine(PersonValue.Key + "-" + PersonValue.Value);
                            ResponseWriter.Flush();
                            Response.StatusCode = (int)HttpStatusCode.OK;
                            Response.Close();

                        }
                        else if (CacheResponse.StatusCode == HttpStatusCode.OK)
                        {
                            System.Console.WriteLine("Cache Data Found");

                            var CacheData = CacheResponse.Content.ReadAsStringAsync().Result;

                            var ResponseWriter = new StreamWriter(Response.OutputStream);
                            ResponseWriter.WriteLine(CacheData);
                            ResponseWriter.Flush();
                            Response.StatusCode = (int)HttpStatusCode.OK;
                            Response.Close();

                        }
                        break;
                    case "PUT":
                        var kvalue = DbContext.Kvalues.Where(x => x.Key == clientText.Split('-')[0]).FirstOrDefault();
                        if (kvalue is null)
                        {
                            Console.WriteLine("Data Base Not Updated");
                            Response.StatusCode = 404;
                            Response.Close();
                            return;
                        }
                        kvalue.Value = int.Parse(clientText.Split('-')[1]);
                        DbContext.SubmitChanges();
                        Console.WriteLine("Data Base Updated");

                        var CacheRes = CacheServer.PutAsync(CacheIp, new StringContent(clientText)).Result;
                        if (CacheRes.StatusCode == HttpStatusCode.OK)
                            Console.WriteLine("Cache Server Updated");
                        else
                            Console.WriteLine("Cache Server Not Updated");
                        Response.StatusCode = 200;
                        var rWriter = new StreamWriter(Response.OutputStream);
                        rWriter.WriteLine("Updated");
                        rWriter.Flush();
                        Response.Close();
                        break;
                    case "POST":
                        DbContext.Kvalues.InsertOnSubmit(new Kvalue { Key = clientText.Split('-')[0], Value = int.Parse(clientText.Split('-')[1]) });
                        DbContext.SubmitChanges();
                        Console.WriteLine("Data Base Added New Data");
                        Response.StatusCode = (int)HttpStatusCode.Created;
                        var rrWriter = new StreamWriter(Response.OutputStream);
                        rrWriter.WriteLine("New Data Added");
                        rrWriter.Flush();
                        Response.Close();
                        break;
                    case "DELETE":
                        var oldText = clientText;
                        clientText = clientText.Split('-')[1];
                        var DeleteThisPerson = DbContext.Kvalues.Where(x => x.Key == clientText.Split('-')[0]).FirstOrDefault();
                        if (DeleteThisPerson is null)
                        {
                            Console.WriteLine("Data Base Data Not Found For Delete");
                            Response.StatusCode = 404;
                            Response.Close();
                            return;
                        }
                        DbContext.Kvalues.DeleteOnSubmit(DeleteThisPerson);
                        Console.WriteLine("Data Deleted In Database");
                        var CacheRequest = new HttpRequestMessage(HttpMethod.Post, CacheIp);
                        var CacheResponse2 = CacheServer.PostAsync(CacheIp, new StringContent(oldText, Encoding.UTF8, "text/plain")).Result;
                        keyValuePairs.Remove(clientText);

                        if (CacheResponse2.StatusCode == HttpStatusCode.OK)
                            Console.WriteLine("Data Deleted In Cache Server");
                        else if (CacheResponse2.StatusCode == HttpStatusCode.NotFound)
                            Console.WriteLine("Data Not Deleted In Cache Server");
                        Response.StatusCode=200;
                        Response.Close();
                        break;
                }

            }


        }
    }
}
