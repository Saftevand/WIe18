using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace NearDubDetect
{
    class RobotTXTHandler
    {
        public RobotTXTHandler()
        {

        }

        public Restriction FindRestrictions(string website)
        {
            bool scope = false;
            WebClient wc = new WebClient();
            byte[] raw = wc.DownloadData(website);
            string[] temp;
            List<string> result = new List<string>();
            Restriction restrictions = new Restriction();

            string webData = Encoding.UTF8.GetString(raw);

            //System.Console.WriteLine(webData);

            string[] lines = webData.Split('\n');
            foreach (string item in lines)
            {
                if (item.Contains("User-Agent: *"))
                {
                    scope = true;
                }
                else if (item.Contains("User-Agent: "))
                {
                    scope = false;
                }

                if (scope == true)
                {
                    if (item.Contains("Allow"))
                    {
                        temp = item.Split(' ');
                        restrictions.allow.Add(temp[1]);
                    }
                    else if (item.Contains("Disallow"))
                    {
                        temp = item.Split(' ');
                        restrictions.disallow.Add(temp[1]);
                    }
                    else if(item.Contains("Crawl-delay"))
                    {
                        temp = item.Split(' ');
                        restrictions.crawldelay = Convert.ToInt32(temp[1]);
                    }
                }
            }

            return restrictions;
        }
    }
}
