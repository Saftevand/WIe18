using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearDubDetect
{
    public class DocumentInfo
    {
        private int _documentID;
        private int _termFrequency;
        private double _termFrequencyStar;
        private double _tfidf;

        public DocumentInfo(int documentID)
        {
            this.DocumentID = documentID;
        }

        public double Tfidf { get => _tfidf; set => _tfidf = value; }
        public double TermFrequencyStar { get => _termFrequencyStar; set => _termFrequencyStar = value; }
        public int TermFrequency { get => _termFrequency; set => _termFrequency = value; }
        public int DocumentID { get => _documentID; set => _documentID = value; }

        public void IncrementTF()
        {
            this.TermFrequency++;
            TermFrequencyStar = 1 + Math.Log10(TermFrequency);
        }
    }
}
