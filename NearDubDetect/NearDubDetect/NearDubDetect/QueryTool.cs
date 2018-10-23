using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NearDubDetect
{
    class QueryTool
    {
        private Crawler _crawler;

        public QueryTool()
        {

        }
        public QueryTool(Crawler crawler)
        {
            _crawler = crawler;
        }

        private List<string> stopWords = new List<string>(new string[] { "-","?",":",";","a","about","above","after","again","against","all","am","an","and","any","are","aren't","as","at","be","because","been","before","being","below","between","both","but"
        ,"by"
        ,"can't"
        ,"cannot"
        ,"could"
        ,"couldn't"
        ,"did"
        ,"didn't"
        ,"do"
        ,"does"
        ,"doesn't"
        ,"doing"
        ,"don't"
        ,"down"
        ,"during"
        ,"each"
        ,"few"
        ,"for"
        ,"from"
        ,"further"
        ,"had"
        ,"hadn't"
        ,"has"
        ,"hasn't"
        ,"have"
        ,"haven't"
        ,"having"
        ,"he"
        ,"he'd"
        ,"he'll"
        ,"he's"
        ,"her"
        ,"here"
        ,"here's"
        ,"hers"
        ,"herself"
        ,"him"
        ,"himself"
        ,"his"
        ,"how"
        ,"how's"
        ,"i"
        ,"i'd"
        ,"i'll"
        ,"i'm"
        ,"i've"
        ,"if"
        ,"in"
        ,"into"
        ,"is"
        ,"isn't"
        ,"it"
        ,"it's"
        ,"its"
        ,"itself"
        ,"let's"
        ,"me"
        ,"more"
        ,"most"
        ,"mustn't"
        ,"my"
        ,"myself"
        ,"no"
        ,"nor"
        ,"not"
        ,"of"
        ,"off"
        ,"on"
        ,"once"
        ,"only"
        ,"or"
        ,"other"
        ,"ought"
        ,"our"
        ,"ours ourselves"
        ,"out"
        ,"over"
        ,"own"
        ,"same"
        ,"shan't"
        ,"she"
        ,"she'd"
        ,"she'll"
        ,"she's"
        ,"should"
        ,"shouldn't"
        ,"so"
        ,"some"
        ,"such"
        ,"than"
        ,"that"
        ,"that's"
        ,"the"
        ,"their"
        ,"theirs"
        ,"them"
        ,"themselves"
        ,"then"
        ,"there"
        ,"there's"
        ,"these"
        ,"they"
        ,"they'd"
        ,"they'll"
        ,"they're"
        ,"they've"
        ,"this"
        ,"those"
        ,"through"
        ,"to"
        ,"too"
        ,"under"
        ,"until"
        ,"up"
        ,"very"
        ,"was"
        ,"wasn't"
        ,"we"
        ,"we'd"
        ,"we'll"
        ,"we're"
        ,"we've"
        ,"were"
        ,"weren't"
        ,"what"
        ,"what's"
        ,"when"
        ,"when's"
        ,"where"
        ,"where's"
        ,"which"
        ,"while"
        ,"who"
        ,"who's"
        ,"whom"
        ,"why"
        ,"why's"
        ,"with"
        ,"won't"
        ,"would"
        ,"wouldn't"
        ,"you"
        ,"you'd"
        ,"you'll"
        ,"you're"
        ,"you've"
        ,"your"
        ,"yours"
        ,"yourself"
        ,"yourselves"});
        private Dictionary<string, DfWrapper> incidenceVector = new Dictionary<string, DfWrapper>();

        public List<string> GenerateTokens(string input, int idOfDocument)
            {
                string lowerCaseInput = input.ToLower();
                List<string> words = new List<string>(lowerCaseInput.Split(' '));
                words.RemoveAll(e => stopWords.Exists(sw => sw.Equals(e)));
                List<string> tokens = new List<string>();
                foreach (string word in words)
                {
                    tokens.Add(PorterStemming(word));
                }

                foreach (string token in tokens)
                {
                    if (incidenceVector.ContainsKey(token))
                    {

                    Document tempDoc = incidenceVector[token].DocumentList.Find(x => x.Id == idOfDocument);

                    if (tempDoc == null)
                       {
                           Document temp = new Document(idOfDocument);
                           incidenceVector[token].Df++;
                           incidenceVector[token].DocumentList.Add(temp);
                           temp.TermFrequency++;
                          
                       }
                       else
                       {
                           incidenceVector[token].Df++;
                           tempDoc.TermFrequency++;
                       }
                    

                    }
                    else
                    {
                        incidenceVector.Add(token, new DfWrapper(1, new Document(idOfDocument)));
                    incidenceVector[token].DocumentList.First().TermFrequency++;
                    }
                }

                return tokens;
            }

        public List<KeyValuePair<Document, double>> PassQuery(string inputQuery)
        {
            List<string> disectedQuery = new List<string>(inputQuery.ToLower().Split(' '));
            disectedQuery.RemoveAll(e => stopWords.Exists(sw => sw.Equals(e)));

            List<string> foundTokens = new List<string>();
            List<Document> blacklistedDocuments = new List<Document>();
            int queryTokenCounter = 0;
            foreach (string queryWord in disectedQuery)
            {
                if (queryWord == "*not*") 
                {
                    if(queryTokenCounter != disectedQuery.Count)
                    {
                        blacklistedDocuments.AddRange(incidenceVector[disectedQuery[queryTokenCounter + 1]].DocumentList);
                    }
                }
                if (incidenceVector.ContainsKey(queryWord) && !foundTokens.Contains(queryWord))
                {
                    foundTokens.Add(queryWord);
                }
                queryTokenCounter++;
            }
            DfWrapper foundPages = new DfWrapper();
            //foundPages.DocumentList.AddRange(ANDpageFinder(foundTokens));
            foundPages.DocumentList.AddRange(ORpageFinder(foundTokens));
            foundPages.DocumentList = foundPages.DocumentList.Distinct().ToList();
            foundPages.DocumentList.RemoveAll(e => blacklistedDocuments.Contains(e));

            //foundPages.DocumentList.ForEach(e => Console.WriteLine(e.Id));

            //REMEMBER 10 IS JUST CHOSEN RANDOMLY - Hardcode
            CalculateTf_Idf(foundPages.DocumentList.Count);

            List<KeyValuePair<Document, double>> cosscore = CalculateCosScores(foundTokens, foundPages);

           
            double[] pageranks = _crawler.PageRanker(0.10, 200);
            List<KeyValuePair<Document, double>> final = new List<KeyValuePair<Document, double>>();

            for (int i = 0; i < pageranks.Length; i++)
            {
                KeyValuePair<Document, double> temp = cosscore.Find(x => x.Key.Id == i);
                if (temp.Key != null)
                {
                    final.Add(new KeyValuePair<Document, double>(temp.Key, pageranks[i] * temp.Value));
                }
            }
            
            final.Sort((x,y) => x.Value.CompareTo(y.Value));
            final.Reverse();
            try
            {
                final = final.GetRange(0, 10);
            }
            catch (Exception)
            {
            }
            
            

            return final;
        }

        private List<KeyValuePair<Document, double>> CalculateCosScores(List<string> tokens, DfWrapper wrapper)
        {

            List<KeyValuePair<int, double>> scores = new List<KeyValuePair<int, double>>();
            
            foreach (Document doc in wrapper.DocumentList)
            {
                double tempVal = 0;
                foreach (string word in tokens)
                {
                    if(doc.TermFrequency != 0)
                    {
                        tempVal += (1 + Math.Log(doc.TermFrequency));
                    }
                }
            
                scores.Add(new KeyValuePair<int, double>(doc.Id, tempVal));
            }

            List<KeyValuePair<int, double>> length = new List<KeyValuePair<int, double>>();

            foreach (Document item in wrapper.DocumentList)
            {
                double temp = 0;
                foreach (string word in tokens)
                {
                    if (incidenceVector[word].DocumentList.Contains(item))
                    {
                        temp += item.TfIdf * item.TfIdf;
                    }
                }
                length.Add(new KeyValuePair<int,double>(item.Id, Math.Sqrt(temp)));
            }

            List<KeyValuePair<int, double>> finalScores = new List<KeyValuePair<int, double>>();

            foreach (KeyValuePair<int,double> item in scores)
            {
                double lengthVal = -1;
                lengthVal = length.Find(x => x.Key == item.Key).Value;
                if (lengthVal >= 0)
                {
                    finalScores.Add(new KeyValuePair<int, double>(item.Key, item.Value/lengthVal));
                }


            }

            finalScores = finalScores.OrderBy(x => x.Value).ToList();


            List<KeyValuePair<Document, double>> finalDocumentPairs = new List<KeyValuePair<Document, double>>();

            foreach (KeyValuePair<int, double> item in finalScores)
            {
                finalDocumentPairs.Add(new KeyValuePair<Document, double>(wrapper.DocumentList.Find(x => x.Id == item.Key), item.Value));
            }

            return finalDocumentPairs;
        }

        public List<Document> ANDpageFinder(List<string> queryTokens)
            {
                List<Document> commonIndexes = new List<Document>();
                List<Document> firstDocIndexes = new List<Document>(incidenceVector[queryTokens[0]].DocumentList);
                if (queryTokens.Count > 1)
                {
                    commonIndexes = incidenceVector[queryTokens[1]].DocumentList.Intersect(firstDocIndexes).ToList();
                    for (int i = 2; i < queryTokens.Count; i++)
                    {
                        commonIndexes = incidenceVector[queryTokens[i]].DocumentList.Intersect(commonIndexes).ToList();
                    }
                    return commonIndexes;
                }
                else return firstDocIndexes;

            }


        public List<Document> ORpageFinder(List<string> queryTokens)
            {
                List<Document> pageIndexes = new List<Document>();
                foreach (string queryToken in queryTokens)
                {
                    pageIndexes.AddRange(incidenceVector[queryToken].DocumentList);
                }
                pageIndexes.Sort((x, y) =>
                {
                    if (pageIndexes.Count(e => e == x) < pageIndexes.Count(e => e == y)) return 1;
                    else if (pageIndexes.Count(e => e == x) > pageIndexes.Count(e => e == y)) return -1;
                    else return 0;
                });

                pageIndexes = pageIndexes.Distinct().ToList();

                return pageIndexes;

            }


        public string PorterStemming(string inputWord)
            {
                Regex ssPattern = new Regex("sses$");
                Regex iesPattern = new Regex("ies");
                Regex ationalPattern = new Regex("ational");
                Regex tionalPattern = new Regex("tional");
                ssPattern.Replace(inputWord, "ss");
                iesPattern.Replace(inputWord, "i");
                ationalPattern.Replace(inputWord, "ate");
                tionalPattern.Replace(inputWord, "tion");

                return inputWord;

            }

        public void CalculateTf_Idf(int totalDocs)
        {
            foreach (KeyValuePair<string, DfWrapper> pair in incidenceVector)
            {
                foreach (Document doc in pair.Value.DocumentList)
                {
                    doc.TfIdf = doc.TermFrequency * Math.Log10(Convert.ToDouble( totalDocs) /Convert.ToDouble( pair.Value.Df));
                }
            }
        }

    }
}



