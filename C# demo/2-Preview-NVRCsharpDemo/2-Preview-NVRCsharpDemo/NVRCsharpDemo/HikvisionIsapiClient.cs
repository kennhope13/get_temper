using System;
using System.IO;
using System.Net;
using System.Text;

namespace NVRCsharpDemo
{
    public class HikvisionIsapiClient
    {
        private readonly string _ip;
        private readonly NetworkCredential _credential;

        public HikvisionIsapiClient(string ip, string username, string password)
        {
            _ip = ip;
            _credential = new NetworkCredential(username, password);
        }

        public string Get(string path)
        {
            string url = "http://" + _ip + path;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Credentials = _credential;
            request.PreAuthenticate = false;
            request.Timeout = 5000;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}