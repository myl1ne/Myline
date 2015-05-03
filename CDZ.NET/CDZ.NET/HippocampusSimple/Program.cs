using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HippocampusSimple
{
    class Program
    {
        class Sequence : List<double[]>
        {
            public string ToReadable()
            {
                string str = "";
                for(int i=0; i<Count;i++)
                {
                    str += "Elt " + i + "-->\t";
                    for (int j = 0; j < this[i].Length; j++)
                    {
                        str += this[i][j] + "\t";
                    }
                    str += "\n";
                }
                return str;
            }
        }

        static void Main(string[] args)
        {
            double[] a = { 1, 0, 0, 0, 0 };
            double[] b = { 0, 1, 0, 0, 0 };
            double[] c = { 0, 0, 1, 0, 0 };
            double[] d = { 0, 0, 0, 1, 0 };
            double[] e = { 0, 0, 0, 0, 1 };

            Sequence abcde = new Sequence() { a, b, c, d, e };
            Sequence ebcda = new Sequence() { e, b, c, d, a };
            Sequence abddc = new Sequence() { a, b, d, d, c };
            Sequence ab = new Sequence() { a, b };
            Sequence cd = new Sequence() { c, d };

            List<Sequence> sequencesSet = new List<Sequence>() { ab, cd, abcde, ebcda, abddc };
            
            //Note: Erreur a la troisieme lettre. Le reseau ne sait pas ce qui vient apres "ab", c'est normal.
            //Il faut detecter ce genre d'erreur pour segmenter les sous sequences.
            //Par contre, le reseau corrige parfaitement une fois desambiguise (la 5eme lettre est correcte)


            TimeCanvas canvas = new TimeCanvas(5,7);
            for (int i = 0; i < 500000; i++)
            {
                foreach (Sequence s in sequencesSet)
                {
                    bool training = true;
                    string errorMsg = "\n\n Sequence \n";
                    double seqMeanError = 0.0;

                    canvas.Reset();
                    foreach (double[] item in s)
                    {
                        if (item != s.First())
                        {
                            double[] prediction = canvas.Predict();
                            double[] reality = item;
                            double itemError = CDZNET.MathHelpers.distance(prediction, reality);
                            seqMeanError += itemError;
                            errorMsg += "Error = " + itemError + "\n";
                            errorMsg += "Reality " + Convert(reality) + "\n ";
                            errorMsg += "Predict " + Convert(prediction) + "\n \n ";
                        }
                        if (training)
                            canvas.Train(item);
                    }
                    Console.WriteLine(errorMsg);
                }
            }
        }

        static string Convert(double[] d)
        {
            string str = "";
            for (int i = 0; i < d.Length; i++)
            {
                str += d[i] + "\t";   
            }
            return str;
        }
    }
}
