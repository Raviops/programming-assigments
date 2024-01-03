using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toetsenspion23_2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine().Split()[0]);
            List<string> wachtwoorden = new List<string>();

            for(int i = 0; i < n; i++)
            {
                StringBuilder wachtwoord = new StringBuilder();
                MijnLinkedList L = new MijnLinkedList();
                MijnElem cursor = L.Head; //cursor verwijst naar een element in de lijst, nieuwe elementen komen worden na de cursor ingevoegd
                string invoer = Console.ReadLine().Split()[0];

                for(int j = 0; j < invoer.Length; j++)
                {
                    switch(invoer[j])
                    {
                        case '<':
                            if(cursor.prev != null)
                            {
                                cursor = cursor.prev;
                            }
                            break;

                        case '>':
                            if(cursor.next != L.Tail) //Misschien niet null maar L.Tail
                            {
                                cursor = cursor.next;
                            }
                            break;

                        case '-':
                            if (cursor != L.Head)
                            {
                                MijnLinkedList.Delete(L, cursor);
                                cursor = cursor.prev; 
                            }
                            break;

                        default:
                            MijnElem elem = new MijnElem(invoer[j]);
                            MijnLinkedList.Insert(L, elem, cursor);
                            cursor = elem;
                            break;
                    }
                }

                MijnElem item = L.Head.next;
                while (item != L.Tail) //null moet misschien L.Tail zijn
                {
                    wachtwoord.Append(item.key);
                    item = item.next;
                }
                wachtwoorden.Add(wachtwoord.ToString());
            }
            foreach(string ww in wachtwoorden)
            {
                Console.WriteLine(ww);
            }
            Console.ReadLine();
        }
    }

    class MijnLinkedList
    {
        public MijnElem Head;
        public MijnElem Tail;
        public MijnLinkedList()
        {
            this.Head = new MijnElem('\0'); //sentinel
            this.Tail = new MijnElem('\0'); //sentinel
            Head.next = Tail;
            Tail.prev = Head;
        }

        public static void Insert(MijnLinkedList L, MijnElem x, MijnElem y)
        /*Element toevoegen. x wordt ingevoegd na y.*/
        {
            x.next = y.next;
            x.prev = y;
            if (y.next != null)
            { 
                y.next.prev = x;
            }
            y.next = x;
        }

        public static void Delete(MijnLinkedList L, MijnElem x)
        /*Om x te verwijderen moeten we de pointers die naar x verwijzen veranderen, zodat niks meer naar x verwijst.
        x.prev.next = x.next en x.next.prev = x.prev*/
        {
            x.prev.next = x.next;
            if(x.next != null)
            {
                x.next.prev = x.prev;
            }
        }
    }
    class MijnElem
    {
        public char key;
        public MijnElem prev;
        public MijnElem next;

        public MijnElem(char element)
        {
            this.key = element;
        }
    }

}

