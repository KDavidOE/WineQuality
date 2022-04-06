using System;
using System.IO;

namespace DLFeleves_RES572
{
    internal class Program
    {

        static DataSet trainDS;
        static DataSet testDS;

        static void Main(string[] args)
        {
            //FileConverter.ConvertCsvToTxt("winequality-white.csv", "data_multi.txt", "test_multi.txt");
            //FileConverter.ConvertResultToBinary("data_multi.txt", "data_binary.txt", 7);
            //FileConverter.ConvertResultToBinary("test_multi.txt", "test_binary.txt", 7);

            RunBinary();
            //RunMulti();
        }

        public static void RunBinary()
        {
            DataSet.LoadMinMax(@"Data\data_binary.txt");
            testDS = new DataSet(@"Data\test_binary.txt", 11, 1);
            trainDS = new DataSet(@"Data\data_binary.txt", 11, 1);
            RunNetwork(@"Data\SavedNetworkBinary.txt", "binary", 10, 0, 0);
        }

        public static void RunMulti()
        {
            DataSet.LoadMinMax(@"Data\data_multi.txt");
            testDS = new DataSet(@"Data\test_multi.txt", 11, 6);
            trainDS = new DataSet(@"Data\data_multi.txt", 11, 6);
            RunNetwork(@"Data\SavedNetworkMulti.txt", "multi", 10, 0, 0);
        }

        public static void RunNetwork(string file, string outputFileName, int hiddenLayerSize, double l1weight, double l2weight)
        {
            NeuralNetwork app;
            if (File.Exists(file))
            {
                app = new NeuralNetwork(file);
            }
            else
            {
                app = new NeuralNetwork(hiddenLayerSize);
                app.Train(trainDS, l1weight, l2weight);
                app.Save(file);
            }

            NNMetrics trainMetrics = app.Evaluate(trainDS);
            NNMetrics testMetrics = app.Evaluate(testDS);
            Console.WriteLine(trainMetrics.ToString());
            Console.WriteLine(testMetrics.ToString());

            app.PredictionOnDataSet("Result_" + outputFileName + ".txt", testDS);
        }
    }
}
