using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breuken_DAKI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string regel1 = Console.ReadLine();
            int n = int.Parse(regel1.Split()[0]);
            int k = int.Parse(regel1.Split()[1]);

            Breuk[] breuken = new Breuk[n];
            for (int i = 0; i < n; i++)
            {
                string input = Console.ReadLine();
                int teller = int.Parse(input.Split()[0]);
                int noemer = int.Parse(input.Split()[1]);
                Breuk breuk = new Breuk(teller, noemer);
                breuken[i] = breuk;                
            }


            Console.WriteLine(n);
            if (n == 1)
            {
                Console.WriteLine(breuken[0].breuk_teller + " " + breuken[0].breuk_noemer);
            }
            else
            {
                //Selection_Sort(breuken, 0, n - 1);
                QuickSort(breuken, 0, n - 1, k);
                for (int t = 0; t < n; t++)
                {
                    Console.WriteLine(breuken[t].breuk_teller + " " + breuken[t].breuk_noemer);
                }
            }
           
            Console.ReadLine();
        }
        static void Selection_Sort(Breuk[] breuken, int begin, int eind)
        {
            for (int j = begin; j < eind; j++)
            {
                Breuk key = breuken[j];
                int kleinste = j;
                for (int i = j + 1; i <= eind; i++)
                {
                    if(breuken[i].CompareTo(breuken[kleinste]) == -1)  //breuken[i] < breuken[kleinste]
                    {
                        kleinste = i;
                    }
                }
                breuken[j] = breuken[kleinste];
                breuken[kleinste] = key;
            }
        }

        static void QuickSort(Breuk[] breuken, int begin, int eind, int k)
        {
            if (begin < eind)
            {
                int q = Partition(breuken, begin, eind);
                if (((q - 1) - begin) + 1 > k)
                {
                    QuickSort(breuken, begin, q - 1, k);
                }

                else
                {
                    Selection_Sort(breuken, begin, q - 1);
                }

                if(eind - (q + 1) + 1 > k)
                {
                    QuickSort(breuken, q + 1, eind, k);
                }

                else
                {
                    Selection_Sort(breuken, q + 1, eind);
                }
            }
        }

        static int Partition(Breuk[] breuken, int begin, int eind)
        {
            Breuk x = breuken[eind];
            int i = begin - 1;
            Breuk opslag = breuken[eind];
            for (int j = begin; j < eind; j++)
            {
                if (breuken[j].CompareTo(x) <= 0)  //breuken[j] <= x
                {
                    i += 1;
                    //wissel A[i] en A[j]
                    opslag = breuken[i];
                    breuken[i] = breuken[j];
                    breuken[j] = opslag;
                }
            }
            //wissel A[i+1] en A[r]
            opslag = breuken[i + 1];
            breuken[i + 1] = breuken[eind];
            breuken[eind] = opslag;
            return i + 1;
        }

    }
    class Breuk : IComparable<Breuk>
    {
       public int breuk_teller;
       public int breuk_noemer;
       public Breuk(int teller, int noemer)
       {
            breuk_teller = teller;
            breuk_noemer = noemer;
       }

        public int CompareTo(Breuk y)
        {
            int verschil = (this.breuk_teller*y.breuk_noemer)-(y.breuk_teller*this.breuk_noemer);
            if (verschil < 0)
            {
                return -1;
            }

            if (verschil > 0)
            {
                return 1;
            }

            else
            {
                if(y.breuk_noemer > this.breuk_noemer)
                {
                    return -1;
                }

                else if (y.breuk_noemer < this.breuk_noemer)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        

        
    }
}
