using System;
using System.Net;

namespace GPhotosBackup.Analytics
{
    public class Heartbeat
    {
        public static void Beat()
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.QueryString.Add("application", Parameters.ANALYTICS_ID);

                try
                {
                    webClient.DownloadStringAsync(new Uri(Parameters.HEARTBEAT_URL));
                }
                catch (Exception)
                {
                    //Error in sending heartbeat is no reason to crash the application, so simply ignore this exception
                }
            }
        }
    }
}
