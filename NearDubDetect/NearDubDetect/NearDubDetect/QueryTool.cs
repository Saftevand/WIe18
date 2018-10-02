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
            private Dictionary<string, List<int>> incidenceVector = new Dictionary<string, List<int>>();

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
                        if (!incidenceVector[token].Contains(idOfDocument))
                        {
                            incidenceVector[token].Add(idOfDocument);
                        }

                    }
                    else
                    {
                        incidenceVector.Add(token, new List<int>());
                        incidenceVector[token].Add(idOfDocument);
                    }
                }

                return tokens;
            }

            public List<int> PassQuery(string inputQuery)
            {
                List<string> disectedQuery = new List<string>(inputQuery.ToLower().Split(' '));
                disectedQuery.RemoveAll(e => stopWords.Exists(sw => sw.Equals(e)));

                List<string> foundTokens = new List<string>();
                List<int> blacklistedDocuments = new List<int>();
                int queryTokenCounter = 0;
                foreach (string queryWord in disectedQuery)
                {
                    if (queryWord == "*not*") 
                    {
                        if(queryTokenCounter != disectedQuery.Count)
                        {
                            blacklistedDocuments.AddRange(incidenceVector[disectedQuery[queryTokenCounter + 1]]);
                        }
                    }
                    if (incidenceVector.ContainsKey(queryWord) && !foundTokens.Contains(queryWord))
                    {
                        foundTokens.Add(queryWord);
                    }
                    queryTokenCounter++;
                }
                List<int> foundPages = new List<int>();
                foundPages.AddRange(ANDpageFinder(foundTokens));
                foundPages.AddRange(ORpageFinder(foundTokens));
                foundPages = foundPages.Distinct().ToList();
                foundPages.RemoveAll(e => blacklistedDocuments.Contains(e));

                foundPages.ForEach(e => Console.WriteLine(e));

                return foundPages;
            }


            public List<int> ANDpageFinder(List<string> queryTokens)
            {
                List<int> commonIndexes = new List<int>();
                List<int> firstDocIndexes = new List<int>(incidenceVector[queryTokens[0]]);
                if (queryTokens.Count > 1)
                {
                    commonIndexes = incidenceVector[queryTokens[1]].Intersect(firstDocIndexes).ToList();
                    for (int i = 2; i < queryTokens.Count; i++)
                    {
                        commonIndexes = incidenceVector[queryTokens[i]].Intersect(commonIndexes).ToList();
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
                    pageIndexes.AddRange(incidenceVector[queryToken]);
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



