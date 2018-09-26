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
            Uri a = new Uri(_seedURL);

            Domain seeddomain = new Domain(a.Host, RobotTXTHandler.FindRestrictions(a.Host));
            domains.Add(seeddomain);
            Website seedwebsite = new Website(seeddomain, _seedURL);
            websites.Add(seedwebsite);
            ProcessNewPage(seedwebsite);
            
            while(websites.Count < amountOfPages)
            {
                if (queue.Count > 0)
                {
                    _website = queue.Dequeue();

                    if (DateTime.Now > _website.DomainURL.LastVisisted + new TimeSpan(0, 0, _website.DomainURL.restriction.crawldelay))
                    {
                        tempWebsites = new List<Website>();
                        ProcessNewPage(_website);

                        foreach (Website item in websites)
                        {
                            if (!websites.Contains(_website) && add == false)
                            {
                                if (NearDubDetector.Jaccard(item, _website) < 90)
                                {
                                    add = true;
                                }
                            }
                        }

                        if (add)
                        {
                            websites.Add(_website);
                            add = false;
                            _website.DomainURL.LastVisisted = DateTime.Now;
                        }

                        Console.WriteLine("Queue count before: " + queue.Count);
                        Console.WriteLine("Websites" + websites.Count);
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

        public void ProcessNewPage(Website inputwebsite)
        {
            string URL = inputwebsite.currentPath;
            HtmlWeb htmlweb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlweb.Load(URL);
            List<string> urls =  htmlDocument.DocumentNode.SelectNodes("//a[@href]").Select(i => i.GetAttributeValue("href", null)).ToList();
            List<string> banned = new List<string>();
            inputwebsite.HTMLContent = htmlDocument.Text;
            //websites.Find(x => x.currentPath == URL).HTMLContent = htmlDocument.Text;
            
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
                try
                {
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
               
                    Website tempwebsite = new Website(dom, url1);
                    if (!dom.restriction.disallow.Contains(tempwebsite.currentPath.Remove(0, tempwebsite.DomainURL.URL.Length)))
                    {
                        if (!queue.Contains(tempwebsite) && !websites.Contains(tempwebsite))
                        {
                            queue.Enqueue(tempwebsite);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }                
            }
        }
    }
}
