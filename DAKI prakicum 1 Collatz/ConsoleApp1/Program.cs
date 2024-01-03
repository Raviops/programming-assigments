using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {

        static void Main(string[] args)
        {
            List<int> clist = new List<int>();                      //lijst met getallen waarvan de collatz-lengte moet worden berekend
            List<int> collatz = new List<int>();                    //lijst met collatz-lengtes

            string invoer = Console.ReadLine();                     //haal string uit invoer
            string aantalgetallen = invoer.Split(' ')[0];           //split string op spaties
            int n = int.Parse(aantalgetallen);                      //haal het getal uit de string

            for (; n > 0; n--)                                      //n is het aantal getallen waarvan de lengtes berekend moeten worden, we lezen dus door totdat we alle getallen gehad hebben
            {
                string invoerc = Console.ReadLine();
                string getal = invoerc.Split(' ')[0];
                int c = int.Parse(getal);                           //loop door alle ingevoerde getallen en stop deze in een lijst
                clist.Add(c);
            }

            for (int i = 0; i < clist.Count; i++)
            { 
                int teller = 0;
                long newc = clist[i];

                while (newc > 1)
                {
                    if (newc % 2 == 0)
                    {
                        newc = newc / 2;                //berekening voor even getallen
                    }
                    else
                    {
                        newc = newc * 3 + 1;            //berekening voor oneven getallen
                    }
                    teller++;                           //elke berekening is een collatz-stap, dus de teller houdt de stappen bij
                }
                collatz.Add(teller);                    //alle getallen hebben een eigen collatz-lengte, we stoppen deze ook in een lijst zodat we het later makkelijk terug kunnen halen
                
            }
            foreach (int number in collatz)
            {
                Console.WriteLine(number);
            }
            Console.ReadLine();
        } 

    }
}
