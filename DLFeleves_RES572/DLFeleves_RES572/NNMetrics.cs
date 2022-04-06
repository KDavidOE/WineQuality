using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLFeleves_RES572
{
    class NNMetrics
    {
        public NNMetrics(double lossValue, double accValue, double precision, double sensitivity, double specifity, double f1)
        {
            LossValue = lossValue;
            AccValue = accValue;
            Precision = precision;
            Sensitivity = sensitivity;
            Specifity = specifity;
            F1 = f1;
        }

        public double LossValue { get; }
        public double AccValue { get; }
        public double Precision { get; }
        public double Sensitivity { get; }
        public double Specifity { get; }
        public double F1 { get; }

        public override string ToString()
        {
            string text = String.Format("loss: {0}\taccuracy: {1}\tprecision: {2}\tsensitivity: {3}\tspecifity: {4}\tF1: {5}", LossValue, AccValue, Precision, Sensitivity, Specifity, F1);
            return text;
        }
    }
}
