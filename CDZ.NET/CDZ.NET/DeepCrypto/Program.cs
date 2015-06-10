using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections;

using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Networks;
using AForge.Neuro;
using AForge.Neuro.Learning;
using Accord.Neuro.Learning;

using Accord.Math;

namespace DeepCrypto
{
    class Program
    {
        class Triplet
        {
            public double[] original;
            public double[] encrypted;
            public double[] key;

            public Triplet(byte[] boriginal, byte[] bencrypted, byte[] bkey)
            {
                original = new double[boriginal.Count() * 8];
                encrypted = new double[bencrypted.Count() * 8];
                key = new double[bkey.Count() * 8];

                BitArray baOriginal = new BitArray(boriginal);
                BitArray baEncrypted = new BitArray(bencrypted);
                BitArray baKey = new BitArray(bkey);

                for (int byteIndex = 0; byteIndex < boriginal.Length; byteIndex++)
                    for(int bitIndex = 0; bitIndex<8; bitIndex++)
                        original[byteIndex*8 + bitIndex] = baOriginal[byteIndex*8+bitIndex] ? 1.0 : 0.0;

                for (int byteIndex = 0; byteIndex < bencrypted.Length; byteIndex++)
                    for(int bitIndex = 0; bitIndex<8; bitIndex++)
                        encrypted[byteIndex*8 + bitIndex] = baEncrypted[byteIndex*8+bitIndex] ? 1.0 : 0.0;

                for (int byteIndex = 0; byteIndex < bkey.Length; byteIndex++)
                    for(int bitIndex = 0; bitIndex<8; bitIndex++)
                        key[byteIndex*8 + bitIndex] = baKey[byteIndex*8+bitIndex] ? 1.0 : 0.0;
            }

            public Triplet(double[] input, double[] output, int msgSize)
            {
                original = new double[msgSize];
                encrypted = new double[input.Count() - msgSize];
                key = new double[output.Count()];
                Array.Copy(input, original, msgSize);
                Array.Copy(input, msgSize - 1, encrypted, 0, input.Count() - msgSize);
                Array.Copy(output, key, output.Count());
            }

            public void ToIO(out double[] input, out double[] output)
            {
                input = original.Concat(encrypted).ToArray();
                output = key;
            }

            public void ToBytes(out byte[] poriginal, out byte[] pencrypted, out byte[] pkey)
            {
                BitArray baOriginal = new BitArray(original.Count());
                for(int bitIndex = 0; bitIndex<original.Count(); bitIndex++)
                    baOriginal[bitIndex] = original[bitIndex]>0.5;
                poriginal = new byte[baOriginal.Count / 8];
                baOriginal.CopyTo(poriginal, 0);
                
                BitArray baEncrypted = new BitArray(encrypted.Count());
                for(int bitIndex = 0; bitIndex<encrypted.Count(); bitIndex++)
                    baEncrypted[bitIndex] = encrypted[bitIndex]>0.5;
                pencrypted = new byte[baEncrypted.Count / 8];
                baEncrypted.CopyTo(pencrypted, 0);
                
                BitArray baKey = new BitArray(key.Count());
                for(int bitIndex = 0; bitIndex<key.Count(); bitIndex++)
                    baKey[bitIndex] = key[bitIndex]>0.5;
                pkey = new byte[baKey.Count / 8];
                baKey.CopyTo(pkey, 0);    
            }

            public static void Transform2IO(List<Triplet> triplets, out double[][] input, out double[][] output)
            {
                input = new double[triplets.Count][];
                output = new double[triplets.Count][];
                for (int t = 0; t < triplets.Count; t++)
                    triplets[t].ToIO(out input[t], out output[t]);
            }

            public static void Transform2Triplets(double[][] input, double[][] output, int msgSize, out List<Triplet> triplets)
            {
                triplets = new List<Triplet>(input.Count());
                for (int t = 0; t < triplets.Count; t++)
                    triplets[t] = new Triplet(input[t], output[t],msgSize);
            }

            public string GetBytesForm()
            {
                byte[] bOriginal, bEncrpted, bKey;
                ToBytes(out bOriginal, out bEncrpted, out bKey);
                string msg = "Original = " + System.Text.Encoding.UTF8.GetString(bOriginal) + '\n';
                msg += "Encrypted = " + System.Text.Encoding.UTF8.GetString(bEncrpted) + '\n';
                msg += "Key = " + System.Text.Encoding.UTF8.GetString(bKey) + '\n';
                return msg;
            }
            //public int BytesDifference(Triplet t)
            //{

            //}
        }

