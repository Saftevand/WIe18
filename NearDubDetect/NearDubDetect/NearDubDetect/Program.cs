using System;
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

            foreach (var item in crawler.websites)
            {
                Console.WriteLine(item.currentPath);
            }

            /*
            string et = "<h2>Contrary to popular belief, Lorem Ipsum is not simply random text.</h1> It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old.";
            string to = "Contrary test to popular belief, test Lorem psum is simply dummy text of the p Ipsum is not simply random text. It test has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old.";
            string tre = "polse polse polse polse polse";
            string tree = "is simply dummy text of the printing and typesetting industry.";
            string fire = "do not worry about your difficulties in mathematics";
            string firee = "i would not worry about your difficulties, you can easily learn what is needed.";
            string fem = "Contrary to popular zzzzz belief, Lorem Ipsum is not simply random zzzzz text. It has roots in a piece of classical Latin literature fuck from 45 BC, making it over 2000 fuck years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia, looked heck up one of the more obscure Latin words, consectetur, from a Lorem heckk Ipsum passage, and going through the cites of the word in classical literature, discovered the undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33 of de Finibus Bonorum et Malorum (The Extremes of Good and Evil) by Cicero, written in 45 BC. This book is a treatise on the theory of ethics, very popular during the Renaissance. The first line of Lorem Ipsum, Lorem ipsum dolor sit amet.., comes from a line in section 1.10.32.";
            string seks = "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia, looked up one of the more obscure Latin words, consectetur, from a Lorem Ipsum passage, and going through the cites of the word in classical literature, discovered the undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33 of de Finibus Bonorum et Malorum (The Extremes of Good and Evil) by Cicero, written in 45 BC. This book is a treatise on the theory of ethics, very popular during the Renaissance. The first line of Lorem Ipsum, Lorem ipsum dolor sit amet.., comes from a line in section 1.10.32.";

            NearDubDetector tester = new NearDubDetector();

            System.Console.WriteLine(tester.Jaccard(fire, firee) + "%");
            //System.Console.WriteLine(tester.Jaccard(fire, firee) + "%");
            //System.Console.WriteLine(tester.Jaccard(fire, firee) + "%");
            //System.Console.WriteLine(tester.Jaccard(fire, firee) + "%");
            //System.Console.WriteLine(tester.Jaccard(fire, firee) + "%");
            

            
            RobotTXTHandler RTHandler = new RobotTXTHandler();
            Restriction a = RTHandler.FindRestrictions("https://www.twitch.tv/robots.txt");

            Console.WriteLine("Allow: ");
            foreach (string item in a.allow)
            {
                System.Console.WriteLine(item);
            }
            Console.WriteLine("Disallow: ");
            foreach (string item in a.disallow)
            {
                System.Console.WriteLine(item);
            }

            System.Console.ReadLine(); 

            
            foreach (Shingle shingle in tester.FindShingles(tre))
            {
                foreach(string str in shingle.words)
                {
                    System.Console.WriteLine(str);

                }
            }*/
            Console.WriteLine(crawler.websites.Count);
            Console.WriteLine(crawler.websites[1].HTMLContent);

            Console.ReadLine();
        }
    }
}
