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
        NearDubDetector NearDubDetector = new NearDubDetector();
        private string _seedURL;
        private List<Domain> domains = new List<Domain>();
        private Queue<Website> queue = new Queue<Website>();
        private Website _website;
        public List<Website> websites = new List<Website>();
        bool add = true;
        public Crawler(string seedURL)
        {
            _seedURL = seedURL;
        }


        public void Crawl(int amountOfPages)
        {
            List<Website> tempWebsites = new List<Website>();
            ProcessNewPage(_seedURL);
            
            while(websites.Count < amountOfPages)
            {
                if (queue.Count > 0)
                {
                    _website = queue.Dequeue();

                    if (true)//_website.DomainURL.LastVisisted)
                    {
                        tempWebsites = new List<Website>();

                        foreach (Website item in websites)
                        {
                            if (!websites.Contains(_website) && add == false)
                            {
                                //if (NearDubDetector.Jaccard(item, _website) < 90)
                                {
                                    add = true;
                                }
                            }
                        }

                        if (add)
                        {
                            websites.Add(_website);
                            add = false;
                            _website.DomainURL.LastVisited = DateTime.Now;
                        }

                        Console.WriteLine("Queue count before: " + queue.Count);
                        Console.WriteLine("Websites" + websites.Count);
                        ProcessNewPage(_website.currentPath);
                        Console.WriteLine("Queue count after: " + queue.Count + "\n");
                    }
                    else
                    {
                        queue.Enqueue(_website);
                    }
                }
                else break;
            }
            
        }


        public void ProcessNewPage(string URL)
        {
            WebClient wc = new WebClient();
            byte[] raw = wc.DownloadData(URL);
            string webData = Encoding.UTF8.GetString(raw);

            HtmlWeb htmlweb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlweb.Load(URL);
            List<string> urls = new List<string>();
            try
            {
                urls = htmlDocument.DocumentNode.SelectNodes("//a[@href]").Select(i => i.GetAttributeValue("href", null)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
            

            string url1;
            foreach (string url in urls)
            {
                try
                {
                    //Console.WriteLine(url.IndexOf('h').ToString() + url.IndexOf('t').ToString() + url.IndexOf('p').ToString());
                    if (!url.Contains("www"))
                    {
                        if(url.IndexOf('h') == 0 && url.IndexOf('t') == 1 && url.IndexOf('p') == 3)
                        {
                            url1 = url;
                        }
                        else
                        {
                            url1 = URL.Remove(URL.Length - 1, 1) + url;
                        }
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
                
                    raw = wc.DownloadData(url1);
                    webData = Encoding.UTF8.GetString(raw);
                    Website tempwebsite = new Website(dom, url1, webData);
                    if (!dom.restriction.disallow.Contains(tempwebsite.currentPath.Remove(0, tempwebsite.DomainURL.URL.Length)))
                    {
                        if (!queue.Contains(tempwebsite) && !websites.Contains(tempwebsite))
                        {
                            queue.Enqueue(tempwebsite);
                        }
                    }
                    System.Threading.Thread.Sleep(dom.restriction.crawldelay * 1000);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                    
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
