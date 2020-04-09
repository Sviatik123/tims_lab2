using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MathNet.Numerics;
using System.Windows.Input;

namespace TimsLab2WPF
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private double alfa;
        public double Alfa
        {
            get { return alfa; }
            set
            {         
                alfa = value;
                OnPropertyChanged("Alfa");
            }
        }

        private double? a;
        public double? A
        {
            get { return a; }
            set
            {
                a = Math.Round((double)value, 4);
                OnPropertyChanged("A");
            }
        }

        private double? sigma;
        public double? Sigma
        {
            get { return sigma; }
            set
            {
                sigma = Math.Round((double)value, 4);
                OnPropertyChanged("Sigma");
            }
        }

        private double df;
        public double Df
        {
            get { return df; }
            set
            {
                df = value;
                OnPropertyChanged("Df");
            }
        }

        private double xe;
        public double Xe
        {
            get { return xe; }
            set
            {
                xe = Math.Round(value, 4);
                OnPropertyChanged("Xe");
            }
        }

        private double xcr;
        public double Xcr
        {
            get { return xcr; }
            set
            {
                xcr = Math.Round(value, 4);
                OnPropertyChanged("Xcr");
            }
        }

        private int colAmount;
        public int ColAmount
        {
            get { return colAmount; }
            set
            {
                colAmount = value;
                OnPropertyChanged("ColAmount");
                //Df = colAmount - 1;
            }
        }

        private string result;
        public string Result
        {
            get { return result; }
            set
            {
                result = value;
                OnPropertyChanged("Result");
            }
        }

        int n;

        List<Tuple<double, double>> intervals;

        List<int> freaquencies;

        List<double> npi;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public ICommand CalculateCommand { get; set; }

        public MainWindowViewModel()
        {
            CalculateCommand = new DelegateCommand(Calculate);
        }

        void ParceData()
        {
            using (StreamReader sr = new StreamReader("../../data.txt"))
            {
                string[] data = sr.ReadToEnd().Split('\n');
                intervals = new List<Tuple<double, double>>();
                string[] strIntervals = data[0].Replace("\r", "").Split();
                colAmount = strIntervals.Length;
                foreach(var interval in strIntervals)
                {
                    string[] limits = interval.Split(',');
                    intervals.Add(new Tuple<double, double>(double.Parse(limits[0]), double.Parse(limits[1])));
                }
                freaquencies = new List<int>();
                string[] strFreaquency = data[1].Replace("\r", "").Split();
                foreach (var el in strFreaquency)
                {
                    freaquencies.Add(int.Parse(el));
                }
                n = freaquencies.Sum();
            }
        }
        void CalculateA()
        {
            var intervalsMiddle = intervals.Select(x => (x.Item1 + x.Item2) / 2).ToList();
            A = 0;
            for (int i = 0; i < intervalsMiddle.Count; i++)
            {
                A += intervalsMiddle[i] * freaquencies[i];
            }
            A /= n;
            Df--;
        }
        void CalculateSigma()
        {
            var intervalsMiddle = intervals.Select(x => (x.Item1 + x.Item2) / 2).ToList();
            Sigma = 0;
            for (int i = 0; i < intervalsMiddle.Count; i++)
            {
                Sigma += Math.Pow((double)(intervalsMiddle[i] - A), 2) * freaquencies[i];
            }
            Sigma = Math.Sqrt((double)Sigma / n);
            Df--;
        }
        void CalculateNpi()
        {
            npi = new List<double>();
            var newFrequencies = new List<int>();
            double npiToAdd = 0;
            int freqToAdd = 0;
            for (int i = 0; i < intervals.Count; i++)
            {
                npiToAdd += ((MathNet.Numerics.Distributions.Normal.CDF((double)A, (double)Sigma, intervals[i].Item2) - MathNet.Numerics.Distributions.Normal.CDF((double)A, (double)Sigma, intervals[i].Item1)) * n);
                freqToAdd += freaquencies[i];
                if (npiToAdd >= 10)
                {
                    npi.Add(npiToAdd);
                    newFrequencies.Add(freqToAdd);
                    npiToAdd = 0;
                    freqToAdd = 0;
                }
            }
            if(npiToAdd != 0)
            {
                npi[npi.Count - 1] += npiToAdd;
                newFrequencies[npi.Count - 1] += freqToAdd;
            }
            freaquencies = newFrequencies;
            Df += npi.Count - 1;
        }
        void CalculateXcr()
        {
            Xcr = MathNet.Numerics.Distributions.ChiSquared.InvCDF(Df, 1 - Alfa);
        }
        void CalculateXe()
        {
            Xe = 0;
            for (int i = 0; i < npi.Count; i++)
            {
                Xe += Math.Pow((freaquencies[i] - npi[i]), 2) / npi[i];
            }
        }
        void PrintRes()
        {
            if(Xe <= Xcr)
            {
                Result = "Accept the hypotesis";
            }
            else
            {
                Result = "Reject the hypotesis";
            }
        }
        
        public void Calculate(object o)
        {
            ParceData();
            if (A == null)
            {
                CalculateA();
            }
            if(Sigma == null)
            {
                CalculateSigma();
            }
            CalculateNpi();
            CalculateXcr();
            CalculateXe();
            PrintRes();
        }
    }
}
