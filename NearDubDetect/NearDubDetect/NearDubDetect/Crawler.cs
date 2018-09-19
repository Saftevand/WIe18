using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace NearDubDetect
{
    class Crawler
    {
        RobotTXTHandler RobotTXTHandler = new RobotTXTHandler();
        private string _seedURL;
        private List<Domain> domains = new List<Domain>();
        private Queue<Website> queue = new Queue<Website>();        
        public Crawler(string seedURL)
        {
            _seedURL = seedURL;
        }


        public void processNewPage(string URL)
        {
            WebClient wc = new WebClient();
            byte[] raw = wc.DownloadData(URL);
            string webData = Encoding.UTF8.GetString(raw);

            HtmlWeb htmlweb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlweb.Load(URL);
            List<string> urls =  htmlDocument.DocumentNode.SelectNodes("//a[@href]").Select(i => i.GetAttributeValue("href", null)).ToList();

            foreach (string url in urls)
            {
                Uri uri = new Uri(url);
                string domain = uri.Host;
                Domain dom = domains.Find(x => x.URL == domain);
                if (dom == null)
                {
                    dom = new Domain(domain, RobotTXTHandler.FindRestrictions(domain));
                    domains.Add(dom);                    
                }
                raw = wc.DownloadData(url);
                webData = Encoding.UTF8.GetString(raw);
                Website tempwebsite = new Website(dom, url, webData);
                if (!queue.Contains(tempwebsite))
                {
                    queue.Enqueue(tempwebsite);
                }
            }


            //webData = Regex.Replace(webData, "[^a-zA-Z0-9% -]", string.Empty);

        }
    }
}
