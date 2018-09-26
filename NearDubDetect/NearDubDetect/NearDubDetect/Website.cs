using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearDubDetect
{
    class Website
    {
        public Website(Domain domain, string url, string content)
        {
            DomainURL = domain;
            currentPath = url;
            HTMLContent = content;
        }
        public Domain DomainURL;
        public string currentPath;
        public string HTMLContent;
        public List<int> Hashnumber;

        public override bool Equals(Object obj)
        {
            if ((obj as Website).currentPath == this.currentPath)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
