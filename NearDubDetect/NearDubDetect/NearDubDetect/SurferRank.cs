using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearDubDetect
{
    public class SurferRank
    {
        double[][] surferMatrix;

        public void generateSurferMatrix(List<Website> websites)
        {

            for (int i = 0; i < websites.Count; i++)
            {
                for (int h = 0; h < websites.Count; h++)
                {
                    surferMatrix[i][h] = websites
                }
            }
        }
    }
}
