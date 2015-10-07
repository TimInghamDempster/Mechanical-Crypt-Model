using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class NormalDistributionRNG
    {
        Random m_uniformRNG;
        double m_mean;
        double m_sd;

        bool m_hasSecondOfPair;
        double m_secondOfPair;

        public NormalDistributionRNG(Random uniformRNG, double mean, double sd)
        {
            m_mean = mean;
            m_uniformRNG = uniformRNG;
            m_sd = sd;
            m_hasSecondOfPair = false;
        }

        public double Next()
        {
            if (m_hasSecondOfPair)
            {
                m_hasSecondOfPair = false;
                return m_secondOfPair;
            }
            else
            {
                m_hasSecondOfPair = true;

                double u;
                double v;
                double s;
                bool suitableUV = false;

                do
                {
                    u = (m_uniformRNG.NextDouble() * 2.0) - 1.0;
                    v = (m_uniformRNG.NextDouble() * 2.0) - 1.0;

                    s = u * u + v * v;

                    suitableUV = (s != 0.0) && (s < 1.0);
                } while (suitableUV == false);

                double factor = -2.0 * Math.Log(s) / s;
                factor = Math.Sqrt(factor);

                m_secondOfPair = v * factor * m_sd + m_mean;
                return u * factor * m_sd + m_mean;
            }
        }

        public void Test(System.IO.StreamWriter output)
        {
            int count = 1000000;

            List<double> generated = new List<double>(count);

            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < count; i++)
            {
                double val = Next();
                generated.Add(val);
                min = Math.Min(min, val);
                max = Math.Max(max, val);
            }

            int numBuckets = 100;
            List<int> counts = new List<int>(numBuckets);

            for (int i = 0; i < numBuckets; i++)
            {
                counts.Add(0);
            }

            foreach (double val in generated)
            {
                double offset = val - min;
                double scaled = offset / (max - min);
                int index = (int)(scaled * (double)(numBuckets - 1));

                counts[index]++;
            }

            foreach(int total in counts)
            {
                output.WriteLine(total);
            }
            output.Close();
        }
    }
}
