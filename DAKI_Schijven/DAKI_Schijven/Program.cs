using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAKI_Schijven
{
    public class Program
    {
        static void Main(string[] args)
        {
            string input = Console.ReadLine().Split()[0];
            int n = int.Parse(input);                       // n = omvang catalogus

            /*Console.WriteLine(n);
            Console.ReadLine();*/

            int[] catalogus = new int[n];         // In deze lijst komen de standaardblokken, dus dit is de catalogus
            for(int t = 0; t < n; t++)                     // Deze for-loop leest de grootte van de standaardblokken in en 'maakt' de catalogus
            {
                string catalog_input = Console.ReadLine().Split()[0];
                int catalog_n = int.Parse(catalog_input);
                catalogus[t] = catalog_n;
            }
            
            int m = int.Parse(Console.ReadLine().Split()[0]); // m = aantal bestelde blokken
            int[] bestellingen = new int[m];
            long schijfjes = 0;     
            //het totale aantal schijfjes dat gebruikt moet worden voor ALLE bestellingen in de input. Beginnend op 0 schijfjes.
            for (int t = 0; t < m; t++)
            {
                bestellingen[t] = int.Parse(Console.ReadLine().Split()[0]);
            }
            
            for(int i = 0; i < m; i++)
            {
                int standaardblok;
                if (catalogus.Length > 0)    //Als de catalogus leeg is, dan kan er ook niet in gezocht worden.
                {
                    standaardblok = BinarySearch(catalogus, 0, n - 1, bestellingen[i]);
                }

                else
                {
                    standaardblok = 0;      //Bij een lege catalogus is de waarde van het steunblok daarom 0, oftewel, geen steunblok
                }

                schijfjes = schijfjes + (bestellingen[i] - standaardblok); 
                /*Berekening voor het benodigde aantal schijfes voor de bestellingen tot nu toe. 
                  Het gevonden standaardblok wordt van de waarde(=hoogte) van het bestelde steunblok getrokken,
                  om zo het aantal benodigde schijfjes te berekenen. 
                  Dit aantal wordt bij het totale aantal schijfjes opgeteld, zodat we uiteindelijk het totale aantal schijfjes krijgen.*/
                
            }
            Console.WriteLine(schijfjes); //output
            Console.ReadLine();
        }

        /*Binair zoeken methode. De methode gaat in de catalogus op zoek naar het juiste standaardblok voor de bestelling.*/
        static int BinarySearch(int[] catalogus, int links, int rechts, int x)
        {
            int beste = 0;  
            /*deze int representeert het beste standaardblok dat gebruikt kan worden.
             * het begint op 0 en wordt bij elke gevonden compatibele standaardblok geüpdate naar de waarde van dat standaardblok.
             * Zodra er een te groot standaardblok wordt gevonden, kunnen we dan terugvallen op deze waarde.*/

            while (links < rechts)
            {
                int midden = links + (rechts - links) / 2; //vind en update het midden na aanpassing van de grenzen
                if (catalogus[midden] < x)
                {
                    beste = catalogus[midden]; //update de waarde van beste
                    links = midden + 1;         //Als midden < x, verplaats ondergrens
                }
                else
                {
                    rechts = midden; //Als midden >= x, verplaats bovengrens
                }
            }
            if (catalogus[links] <= x)
            {
                return catalogus[links]; //Als het gevonden standaardblok <= x dan is deze compatibel, dus returnen we deze waarde
            }

            else
            {
                return beste; //Anders returnen we het beste alternatief
            }
        }
    }
}
