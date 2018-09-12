using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NearDubDetect
{
    class NearDubDetector
    {

        public NearDubDetector()
        {
            
        }

        public List<Shingle> FindShingles(string textinput)
        {
            string text = HtmlRemoval.StripTagsRegex(textinput);
            string fixedText = Regex.Replace(text, "[^a-zA-Z0-9% -]", string.Empty);
            string[] textsplit = fixedText.Split(' ');

            List<Shingle> returnlist = new List<Shingle>();

            for (int i = 3; i < textsplit.Length; i++)
            {
                Shingle temp = new Shingle();
                temp.words.Add(textsplit[i - 3]);
                temp.words.Add(textsplit[i - 2]);
                temp.words.Add(textsplit[i - 1]);
                temp.words.Add(textsplit[i]);
                returnlist.Add(temp);
            }

            return returnlist;
        }

        public List<int> FindHashNumber(string input)
        {
            double h = 0;
            double t;
            string temp;
            List<Shingle> shingles = FindShingles(input);
            List<Int32> returnlist = new List<int>();
            foreach (Shingle shingle in shingles)
            {
                foreach (string word in shingle.words)
                {
                    t = 0;
                    MD5 md5 = System.Security.Cryptography.MD5.Create();

                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(word);

                    byte[] hash = md5.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString());
                    }

                    temp = sb.ToString();

                    foreach (char ch in temp)
                    {
                        t += Convert.ToInt32(ch);
                    }

                    h += t;
                    /*
                    t = 0;
                    foreach (char ch in word)
                    {
                        t += Convert.ToInt32(ch);
                    }
                    h += t / word.Length;
                    */
                }
                returnlist.Add(Convert.ToInt32(h));
                h = 0;
            }

            return returnlist;
        }

        public List<int> BigShiftHash(List<int> input, int randomint)
        {
            List<int> returnlist = new List<int>();
            foreach (int item in input)
            {
                returnlist.Add(item ^ randomint);
            }
            return returnlist;
        }
        /*
        public List<int> FindHashNumber(string text)
        {
            List<Shingle> shingles = FindShingles(text);
            List<List<int>> texthashes = new List<List<int>>();
            List<int> returnlist = new List<int>();
            Random rand = new Random();

            for (int i = 0; i < 5; i++)
            {
                List<int> intermediateHash = new List<int>();
                foreach (Shingle item in shingles)
                {
                    intermediateHash.Add(FindHashNumber(item));
                }
                texthashes.Add(intermediateHash);
            }

            foreach (List<int> item in texthashes)
            {
                //returnlist.Add(item.Min());
                for (int i = 0; i < 84; i++)
                {
                    returnlist.Add(item[rand.Next(0,4)]);
                }
                for (int i = 0; i < 5; i++)
                {
                    returnlist.Add(item[i]);
                }
            }

            return returnlist;
        }*/

        public List<Int32> GenerateRandomIntegers()
        {
            List<Int32> liste = new List<Int32>();
            Random RNG = new Random();
            for (int i = 0; i < 84; i++)
            {
                liste.Add(RNG.Next(0,1000000));
            }
            return liste;
        }

        public double Jaccard(string text1input, string text2input)
        {
            List<Int32> randomList = GenerateRandomIntegers();
            List<int> text1 = FindHashNumber(text1input);
            List<int> text2 = FindHashNumber(text2input);
            double identicalcounter = 0;
            Int32 text1Hashes;
            Int32 text2Hashes;

            foreach (Int32 item in randomList)
            {
                text1Hashes = BigShiftHash(text1, item).Min();
                text2Hashes = BigShiftHash(text2, item).Min();
                if (text1Hashes == text2Hashes)
                {
                    identicalcounter++;
                }/*
                text1Hashes = BigShiftHash(text1, item).Max();
                text2Hashes = BigShiftHash(text2, item).Max();
                if (text1Hashes == text2Hashes)
                {
                    identicalcounter++;
                }*/
            }

            return (identicalcounter / 84) * 100;

            /*
            for (int i = 0; i < 5; i++)
            {
                if (text1[i] == text2[i])
                {
                    identicalcounter++;
                }
            }
            */
            foreach (int item in text1)
            {
                foreach (int item2 in text2)
                {
                    if (item == item2)
                    {
                        identicalcounter++;
                        break;
                    }
                }
            }
            System.Console.WriteLine(identicalcounter);
            if (identicalcounter == 0)
            {
                return 0;
            }
            return 100 * (identicalcounter / (25));
                     
        }
    }
}
