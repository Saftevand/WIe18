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
        private Dictionary<string, TermIndexer> incidenceVector = new Dictionary<string, TermIndexer>();

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
                    if (!incidenceVector[token].Documents.Exists(doc => doc.DocumentID == idOfDocument))
                    {
                        incidenceVector[token].AddNewDocument(idOfDocument);
                    }
                    else
                    {
                        incidenceVector[token].Documents.Find(doc => doc.DocumentID == idOfDocument).IncrementTF();
                    }

                }
                else
                {
                    incidenceVector.Add(token, new TermIndexer());
                    incidenceVector[token].AddNewDocument(idOfDocument);
                }
            }
            foreach (var value in incidenceVector.Values)
            {
                value.Calc_tfidf();
            }
            return tokens;
        }

        public List<int> PassQuery(string inputQuery)
        {
            List<string> disectedQuery = GenerateTokens(inputQuery, -1);
            List<string> foundTokens = new List<string>();
            List<DocumentInfo> blacklistedDocuments = new List<DocumentInfo>();
            int queryTokenCounter = 0;
            foreach (string queryWord in disectedQuery)
            {
                if (queryWord == "*not*")
                {
                    if (queryTokenCounter != disectedQuery.Count)
                    {
                        blacklistedDocuments.AddRange(incidenceVector[queryWord].Documents);
                    }
                }
                if (incidenceVector.ContainsKey(queryWord) && !foundTokens.Contains(queryWord))
                {
                    foundTokens.Add(queryWord);
                }
                queryTokenCounter++;
            }



            List<int> foundPages = new List<int>();
            foundPages.AddRange(CosineScoreCalculator(foundTokens, ORpageFinder(foundTokens)));
            foundPages.AddRange(ANDpageFinder(foundTokens));
            foundPages.AddRange(ORpageFinder(foundTokens));
            foundPages = foundPages.Distinct().ToList();
            foundPages.RemoveAll(e => blacklistedDocuments.Exists(bDoc => bDoc.DocumentID == e));

            foundPages.ForEach(e => Console.WriteLine(e));

            return foundPages;
        }

        public List<int> CosineScoreCalculator(List<string> queryTokens, List<int> pagesContainingQuery)
        {
            Dictionary<int, double> score = new Dictionary<int, double>();
            foreach (int docID in pagesContainingQuery)
            {
                double tfStarExpSum = 0;
                foreach (string qToken in queryTokens)
                {
                    DocumentInfo docInfo = incidenceVector[qToken].Documents.Find(e => e.DocumentID == docID);
                    
                    if(docInfo != null)
                    {
                        tfStarExpSum += Math.Pow(docInfo.TermFrequencyStar, 2);
                    }
                }
                tfStarExpSum = Math.Sqrt(tfStarExpSum);
                foreach (string qToken in queryTokens)
                {
                    DocumentInfo docInfo = incidenceVector[qToken].Documents.Find(e => e.DocumentID == docID);

                    if (docInfo != null)
                    {
                        docInfo.Normalised = docInfo.TermFrequencyStar / tfStarExpSum;
                    }
                }
                double totalScore = 0;
                foreach (string qToken in queryTokens)
                {
                    DocumentInfo docInfo = incidenceVector[qToken].Documents.Find(e => e.DocumentID == docID);
                    DocumentInfo queryInfo = incidenceVector[qToken].Documents.Find(e => e.DocumentID == -1);
                    
                    if (docInfo != null && queryInfo != null)
                    {
                        totalScore += docInfo.Normalised * (queryInfo.TermFrequencyStar * incidenceVector[qToken].InverseDF);
                    }
                }
                score.Add(docID, totalScore);
            }

             List<KeyValuePair<int,double>> resultingPagesSorted = score.ToList();
             resultingPagesSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
             List<int> resultingPages = new List<int>();
            foreach (KeyValuePair<int,double> kvPair in resultingPagesSorted)
            {
                resultingPages.Add(kvPair.Key);
            }
            return resultingPages;

        }


        public List<int> ANDpageFinder(List<string> queryTokens)
        {
            List<int> commonIndexes = new List<int>();
            List<int> firstDocIndexes = new List<int>(incidenceVector[queryTokens[0]].DocumentIDs());
            if (queryTokens.Count > 1)
            {
                commonIndexes = incidenceVector[queryTokens[1]].DocumentIDs().Intersect(firstDocIndexes).ToList();
                for (int i = 2; i < queryTokens.Count; i++)
                {
                    commonIndexes = incidenceVector[queryTokens[i]].DocumentIDs().Intersect(commonIndexes).ToList();
                }
                return commonIndexes;
            }
            else return firstDocIndexes;

        }



        public List<int> ORpageFinder(List<string> queryTokens)
        {
            List<int> pageIndexes = new List<int>();
            foreach (string queryToken in queryTokens)
            {
                pageIndexes.AddRange(incidenceVector[queryToken].DocumentIDs());
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
    }
}



