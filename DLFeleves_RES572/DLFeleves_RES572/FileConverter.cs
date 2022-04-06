using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLFeleves_RES572
{
    class FileConverter
    {
        /// <summary>
        /// Converts input .cvs file into .txt file.
        /// </summary>
        /// <param name="sourceFile">Input source file in .cvs format.</param>
        public static void ConvertCsvToTxt(string sourceFile, string dataOutput, string testOutput)
        {
            if(File.Exists(sourceFile))
            {
                StreamWriter[] outputWriter = new StreamWriter[2];
                outputWriter[0] = new StreamWriter(dataOutput);
                outputWriter[1] = new StreamWriter(testOutput);
                int index = 1;
                using (var rd = new StreamReader(sourceFile))
                {
                    string[] label = rd.ReadLine().Split(";").ToArray();
                    while (!rd.EndOfStream)
                    {
                        string[] line = rd.ReadLine().Split(";").ToArray();

                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < line.Length; i++)
                        {

                            if (i == line.Length - 1)
                            {
                                builder.Append(line[i]);
                            }
                            else
                            {
                                builder.Append(line[i]);
                                builder.Append('\t');
                            }
                        }

                        string result = builder.ToString();

                        if (index % 10 == 0)
                        {
                            outputWriter[1].WriteLine(result);
                        }
                        else
                        {
                            outputWriter[0].WriteLine(result);
                        }

                        index++;
                    }
                }
                outputWriter[0].Close();
                outputWriter[1].Close();
            }
            else
                Console.WriteLine("Could not find the source file");
        }

        /// <summary>
        /// Converts multi class regression data set into binary.
        /// </summary>
        /// <param name="sourceFile">Input data set in .txt format.</param>
        /// <param name="threshold">Output data set in .txt format.</param>
        public static void ConvertResultToBinary(string sourceFile, string outputFile, int threshold)
        {
            using (var sr = new StreamReader(sourceFile))
            {
                StreamWriter outputWriter = new StreamWriter(outputFile);
                StringBuilder builder = new StringBuilder();
                StringBuilder firstClone = new StringBuilder();
                StringBuilder secondClone = new StringBuilder();
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split("\t").ToArray();

                    float quality = float.Parse(line[line.Length - 1]);
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (i == line.Length - 1)
                        {
                            if(threshold <= quality)
                            {
                                builder.Append(threshold <= quality ? "1" : "0");
                                firstClone.Append(threshold <= quality ? "1" : "0");
                                secondClone.Append(threshold <= quality ? "1" : "0");
                            }
                            else
                            {
                                builder.Append(threshold <= quality ? "1" : "0");
                            }
                        }
                        else
                        {
                            if(threshold <= quality)
                            {
                                firstClone.Append(float.Parse(line[i]) + float.Parse(line[i]) * 0.01f);
                                firstClone.Append('\t');
                                secondClone.Append(float.Parse(line[i]) - float.Parse(line[i]) * 0.01f);
                                secondClone.Append('\t');
                            }
                            builder.Append(line[i]);
                            builder.Append('\t');
                        }
                    }

                    string result = builder.ToString();
                    outputWriter.WriteLine(result.ToString());
                    builder.Clear();

                    if (threshold <= quality)
                    {
                        string secondResult = firstClone.ToString();
                        string thirdResult = secondClone.ToString();
                        outputWriter.WriteLine(secondResult.ToString());
                        outputWriter.WriteLine(thirdResult.ToString());
                        firstClone.Clear();
                        secondClone.Clear();
                    }
                }
                sr.Close();
                outputWriter.Close();
            }
        }

        public static void countPos(string sourceFile, int threshold)
        {
            int goodBor = 0;
            using (var sr = new StreamReader(sourceFile))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split("\t").ToArray();

                    if (threshold == float.Parse(line[line.Length - 1]))
                        goodBor++;

                }
            }
            Console.WriteLine(goodBor);
        }
    }
}
