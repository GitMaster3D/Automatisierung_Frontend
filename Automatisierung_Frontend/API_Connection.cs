using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Api_Connection
{
    internal class API_Connection
    {
        static string URL = "http://127.0.0.1:8000";

        public struct Response
        {
            public string data;
            public HttpStatusCode statusCode;

            public Response(string data, HttpStatusCode statusCode)
            {
                this.data = data;
                this.statusCode = statusCode;
            }

            public override string ToString()
            {
                return data;
            }
        }


        #region GET
        /// <summary>
        /// Gets the Value from the given path
        /// </summary>
        public static async Task<Response> Get(string path, bool isFullURI = false)
        {
            string fullPath = isFullURI ? path : URL + path;

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(fullPath);



                string content = "";
                if (response.IsSuccessStatusCode)
                    content = await response.Content.ReadAsStringAsync();


                return new Response(content, response.StatusCode);
            }
        }
        #endregion


        #region POST
        /// <summary>
        /// Posts to the given path with the given data, converted to json
        /// </summary>
        public static async Task<Response> Post(string path, object data)
        {
            string fullPath = URL + path;

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(data); // Convert data to json
                Console.WriteLine(json);


                var response = await client.PostAsync(fullPath, new StringContent(json, Encoding.UTF8, "application/json"));




                string content = "";
                if (response.IsSuccessStatusCode)
                    content = await response.Content.ReadAsStringAsync();


                return new Response(content, response.StatusCode);
            }
        }
        #endregion


        #region NETWORK_SCAN
        static List<string> foundIps = new List<string>();
        static List<string> lastFoundIps = new List<string>();
        static Semaphore networkScanSemaphore = new(1, 1);


        /// <summary>
        /// Returns the last scanned ips
        /// </summary>
        public static string[] GetLastScanned()
        {
            lock (lastFoundIps)
            {
                var result = new string[lastFoundIps.Count];
                lastFoundIps.CopyTo(result);
                return result;
            }
        }




        /// <summary>
        /// Checks the local network for all ips of available apis 
        /// Has the possibility to set part of the ip if it is known, to increase search speed
        /// </summary>
        /// <returns>
        /// A list with all those ips
        /// </returns>
        public static string[] NetworkScan(int targetPort, out int found, string typeCNumber)
        {
            networkScanSemaphore.WaitOne();

            lock (foundIps)
            {
                lastFoundIps = foundIps;
                foundIps = new();

                string ip = "";
                string baseIp = GetLocalIPAddress();
                string subnet = baseIp.Substring(0, baseIp.LastIndexOf(".") + (typeCNumber == null ? 1 : -3));
                subnet += typeCNumber ?? "";
                subnet += ".";


                List<Thread> threads = new(256); // Store refrences to all started threads
                for (int i = 0; i < 256; i++)
                {
                    ip = subnet + i;

                    Console.WriteLine(ip);

                    Thread t = new Thread(APICheck);
                    t.Start(ip);

                    threads.Add(t);                    
                }

                // Sync
                foreach (var thread in threads)
                    thread.Join();

                lastFoundIps = foundIps;
                found = foundIps.Count;
            }


            networkScanSemaphore.Release();

            return GetLastScanned();
        }


        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


        static async void APICheck(object ip)
        {
            try
            {
                if (Ping((string)ip))
                {
                    foundIps.Add((string)ip);

                    var isAvailable = await IsApiAvailable("http://" + ip + ":8000");


                    if (isAvailable)
                    {
                        lock (foundIps)
                        {
                            foundIps.Add((string)ip);
                        }
                    }
                }


            }
            catch { }
        }



        /// <summary>
        /// Check if the api is hosted at the given uri
        /// </summary>
        public static async Task<bool> IsApiAvailable(string uri)
        {
            var response = (await Get(uri + "/Pingme", true)).data;
            dynamic responseJson = JsonConvert.DeserializeObject<dynamic>(response)!;


            return responseJson.API == "Water System";
        }


        /// <summary>
        /// Pings the given ip
        /// </summary>
        /// <returns>
        /// if the ping was succsessful
        /// </returns>
        public static bool Ping(string ip)
        {
            using (Ping ping = new Ping())
            {
                try
                {
                    var reply = ping.Send(ip, 150000);
                    return reply.Status == IPStatus.Success;
                }
                catch (PingException)
                {
                    return false;
                }
            }
        }

        public static PingReply Ping2(string ip)
        {
            using (Ping ping = new Ping())
            {

                

                    var reply = ping.Send(ip, 150000);
                    return reply;
            }
        }
        #endregion


        #region SET_URL
        public static void SetHost(string host, int port)
        {
            URL = "http://" + host + ":" + port;
        }
        #endregion
    }
}
