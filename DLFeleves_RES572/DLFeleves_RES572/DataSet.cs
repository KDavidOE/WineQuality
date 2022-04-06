using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DLFeleves_RES572
{
    public class DataSet
    {
        public static int InputSize = 11;
        public List<float> Input { get; set; } = new List<float>();

        public static int OutputSize = 1;
        public List<float> Output { get; set; } = new List<float>();
        public int Count { get; set; }

        public DataSet(string filename, int inputSize = 0, int outputSize = 0)
        {
            InputSize = inputSize;
            OutputSize = outputSize;
            LoadData(filename);
        }

        void LoadData(string filename)
        {
            Count = 0;
            foreach (String line in File.ReadAllLines(filename))
            {
                var floats = line.Split('\t').Select(x => float.Parse(x)).ToList();

                if (InputSize == 0 || OutputSize == 0)
                {
                    InputSize = floats.Count - 1;
                    OutputSize = 1;
                }

                float output = floats[InputSize];

                if( OutputSize == 1)
                {
                    Output.Add(output);
                }
                else
                {
                    // Database only has ratings between [3-8]. The Output can be described by 6 numerical values. 
                    for (int i = 3; i < 9; i++)
                    {
                        if (output == i)
                        {
                            Output.Add(1);
                        }
                        else
                        {
                            Output.Add(0);
                        }
                    }
                }

                floats = Normalize(floats);
                Input.AddRange(floats.GetRange(0, InputSize));
                Count++;
            }
        }


        static float[] minValues;
        static float[] maxValues;

        public static List<float> Normalize(List<float> floats)
        {
            List<float> normalized = new List<float>();
            for (int i = 0; i < floats.Count; i++)
            {
                normalized.Add((floats[i] - minValues[i]) / (maxValues[i] - minValues[i]));
            }

            return normalized;
        }

        public static List<float> DeNormalizeResult(List<float> floats)
        {
            List<float> denormalized = new List<float>();
            for (int i = 0; i < floats.Count; i++)
                denormalized.Add(floats[i] * (maxValues[i + InputSize] - minValues[i + InputSize]) + minValues[i + InputSize]);
            return denormalized;
        }

        public void Shuffle()
        {
            Random rnd = new Random();
            for (int swapI = 0; swapI < Count; swapI++)
            {
                var a = rnd.Next(Count);
                var b = rnd.Next(Count);
                if (a != b)
                {
                    float T;
                    for (int i = 0; i < InputSize; i++)
                    {
                        T = Input[a * InputSize + i];
                        Input[a * InputSize + i] = Input[b * InputSize + i];
                        Input[b * InputSize + i] = T;
                    }
                    if (OutputSize == 1)
                    {
                        T = Output[a];
                        Output[a] = Output[b];
                        Output[b] = T;

                    }
                    else
                    {
                        // If there is multi classification, we have to rearrange the output sequence too
                        for (int i = 0; i < OutputSize; i++)
                        {
                            T = Output[a * OutputSize + i];
                            Input[a * OutputSize + i] = Input[b * OutputSize + i];
                            Input[b * OutputSize + i] = T;
                        }
                    }
                }
            }
        }

        public static void LoadMinMax(string filename)
        {
            foreach (String line in File.ReadAllLines(filename))
            {
                var floats = line.Split('\t').Select(x => float.Parse(x)).ToList();
                if (minValues == null)
                {
                    minValues = floats.ToArray();
                    maxValues = floats.ToArray();
                }
                else
                {
                    for (int i = 0; i < floats.Count; i++)
                        if (floats[i] < minValues[i])
                            minValues[i] = floats[i];
                        else
                            if (floats[i] > maxValues[i])
                            maxValues[i] = floats[i];
                }
            }
        }
    }
}
