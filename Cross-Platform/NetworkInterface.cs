using System;
using System.Net;

namespace TimeTracker
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    class NetworkInterface
    {
        public static byte[] GetBytes(String URL)
        {
            URL = URL.Replace("\r\n", "");
            URL = URL.Replace("\n", "");
            try
            {
                WebClient wc = new WebClient();

                byte[] output = wc.DownloadData(URL);
                wc.Dispose();
                return output;
            }
            catch (Exception ex)
            {
                ex.PrintStackTrace();
                return null;
            }
        }

        public static String GetString(String URL)
        {
            URL = URL.Replace("\r\n", "");
            URL = URL.Replace("\n", "");
            try
            {
                WebClient wc = new WebClient();

                String output = wc.DownloadString(URL);
                output = output.Replace("\n", "\r\n");
                output = output.Replace("\r\r", "\r");

                //System.Diagnostics.Debug.WriteLine("!" + output + "!");
                return output;
            }
            catch
            {
                return null;
            }
        }
    }
}