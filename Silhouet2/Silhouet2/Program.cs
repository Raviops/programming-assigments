using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silhouet2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine().Split()[0]);
            Gebouw[] gebouwen = new Gebouw[n];
            for (int i = 0; i < n; i++)
            {
                string input = Console.ReadLine();
                Gebouw gebouw = new Gebouw(int.Parse(input.Split()[0]), int.Parse(input.Split()[1]), int.Parse(input.Split()[2]));
                gebouwen[i] = gebouw;
            }


            List<Strook> stroken = VindStroken(gebouwen);

            Console.WriteLine(stroken.Count - 1);
            for (int i = 1; i < stroken.Count; i++)
            {
                Console.WriteLine($"{stroken[i].X} {stroken[i].Height}");
            }
            Console.ReadLine();
        }

        static List<Strook> VindStroken(Gebouw[] gebouwen)
        {
            if (gebouwen.Length == 0) //zonder gebouwen geen stroken dus lege lijst
            {
                List<Strook> leeg = new List<Strook>();
                leeg.Add(new Strook(0, 0));
                return leeg;
            }

            return MergeSort(gebouwen, 0, gebouwen.Length - 1); //anders gaan we de stroken sorteren
        }
        static List<Strook> MergeSort(Gebouw[] gebouwen, int begin, int eind)
        {
            if (begin == eind) //Base Case: zodra begin niet meer kleiner is dan eind hebben we lengte 1 dus dan is de array gesorteerd
            {
                List<Strook> stroken = new List<Strook>();
                stroken.Add(new Strook(0, 0));
                stroken.Add(new Strook(gebouwen[begin].begin, gebouwen[begin].h)); //Een silhouet heeft altijd een begin en een eind
                stroken.Add(new Strook(gebouwen[begin].eind, 0));                  //Dus beide worden toegevoegd aan de lijst met stroken
                return stroken;
            }

            int q = (begin + eind) / 2;
            List<Strook> Links = MergeSort(gebouwen, begin, q);       //recursieve aanroep op linkerkant van araay
            List<Strook> Rechts = MergeSort(gebouwen, q + 1, eind);   //recursieve aanroep op rechterkant
            return Merge(Links, Rechts); //merge links en rechts
        }

        static List<Strook> Merge(List<Strook> Links, List<Strook> Rechts)
        {
            int nLinks = Links.Count;
            int nRechts = Rechts.Count;
            int HL = 0; //hoogte links
            int HR = 0; //hoogte rechts
            int i = 0;
            int j = 0;
            int counter = 1;
            List<Strook> stroken = new List<Strook>();
            stroken.Add(new Strook(0, 0));
            int hoogte = 0;


            while (i < nLinks && j < nRechts) //Zodra we door een van de twee lijsten zijn dan stoppen we en voegen we het restant van de stroken toe
            {
                if (Links[i].X < Rechts[j].X)
                //Als strook Links zich daadwerkelijk links van strook Rechts bevindt,
                //dan willen we het x-coordinaat van links en de hoogste hoogte tussen Links en Rechts toevoegen als strook             
                {
                    /*if (rechts[j].begin <= links[i].eind) //Als dit waar is dan is er overlap tussen A en B
                    {
                        Update(links[i], rechts[j]); //update begin en eind coordinaten
                    }*/
                    int x = Links[i].X;
                    HL = Links[i].Height;
                    //int hoogte = 0;
                    if (HL < HR) { hoogte = HR; }      //we willen per x de strook met de hoogste hoogte toevoegen
                    else { hoogte = HL; }
                    Strook Max = new Strook(x, hoogte);
                    if (stroken[counter - 1].Height != hoogte) 
                        //Om de gevraagde output te krijgen moeten we ervoor zorgen dat we de 'tussenstroken' eruit filteren
                        //deze conditie zorgt ervoor dat we pas nieuwe stroken toevoegen als de hoogte verandert door de nieuwe hoogte te vergelijken met de oude
                    {
                        stroken.Add(Max);
                        counter++;
                    }
                    i++;
                }

                else if(Links[i].X > Rechts[j].X)
                {
                    /*if (links[i].begin <= rechts[j].eind && links[i].eind >= rechts[j].eind) //Als dit waar is dan is er overlap tussen A en B
                    {
                        Update(rechts[j], links[i]); //update begin en eind coordinaten
                    }*/
                    int x = Rechts[j].X;
                    HR = Rechts[j].Height;
                    //int hoogte = 0;
                    if (HL < HR) { hoogte = HR; }
                    else { hoogte = HL; }
                    Strook Max = new Strook(x, hoogte);
                    if (stroken[counter - 1].Height != hoogte)
                    {
                        stroken.Add(Max);
                        counter++;
                    }
                    j++;
                }
                else
                {//Als Links en Rechts op dezelfde x-coordinaat zitten, dan moet voor deze x de hoogste hoogte bepaald worden
                    //Dit doen we door de hoogte van Links te vergelijken met de hoogte van Rechts.
                    int x = Links[i].X;
                    HL = Links[i].Height;
                    HR = Rechts[j].Height;
                    //int hoogte = 0;
                    if (HL < HR) { hoogte = HR; }
                    else { hoogte = HL;}
                    Strook Max = new Strook(x, hoogte);
                    if (stroken[counter - 1].Height != hoogte)
                    {
                        stroken.Add(Max);
                        counter++;
                    }
                    i++; //We hebben voor de betreffende x-coordinaat bepaald welke hoogte ddarbij hoort dus bij zowel Links als Rechts opschuiven naar de volgende strook
                    j++;
                }
            }

            while (i < nLinks)
            {
                stroken.Add(Links[i]);
                i++;
            }

            while (j < nRechts)
            {
                stroken.Add(Rechts[j]);
                j++;
            }

            return stroken;
        }

    }

    public class Gebouw
    {
        public int begin;
        public int h;
        public int eind;

        public Gebouw(int x_begin, int hoogte, int x_eind)
        {
            begin = x_begin;
            h = hoogte;
            eind = x_eind;
        }
    }

    public class Strook
    {
        public int X;
        public int Height;

        public Strook(int begin, int hoogte)
        {
            X = begin;
            Height = hoogte;
        }
    }
}
