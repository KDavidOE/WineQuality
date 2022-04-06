using CNTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLFeleves_RES572
{
    class NeuralNetwork
    {

        const int batchSize = 32;
        const int epochCount = 1000;

        readonly Variable x;
        readonly Function y;

        public NeuralNetwork(int hiddenLayerSize)
        {
            int[] layers = new int[] { DataSet.InputSize, hiddenLayerSize, hiddenLayerSize, hiddenLayerSize, hiddenLayerSize, DataSet.OutputSize };

            x = Variable.InputVariable(new int[] { layers[0] }, DataType.Float, "x");

            Function lastLayer = x;
            for (int i = 0; i < layers.Length - 1; i++)
            {
                Parameter weight = new Parameter(new int[] { layers[i + 1], layers[i] }, DataType.Float, CNTKLib.GlorotNormalInitializer());
                Parameter bias = new Parameter(new int[] { layers[i + 1] }, DataType.Float, CNTKLib.GlorotNormalInitializer());
                Function times = CNTKLib.Times(weight, lastLayer);
                Function plus = CNTKLib.Plus(times, bias);

                if (DataSet.OutputSize != 1)
                {
                    if (i != layers.Length - 2)
                        lastLayer = CNTKLib.Sigmoid(plus);
                    else
                        lastLayer = CNTKLib.Softmax(plus);
                }
                else
                {
                    lastLayer = CNTKLib.Sigmoid(plus);
                }
            }

            y = lastLayer;
        }

        public NeuralNetwork(string fileName)
        {
            y = Function.Load(fileName, DeviceDescriptor.CPUDevice);
            x = y.Arguments.First(x => x.Name == "x");
        }

        public void Train(DataSet ds, double l1weight, double l2weight)
        {
            PairSizeTDouble p1 = new PairSizeTDouble(10, 3);
            PairSizeTDouble p2 = new PairSizeTDouble(20, 2);
            PairSizeTDouble p3 = new PairSizeTDouble(1, 1);
            var vp = new VectorPairSizeTDouble() { p1, p2, p3 };

            // 2 types of loss functino is needed because of the difference of binary (The difference of 2 values) and multi class regression (The value of classification error). 
            Variable yt = Variable.InputVariable(new int[] { DataSet.OutputSize }, DataType.Float);
            Function loss = CNTKLib.SquaredError(y, yt);
            Function err = CNTKLib.ClassificationError(y, yt);

            Function y_rounded = CNTKLib.Round(y);
            Function y_yt_equal = CNTKLib.Equal(y_rounded, yt);

            AdditionalLearningOptions alo = new AdditionalLearningOptions();
            alo.l1RegularizationWeight = l1weight;
            alo.l2RegularizationWeight = l2weight;

            Learner learner = CNTKLib.SGDLearner(new ParameterVector(y.Parameters().ToArray()), new TrainingParameterScheduleDouble(vp,batchSize), alo) ;
            Trainer trainer = Trainer.CreateTrainer(y, loss, (DataSet.OutputSize == 1 ? y_yt_equal : err), new List<Learner>() { learner });

            for (int epochI = 0; epochI <= epochCount; epochI++)
            {
                double sumLoss = 0;
                double sumEval = 0;
                ds.Shuffle();
                for (int batchI = 0; batchI < ds.Count / batchSize; batchI++)
                {
                    Value x_value = Value.CreateBatch(x.Shape, ds.Input.GetRange(batchI * batchSize * DataSet.InputSize, batchSize * DataSet.InputSize), DeviceDescriptor.CPUDevice);
                    Value yt_value = Value.CreateBatch(yt.Shape, ds.Output.GetRange(batchI * batchSize * DataSet.OutputSize, batchSize * DataSet.OutputSize), DeviceDescriptor.CPUDevice);
                    var inputDataMap = new Dictionary<Variable, Value>()
                    {
                        { x, x_value },
                        { yt, yt_value }
                    };

                    trainer.TrainMinibatch(inputDataMap, false, DeviceDescriptor.CPUDevice);
                    sumLoss += trainer.PreviousMinibatchLossAverage() * trainer.PreviousMinibatchSampleCount();
                    sumEval += trainer.PreviousMinibatchEvaluationAverage() * trainer.PreviousMinibatchSampleCount();
                }
                Console.WriteLine(String.Format("{0}\tloss:{1}\teval:{2}", epochI, sumLoss / ds.Count,(DataSet.OutputSize == 1 ? sumEval / ds.Count : 1- sumEval / ds.Count)));
            }
        }

        public NNMetrics Evaluate(DataSet ds )
        {
            Variable yt = Variable.InputVariable(new int[] { DataSet.OutputSize }, DataType.Float);
            Function loss = CNTKLib.SquaredError(y, yt);
            Function err = CNTKLib.ClassificationError(y, yt);

            Evaluator evaluator_loss = CNTKLib.CreateEvaluator(loss);
            Evaluator evaluator_err = CNTKLib.CreateEvaluator(err);

            Function y_rounded = CNTKLib.Round(y);
            Function y_yt_equal = CNTKLib.Equal(y_rounded, yt);

            Evaluator evaluator_accuracy = CNTKLib.CreateEvaluator(DataSet.OutputSize > 1 ? y_yt_equal : err);

            // this part may not be optimal. Declaring every option in truth table. 
            Constant pos = Constant.Scalar(DataType.Float, 1.0f);
            Constant neg = Constant.Scalar(DataType.Float, 0.0f);
            Function y_positive = CNTKLib.Equal(y_rounded, pos);
            Function y_positive_true = CNTKLib.ElementAnd(y_positive, CNTKLib.Equal(yt, pos));
            Function y_false_positive = CNTKLib.ElementAnd(y_positive, CNTKLib.Equal(yt,neg));
            
            Function y_negative = CNTKLib.Equal(y_rounded, neg);
            Function y_true_negative = CNTKLib.ElementAnd(y_negative, CNTKLib.Equal(yt, neg));
            Function y_false_negative = CNTKLib.ElementAnd(y_negative, CNTKLib.Equal(yt, pos));

            // Thiis part creates the evaulators for getting the metrics base on the truth table.
            Evaluator TruePositives = CNTKLib.CreateEvaluator(y_positive_true);
            Evaluator allPositives = CNTKLib.CreateEvaluator(y_positive);
            Evaluator FalsePositives = CNTKLib.CreateEvaluator(y_false_positive);
            Evaluator TrueNegatives = CNTKLib.CreateEvaluator(y_true_negative);
            Evaluator AllNegatives = CNTKLib.CreateEvaluator(y_negative);
            Evaluator FalseNegatives = CNTKLib.CreateEvaluator(y_false_negative);

            double sumLoss = 0;
            double sumError = 0;
            double sumAccuracy = 0;
            double sumAllPositives = 0;
            double sumTruePositives = 0;
            double sumAllNegatives = 0;
            double sumTrueNegatives = 0;
            double sumFalsePositives = 0;
            double sumFalseNegatives = 0;
            for (int batchI = 0; batchI < ds.Count / batchSize; batchI++)
            {
                Value x_value = Value.CreateBatch(x.Shape, ds.Input.GetRange(batchI * batchSize * DataSet.InputSize, batchSize * DataSet.InputSize), DeviceDescriptor.CPUDevice);
                Value yt_value = Value.CreateBatch(yt.Shape, ds.Output.GetRange(batchI * batchSize * DataSet.OutputSize, batchSize * DataSet.OutputSize), DeviceDescriptor.CPUDevice);
                var inputDataMap = new UnorderedMapVariableValuePtr()
                    {
                        { x, x_value },
                        { yt, yt_value }
                    };

                sumLoss += evaluator_loss.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
                sumError += evaluator_err.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
                sumAccuracy += evaluator_accuracy.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;

                sumAllPositives += allPositives.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
                sumTruePositives += TruePositives.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
                sumFalsePositives += FalsePositives.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
                sumAllNegatives += AllNegatives.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
                sumTrueNegatives += TrueNegatives.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
                sumFalseNegatives += FalseNegatives.TestMinibatch(inputDataMap, DeviceDescriptor.CPUDevice) * batchSize;
            }
            
            var lossValue = sumLoss / ds.Count;
            double accValue;

            if (DataSet.OutputSize == 1)
            {
                accValue = sumAccuracy / ds.Count; // --  general accuracy
            }
            else
            {
                accValue = 1 - sumError / ds.Count;
            }

            var precision = sumTruePositives / (sumTruePositives + sumFalsePositives);
            var sensitivity = sumTruePositives / (sumTruePositives + sumFalseNegatives);
            var specifity = sumTrueNegatives / (sumTrueNegatives + sumFalsePositives);
            var f1 = 2 * ((precision * sensitivity) / (precision + sensitivity));
            NNMetrics metrics = new NNMetrics(lossValue, accValue, precision, sensitivity, specifity, f1);
            return metrics;
        }

        public float Prediction(float[] values)
        {
            Value x_value = Value.CreateBatch(x.Shape, values, DeviceDescriptor.CPUDevice);
            var inputDataMap = new Dictionary<Variable, Value>() { { x, x_value } };
            var outputDataMap = new Dictionary<Variable, Value>() { { y, null } };
            y.Evaluate(inputDataMap, outputDataMap, DeviceDescriptor.CPUDevice);
            var result = DataSet.DeNormalizeResult(new List<float>() { outputDataMap[y].GetDenseData<float>(y)[0].Max()})[0];
            return result;
        }

        public void PredictionOnDataSet(String filename, DataSet predDS)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
            {

                float[] fs = new float[11];
                int v = predDS.Input.Count / 11;
                for (int i = 0; i < v; i++)
                {
                    float pred = Prediction(predDS.Input.GetRange(0 + i * 11, 11).ToArray());
                    float realResult = predDS.Output[i];
                    Console.WriteLine("Real result: {0}\tPredicted Result{1,15}", realResult, pred);

                    StringBuilder sb = new StringBuilder();
                    file.WriteLine("Real result: {0}\tPredicted Result{1,15:}", realResult, Math.Round(pred));
                    sb.Clear();
                }
            }
        }

        public void Save(string filename)
        {
            y.Save(filename);
        }
    }
}
