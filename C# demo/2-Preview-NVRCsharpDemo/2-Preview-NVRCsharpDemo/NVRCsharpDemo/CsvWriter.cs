using System;
using System.IO;
using System.Text;

namespace NVRCsharpDemo
{
    public static class CsvWriter
    {
        private static readonly string FilePath = "camera_metrics.csv";

        public static void Append(string timestamp, string ip, string channel, string temperature, string rawXml)
        {
            bool exists = File.Exists(FilePath);

            using (StreamWriter sw = new StreamWriter(FilePath, true, Encoding.UTF8))
            {
                if (!exists)
                {
                    sw.WriteLine("timestamp,ip,channel,temperature,raw_xml");
                }

                sw.WriteLine(
                    "\"" + Escape(timestamp) + "\"," +
                    "\"" + Escape(ip) + "\"," +
                    "\"" + Escape(channel) + "\"," +
                    "\"" + Escape(temperature) + "\"," +
                    "\"" + Escape(rawXml) + "\""
                );
            }
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\"", "\"\"");
        }
    }
}