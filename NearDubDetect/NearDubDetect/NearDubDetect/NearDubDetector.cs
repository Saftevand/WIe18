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
            randomList = GenerateRandomIntegers();
        }
        List<Website> knownwebsitees = new List<Website>();
        List<Int32> randomList = new List<int>();

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

        public double Jaccard(Website input1, Website input2)
        {
            string text1input = input1.HTMLContent;
            string text2input = input2.HTMLContent;
            List<int> text1 = new List<int>();
            List<int> text2 = new List<int>();
            List<int> shift1 = new List<int>(84);
            List<int> shift2 = new List<int>(84);

            if (input1.HTMLContent == "" || input1.HTMLContent == null)
            {
                if (input2.HTMLContent == "" || input2.HTMLContent == null)
                {
                    return 100;
                }
                else
                {
                    return 0;
                }
            }
            if (input2.HTMLContent == "" || input2.HTMLContent == null)
            {
                return 0;
            }

            if (knownwebsitees.Contains(input1))
            {
                shift1 = input1.Hashnumber;
            }
            else
            {
                text1 = FindHashNumber(text1input);
                for (int i = 0; i < randomList.Count; i++)
                {
                    shift1.Add(BigShiftHash(text1, randomList[i]).Min());
                }
                input1.Hashnumber = shift1;
                knownwebsitees.Add(input1);
            }

            if (knownwebsitees.Contains(input2))
            {
                shift2 = input2.Hashnumber;
            }
            else
            {
                text2 = FindHashNumber(text2input);
                for (int i = 0; i < randomList.Count; i++)
                {
                    shift2.Add(BigShiftHash(text2, randomList[i]).Min());
                }
                input2.Hashnumber = shift2;
                knownwebsitees.Add(input2);
            }

            double identicalcounter = 0;

            for (int i = 0; i < shift1.Count-1; i++)
            {
                if (shift1[i] == shift2[i])
                {
                    identicalcounter++;
                }
            }

            /*
            foreach (Int32 item in randomList)
            {
                text1Hashes = BigShiftHash(text1, item).Min();
                text2Hashes = BigShiftHash(text2, item).Min();
                if (text1Hashes == text2Hashes)
                {
                    identicalcounter++;
                }
            }*/

            return (identicalcounter / 84) * 100;
       
        }
    }
}