        static void Main(string[] args)
        {
            //Generate the training data
            int keySize = 64;
            int messageSize = 64;
            int trainingSetSize = 100;
            List<Triplet> trainingSet = GenerateDESDataset(trainingSetSize, keySize, messageSize);
            double[][] inputTraining, outputTraining;
            Triplet.Transform2IO(trainingSet, out inputTraining, out outputTraining);

            //Generate the test data
            List<Triplet> testSet = GenerateDESDataset(trainingSetSize, keySize, messageSize);
            double[][] inputTest, outputTest;
            Triplet.Transform2IO(testSet, out inputTest, out outputTest);

            //Find the right sizes, not sure why I have to do that :-/
            int inputSize = trainingSet.First().original.Count() + trainingSet.First().encrypted.Count();
            int outputSize = trainingSet.First().key.Count();

            //Create a network
            var function = new SigmoidFunction(2.0);
            //ActivationNetwork network = new ActivationNetwork(function, inputSize, 25, outputSize);
            //ParallelResilientBackpropagationLearning teacher = new ParallelResilientBackpropagationLearning(network);

            DeepBeliefNetwork network = new DeepBeliefNetwork(inputSize, 10, outputSize);
            Accord.Neuro.Learning.DeepNeuralNetworkLearning teacher = new DeepNeuralNetworkLearning(network);

            //Train the network
            int epoch = 0;
            double stopError = 0.1;
            int resets = 0;
            double minimumErrorReached = double.PositiveInfinity;
            while (minimumErrorReached > stopError && resets < 1)
            {
                network.Randomize();
                //teacher.Reset(0.0125);

                double errorTrain = double.PositiveInfinity;
                for (epoch = 0; epoch < 500000 && errorTrain > stopError; epoch++)
                {
                    errorTrain = teacher.RunEpoch(inputTraining, outputTraining) / (double)trainingSetSize;
                    //Console.WriteLine("Epoch " + epoch + " = \t" + error);
                    if (errorTrain < minimumErrorReached)
                    {
                        minimumErrorReached = errorTrain;
                        network.Save("cryptoDESNetwork.mlp");
                    }
                    Console.Clear();
                    Console.WriteLine("Epoch : " + epoch);
                    Console.WriteLine("Train Set  Error : " + errorTrain.ToString("N2"));
                    double errorTest = teacher.ComputeError(inputTest, outputTest) / (double)inputTest.Count();
                    Console.WriteLine("Test Set  Error : " + errorTest.ToString("N2"));      
                }
                //Console.Write("Reset (" + error+")->");
                resets++;
            }
            Console.WriteLine();

            //Compute the reall error
            foreach(Triplet tReal in testSet)
            {
                double[] rIn, rOut, pOut;
                byte[] brMsg, brEncrypted, brKey;
                tReal.ToBytes(out brMsg, out brEncrypted, out brKey);

                tReal.ToIO(out rIn, out rOut); 
                pOut = network.Compute(rIn);

                Triplet tPredicted = new Triplet(rIn, pOut, messageSize);
                byte[] bpMsg, bpEncrypted, bpKey;
                tPredicted.ToBytes(out bpMsg, out bpEncrypted, out bpKey);

                int wrongBytes = 0;
                for (int i = 0; i < keySize / 8; i++ )
                {
                    if (brKey[i] != bpKey[i])
                    {
                        wrongBytes++;
                    }
                }
                Console.WriteLine("Wrong bytes = " + wrongBytes);
                //Console.WriteLine("REAL = \n" + tReal.GetBytesForm());
                //Console.WriteLine("Predicted = \n" + tPredicted.GetBytesForm());
            }

            Console.ReadKey();
        }

        static List<Triplet> GenerateDESDataset(int samples, int keySize = 64, int msgSize = 64)
        {
            Random rand = new Random();
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.KeySize = keySize;

            List<Triplet> triplets = new List<Triplet>(samples);
            for (int i = 0; i < samples; i++)
			{
                des.GenerateKey();
                byte[] key = des.Key; // save this!
                byte[] original = new byte[msgSize];
                rand.NextBytes(original);
                ICryptoTransform encryptor = des.CreateEncryptor();
                byte[] encrypted = encryptor.TransformFinalBlock(original, 0, msgSize);

                //ICryptoTransform decryptor = des.CreateDecryptor();
                //byte[] originalAgain = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

                Triplet triplet = new Triplet(original, encrypted, key);
                triplets.Add(triplet);


                //string msg = "Original = " + System.Text.Encoding.UTF8.GetString(original) + '\n';
                //msg += "Encrypted = " + System.Text.Encoding.UTF8.GetString(encrypted) + '\n';
                //msg += "Key = " + System.Text.Encoding.UTF8.GetString(key) + '\n';
                //Console.WriteLine(msg);
                //Console.WriteLine(triplet.GetBytesForm());
            }
            return triplets;
        }




    }
}
