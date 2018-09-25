﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearDubDetect
{
    class Program
    {
        static void Main(string[] args)
        {
            Crawler crawler = new Crawler("https://www.heste-nettet.dk/");
            crawler.Crawl(20);
            /*
            foreach (var item in crawler.websites)
            {
                Console.WriteLine(item.currentPath);
            }
            */
            
            Console.WriteLine(crawler.websites.Count);
            Console.WriteLine(crawler.websites[1].HTMLContent);

            Console.ReadLine();
        }
    }
}
