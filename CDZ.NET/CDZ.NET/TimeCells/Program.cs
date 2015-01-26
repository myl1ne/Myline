using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeCells
{
    class Program
    {
        class Sequence : List<double[]>
        {
        }

        static void Main(string[] args)
        {
            Pool pool = new Pool(5, 10);

            double[] a = { 1, 0, 0, 0, 0 };
            double[] b = { 0, 1, 0, 0, 0 };
            double[] c = { 0, 0, 1, 0, 0 };
            double[] d = { 0, 0, 0, 1, 0 };
            double[] e = { 0, 0, 0, 0, 1 };

            Sequence abcde = new Sequence() { a, b, c, d, e };
            Sequence ebcda = new Sequence() { e, b, c, d, a };
            Sequence ab = new Sequence() { a, b};
            Sequence cd = new Sequence() { c, d};

            List<Sequence> sequencesSet = new List<Sequence>(){ab,cd};
            
            for (int i = 0; i < 500000; i++)
            {
                foreach(Sequence s in sequencesSet)
                {
                    bool training = true;
                    if (training)
                    {
                        string errorMsg = "Sequence ";
                        double seqMeanError = 0.0;
                        foreach (double[] item in s)
                        {
                            pool.setInput(item); 

                            if (item == s.First())
                            {
                                pool.Update(1.0, 0.0, true);
                            }
                            else
                            {
                                pool.Update(0.3, 0.7, true);
                            }
                            double itemError = pool.error(ref errorMsg);
                            seqMeanError += itemError;
                        }
                        Console.WriteLine(errorMsg);
                    }
                    else
                    {
                        string errorMsg = "Sequence ";
                        double seqMeanError = 0.0;
                        foreach (double[] item in s)
                        {
                            pool.setInput(item);
                            if (item == s.First())
                            {
                                pool.Update(1.0, 0.0, false);
                            }
                            else
                            {
                                pool.Update(0.0, 1.0, false);

                            }
                            double itemError = pool.error(ref errorMsg);
                            seqMeanError += itemError;
                        }
                        Console.WriteLine(errorMsg);
                    }
                }
            }
        }
    }
}
