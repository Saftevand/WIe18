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

            while (websites.Count < amountOfPages)
            {
                if (queue.Count > 0)
                {
                    _website = queue.Dequeue();

                    if (DateTime.Now > _website.DomainURL.LastVisited + new TimeSpan(0, 0, _website.DomainURL.restriction.crawldelay))
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
                            _website.DomainURL.LastVisited = DateTime.Now;
                        }

                        Console.WriteLine("Website count: " + websites.Count);
                        Console.WriteLine("Queue count: " + queue.Count + "\n");
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
            try
            {
                string URL = inputwebsite.currentPath;
                HtmlWeb htmlweb = new HtmlWeb();
                HtmlDocument htmlDocument = htmlweb.Load(URL);
                List<string> urls = new List<string>();
                try
                {
                    urls = htmlDocument.DocumentNode.SelectNodes("//a[@href]").Select(i => i.GetAttributeValue("href", null)).ToList();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                }
                List<string> banned = new List<string>();
                inputwebsite.HTMLContent = htmlDocument.Text;
                //websites.Find(x => x.currentPath == URL).HTMLContent = htmlDocument.Text;

                foreach (string item in urls)
                {
                    if (item.Contains("facebook.com") || item.ToLower().Contains(".pdf"))
                    {
                        banned.Add(item);
                    }
                }
                foreach (string item in banned)
                {
                    urls.Remove(item);
                }



                string url1;
                string httpstring = "http";
                foreach (string url in urls)
                {
                    try
                    {
                        //Console.WriteLine(url.IndexOf('h').ToString() + url.IndexOf('t').ToString() + url.IndexOf('p').ToString());
                        if (!url.Contains("www"))
                        {
                            if (url.IndexOf('h') == 0 && url.IndexOf('t') == 1 && url.IndexOf('p') == 3)
                            {
                                url1 = url;
                            }
                            else if (url[0] == '/' && url[1] == '/')
                            {
                                url1 = httpstring + url;
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

                        Website tempwebsite = new Website(dom, url1);
                        if (!dom.restriction.disallow.Contains(tempwebsite.currentPath.Remove(0, tempwebsite.DomainURL.URL.Length)))
                        {
                            if (!queue.Contains(tempwebsite) && !websites.Contains(tempwebsite))
                            {
                                tempwebsite.LinkedFrom.Add(inputwebsite);
                                queue.Enqueue(tempwebsite);
                            }
                            else if(websites.Contains(tempwebsite))
                            {
                                websites.Find(x => x == tempwebsite).LinkedFrom.Add(inputwebsite);
                            }
                            else if (queue.Contains(tempwebsite))
                            {
                                queue.ElementAt(queue.ToArray().ToList().IndexOf(tempwebsite)).LinkedFrom.Add(inputwebsite);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {

                //Console.WriteLine(e.Message);
            }

        }

        public double[] PageRanker(double alpha, int HowMuchRanking)
        {

            int sizeOfMatrix = websites.Count;
            double[ , ] transitionProbabilityMatrix = new double[sizeOfMatrix, sizeOfMatrix];

            int[] countArray= new int[sizeOfMatrix];

            // initializes matrix with 1s and 0s
            for (int i = 0; i < sizeOfMatrix; i++)
            {
                for (int k = 0; k < sizeOfMatrix; k++)
                {
                    if(websites[k].LinkedFrom.Exists(x => x == websites[i]))
                    {
                        transitionProbabilityMatrix[i, k] = 1;
                        countArray[i] += 1;
                    }
                    else
                    {
                        transitionProbabilityMatrix[i, k] = 0;
                    }
                }
            }

            // Calculates the fractions for each predecessor
            for (int i = 0; i < sizeOfMatrix; i++)
            {
                for (int k = 0; k < sizeOfMatrix; k++)
                {
                    if (transitionProbabilityMatrix[i,k] == 1 && !(countArray[i] == 0))
                    {
                        transitionProbabilityMatrix[i, k] = Convert.ToDouble( 1) / Convert.ToDouble( countArray[i]);
                    }
                }
            }

            // initialize surfer
            double[] surfer1 = new double[sizeOfMatrix];
            Random rng = new Random();

            surfer1[rng.Next(0, sizeOfMatrix - 1 )] = 10000;
            

            // init teleport
            for (int i = 0; i < sizeOfMatrix; i++)
            {
                for (int k = 0; k < sizeOfMatrix; k++)
                {
                    transitionProbabilityMatrix[i, k] = ((1 - alpha) * transitionProbabilityMatrix[i,k]) + (alpha * (Convert.ToDouble( 1) / Convert.ToDouble( sizeOfMatrix)));
                }
            }

            double val = 0;
            // Do pageranking
            for (int i = 0; i < HowMuchRanking; i++)
            {
                double[] surfer2 = new double[sizeOfMatrix];
                for (int k = 0; k < sizeOfMatrix; k++)
                {
                    val = 0;
                    for (int j = 0; j < sizeOfMatrix; j++)
                    {
                        val += surfer1[j] * transitionProbabilityMatrix[j, k];
                    }
                    surfer2[k] = val;
                }
                surfer1 = surfer2;
            }
            return surfer1;
        }
    }
}