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
        private Website _website;
        public List<Website> websites = new List<Website>();
        public Crawler(string seedURL)
        {
            _seedURL = seedURL;
        }


        public void Crawl(int amountOfPages)
        {
            processNewPage(_seedURL);
            while (queue.Count != 0)
            {
                websites.Add(queue.Dequeue());
            }
            while(queue.Count < amountOfPages)
            {
                if (queue.Count > 0)
                {
                    _website = queue.Dequeue();
                    websites.Add(_website);
                    processNewPage(_website.currentPath);
                }
                else break;
            }
        }


        public void processNewPage(string URL)
        {
            WebClient wc = new WebClient();
            byte[] raw = wc.DownloadData(URL);
            string webData = Encoding.UTF8.GetString(raw);

            HtmlWeb htmlweb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlweb.Load(URL);
            List<string> urls =  htmlDocument.DocumentNode.SelectNodes("//a[@href]").Select(i => i.GetAttributeValue("href", null)).ToList();
            List<string> banned = new List<string>();
            foreach (string item in urls)
            {
                if (item.Contains("facebook.com"))
                {
                    banned.Add(item);
                }
            }
            foreach (string item in banned)
            {
                urls.Remove(item);
            }

            string url1 = "";
            foreach (string url in urls)
            {
                //try
                //{
                    if (!url.Contains("www"))
                    {
                        url1 = URL.Remove(URL.Length-1,1) + url;
                    }
                    else
                    {
                        url1 = url;
                    }

                    Uri uri = new Uri(url1);
                    string domain = uri.Host;
                    Domain dom = domains.Find(x => x.URL == domain);
                    if (dom == null)
                    {
                        dom = new Domain(domain, RobotTXTHandler.FindRestrictions(domain));
                        domains.Add(dom);
                    }
                try
                {
                    raw = wc.DownloadData(url1);
                    webData = Encoding.UTF8.GetString(raw);
                    Website tempwebsite = new Website(dom, url1, webData);
                    if (!dom.restriction.disallow.Contains(tempwebsite.currentPath.Remove(0, tempwebsite.DomainURL.URL.Length)) && dom.URL != "www.facebook.com")
                    {
                        if (!queue.Contains(tempwebsite))
                        {
                            queue.Enqueue(tempwebsite);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                    
                    System.Threading.Thread.Sleep(dom.restriction.crawldelay * 1000);
                //}
                //catch (Exception)
                //{
                //    
                //}
                
                
            }


            //webData = Regex.Replace(webData, "[^a-zA-Z0-9% -]", string.Empty);

        }
    }
}
