using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearDubDetect
{
    public class TermIndexer
    {
        private List<DocumentInfo> documents = new List<DocumentInfo>();
        private double _documentFrequency;
        private double _inverseDF;
        
        public double DocumentFrequency { get => _documentFrequency; set => _documentFrequency = value; }
        public double InverseDF { get => _inverseDF; set => _inverseDF = value; }
        public List<DocumentInfo> Documents { get => documents; set => documents = value; }

        public void AddNewDocument(int documentID)
        {
            Documents.Add(new DocumentInfo(documentID));
            IncrementDF();
        }

        private void IncrementDF()
        {
            _documentFrequency++;
            _inverseDF = Math.Log10(Crawler.numberOfDocuments / _documentFrequency);
        }

        public List<int> DocumentIDs()
        {
            List<int> resultList = new List<int>();
            foreach (DocumentInfo documentInfo in documents)
            {
                resultList.Add(documentInfo.DocumentID);
            }
            return resultList;
        }

        public void Calc_tfidf()
        {
            foreach (DocumentInfo document in documents)
            {
                document.Tfidf = _inverseDF * document.TermFrequencyStar;
            }
        }
    }
}
