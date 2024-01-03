using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Reversi
{
    public partial class Form1 : Form
    {
        private Panel panel1;
        private const int gridSize = 50; // Define the size of each grid
        private Grid[,] grids; //2D-array that portrays the board
        private int rows;
        private int columns;
        private int aantalZetten;
        private int aantal_blauw = 2;
        private int aantal_rood = 2;
        Label blue_score = new Label();
        Label red_score = new Label();
        Label beurt = new Label();
        Button NieuwSpel_Knop = new Button();
        Button Help_Knop = new Button();
        Button instructions = new Button();
        ListView score_list = new ListView();
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(750, 600);
            panel1 = new Panel();
            Create_Panel(panel1);

            rows = panel1.Height / gridSize;
            columns = panel1.Width / gridSize;
            grids = new Grid[columns, rows];

            for(int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    grids[i, j] = new Grid(i, j);
                }
            }

            this.panel1.Paint += this.Speelveld;
            this.panel1.MouseClick += this.Speler;
            this.NieuwSpel_Knop.MouseClick += NieuwSpel_Knop_MouseClick;
            this.Help_Knop.MouseClick += Help_Knop_MouseClick;
            this.instructions.MouseClick += Instructions_MouseClick;
            Starting_Position();
            
        }

        private void Instructions_MouseClick(object sender, MouseEventArgs e)
        {
            string title = "Instructions";
            string message = "Welkom bij Reversi!\r\n" + "\r\n" +
                "Spelregels:\r\n" +
                "- Spelers spelen met rood of blauw.\r\n" +
                "- Het doel is om zoveel mogelijk stenen van jouw kleur op het bord te krijgen.\r\n" +
                "- Als je aan de beurt bent mag je een steen van jouw kleur op het bord plaatsen als je daarmee de minstens een steen van de tegenstander insluit.\r\n" +
                "- Insluiten kan horizontaal, verticaal of diagonaal en de van de tegenstander die ingesloten worden door jouw zet veranderen naar jouw kleur.\r\n" +
                "- Rood begint.\r\n" +
                "- Klik op Nieuw Spel om opnieuw te beginnen en als je even niet weet welke zetten je kunt doen dan kun je op de knop Help drukken voor hulp.\r\n" +
                "\r\n" +
                "Veel Plezier!";

            MessageBox.Show(message, title);
        }

        private void Help_Knop_MouseClick(object sender, MouseEventArgs e)
        {
            Help();
        }

        private void NieuwSpel_Knop_MouseClick(object sender, MouseEventArgs e)
        {
            Nieuw_Spel();
        }

        private void Nieuw_Spel()
        {
            foreach (Grid grid in grids)
            {
                grid.Occupied = false;
                grid.stone = null;
            }
            Stop();
            aantalZetten = 0;
            Starting_Position();
            Update_Score();
            beurt.Text = "Rood begint!";
            panel1.Invalidate();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
            red_score.Text = "Rood: " + aantal_rood;
            red_score.ForeColor = Color.Red;
            red_score.Location = new Point(50, 30);
            red_score.Size = new Size(100, 20);
            red_score.Font = new Font("Calibri", 12);
            this.Controls.Add(red_score);

            blue_score.Text = "Blauw: " + aantal_blauw;
            blue_score.ForeColor = Color.Blue;
            blue_score.Location = new Point(450, 30);
            blue_score.Size = red_score.Size;
            blue_score.Font = red_score.Font;
            this.Controls.Add(blue_score);

            Label lijst_titel = new Label();
            lijst_titel.Text = "Resultaten";
            lijst_titel.Font = new Font("Calibri", 14, FontStyle.Italic);
            lijst_titel.Location = new Point(610, 200);
            lijst_titel.Size = red_score.Size;
            this.Controls.Add(lijst_titel);

            score_list.Name = "Resultaten";
            score_list.Size = new Size(150, 300);
            score_list.Location = new Point(575, 225);
            score_list.View = View.Details;
            score_list.Columns.Add("Winnaar", 50, HorizontalAlignment.Left);
            score_list.Columns.Add("Rood", 50, HorizontalAlignment.Center);
            score_list.Columns.Add("Blauw", 50, HorizontalAlignment.Center);
            this.Controls.Add(score_list);

            beurt.Text = "Rood begint!";
            beurt.ForeColor = Color.Red;
            beurt.Font = red_score.Font;
            beurt.Size = new Size(200, 20);
            beurt.Location = new Point(235 ,30);
            this.Controls.Add(beurt);

            NieuwSpel_Knop.Text = "Nieuw Spel";
            NieuwSpel_Knop.Location = new Point(600,125);
            NieuwSpel_Knop.Size = new Size(100, 25);
            this.Controls.Add(NieuwSpel_Knop);

            Help_Knop.Size = NieuwSpel_Knop.Size;
            Help_Knop.Location = new Point(600, 160);
            Help_Knop.Text = "Help";
            this.Controls.Add(Help_Knop);

            instructions.Text = "Instructions";
            instructions.Size = NieuwSpel_Knop.Size;
            instructions.Location = new Point(600, 90);
            this.Controls.Add(instructions);
        }

        private void Update_Beurt()
        {
            List<string> rood = new List<string> { "Rood is aan zet!", "Nu Rood!", "De beurt is aan Rood!", "Rood mag!", "Pak 'm aan Rood!"};
            List<string> blue = new List<string> { "Blauw is aan zet!", "Nu Blauw!", "De beurt is aan Blauw!", "Blauw mag nu!", "Bekijk de opties goed Blauw!"};
            
            var random = new Random();
            int r = random.Next(rood.Count);

            if(aantalZetten % 2 == 0)
            {
                beurt.Text = rood[r];
                beurt.ForeColor= Color.Red;
            }

            else
            {
                beurt.Text = blue[r];
                beurt.ForeColor= Color.Blue;
            }
        }
        private void Update_Score()
        {
            aantal_rood = 0;
            aantal_blauw = 0;

            foreach (Grid grid in grids)
            {
                if(grid.stone != null)
                {
                    if (grid.stone.color == Color.Red)
                    {
                        aantal_rood += 1;
                    }
                    
                    else
                    {
                        aantal_blauw += 1;
                    }
                }
            }

            red_score.Text = "Rood: " + aantal_rood;
            blue_score.Text = "Blauw: " + aantal_blauw;

        }
        private void Create_Panel(Panel panel1)
        {
            panel1.Location = new Point(50, 50);
            panel1.BackColor = Color.Gray;
            panel1.Size = new Size(500, 500);
            this.Controls.Add(panel1);
        }

        private void Starting_Position()
        {
            Stone stone1 = new Stone(grids[4, 4], 0);  //creating starting position
            Stone stone2 = new Stone(grids[5, 4], 1);
            Stone stone3 = new Stone(grids[4, 5], 3);
            Stone stone4 = new Stone(grids[5, 5], 4);


            using (Graphics g = panel1.CreateGraphics())
            {
                stone1.Create_Stone(g);
                grids[4, 4].Occupied = true;
                grids[4, 4].stone = stone1;
                stone2.Create_Stone(g);
                grids[5, 4].Occupied = true;
                grids[5, 4].stone = stone2;
                stone3.Create_Stone(g);
                grids[4, 5].Occupied = true;
                grids[4, 5].stone = stone3;
                stone4.Create_Stone(g);
                grids[5, 5].Occupied = true;
                grids[5, 5].stone = stone4;
            }

            panel1.Invalidate();
        }
       
        private void Speelveld(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;

            if (panel != null)
            {
                int rows = panel.Height / gridSize;
                int columns = panel.Width / gridSize;

                foreach(Grid grid in grids)
                {
                    grid.Omtrek(e.Graphics);
                }
                
            }
            foreach (Grid grid in grids)
            {
                if (grid.Occupied && grid.stone != null)
                {
                    grid.stone.Create_Stone(e.Graphics);
                }
            }
        }

        private void Speler(object sender, MouseEventArgs e)
        {
            Panel panel = sender as Panel;

            if (panel != null)
            {
                int x = e.X/gridSize;
                int y = e.Y/gridSize;

                if (x < grids.GetLength(1) && y < grids.GetLength(0) && grids[x, y].Occupied == false)
                {
                    Stone played_stone = new Stone(grids[x, y], aantalZetten);

                    using (Graphics g = panel1.CreateGraphics())
                    {
                        List<Grid> buren = played_stone.Neighbours(grids);
                        List<Stone> valid = Ingesloten(played_stone, buren);
                        if (valid.Count > 0)
                        {
                            foreach (Stone stone in valid)
                            {
                                stone.color = played_stone.color; //update colors
                            }
                            // Update the stones and increment the turn count only if the move is valid
                            aantalZetten++;
                            Update_Beurt();
                            played_stone.Create_Stone(g); // Draw stone on the clicked grid
                            grids[x, y].Occupied = true;
                            grids[x, y].stone = played_stone;
                            panel1.Invalidate();
                            Update_Score();
                            
                            if(Einde())
                            {
                                Stop();
                            }
                        }
                    }  
                      
                }

            }
        }

        private List<Stone> Ingesloten(Stone played_stone, List<Grid> buren)
        {
            List<Stone> Ingesloten = new List<Stone>();

            for (int t = 0; t < buren.Count; t++)
            {
                List<Stone> In_Richting = new List<Stone>();
                int xOffset = buren[t].X - played_stone.grid.X;
                int yOffset = buren[t].Y - played_stone.grid.Y;

                int newX = played_stone.grid.X + xOffset;
                int newY = played_stone.grid.Y + yOffset;

                while(newX >= 0 && newX < columns && newY >= 0 && newY < rows)
                {
                    if(!grids[newX, newY].Occupied)
                    {
                        In_Richting.Clear(); //gesloten.Remove(grids[newX-xOffset,newY-yOffset].stone);
                        break;
                    }

                    else if (grids[newX, newY].stone.color != played_stone.color)
                    {
                        In_Richting.Add(grids[newX, newY].stone);
                    }

                    else if (grids[newX, newY].stone.color == played_stone.color)
                    {
                        Ingesloten.AddRange(In_Richting);
                        break;
                    }

                    newX += xOffset;
                    newY += yOffset;
                }

            }

            return Ingesloten;
        }


        private void Help() 
            //This method will provide a list of possible moves
            //by evaluating all of the grids and saving the ones that are valid moves
        {
            
            foreach (Grid grid in grids)
            {
                if(grid.stone == null && !grid.Occupied)
                {
                    Stone mogelijkheid = new Stone(grid, aantalZetten);

                    List<Grid> buren = mogelijkheid.Neighbours(grids);
                    using (Graphics g = panel1.CreateGraphics())
                    {
                        List<Stone> valid = Ingesloten(mogelijkheid, buren);
                        if(valid.Count > 0)
                        {
                            Pen pen = new Pen(Color.Green);
                            Rectangle rectangle = new Rectangle(grid.X * grid.Width + 25/2, grid.Y * grid.Height + 25/2, grid.Width/2, grid.Height/2);
                            g.DrawEllipse(pen, rectangle);
                        }
                    }
                        
                }
            }
        }

        private bool Einde()
        {
            //the game stops if at least one of the players cannot do any moves anymore
            foreach (Grid grid in grids)
            {
                if (grid.stone == null && !grid.Occupied)
                {
                    Stone mogelijkheid = new Stone(grid, aantalZetten);

                    List<Grid> buren = mogelijkheid.Neighbours(grids);
                    List<Stone> valid = Ingesloten(mogelijkheid, buren);
                    if(valid.Count > 0)
                    {
                        return false;
                    }

                }

            }

            return true;
        }

        private void Stop()
        {
            string message;
            //string winner;
            ListViewItem winnaar = null;
            
            if (aantal_rood > aantal_blauw)
            {
                winnaar = score_list.Items.Add("Rood");
                winnaar.ForeColor = Color.Red;
                message = "Rood Wint!";
            }

            else if(aantal_blauw > aantal_rood)
            {
                winnaar = score_list.Items.Add("Blauw");
                winnaar.ForeColor = Color.Blue;
                message = "Blauw Wint!";
            }

            else 
            {
                winnaar = score_list.Items.Add("Gelijkspel");
                winnaar.ForeColor = Color.Black;
                message = "Gelijkspel";
            }

            string title = "Einde Spel";
            winnaar.Font = new Font("Arial", 10, FontStyle.Bold);
            ListViewItem.ListViewSubItem rood = winnaar.SubItems.Add(aantal_rood.ToString());
            ListViewItem.ListViewSubItem blauw = winnaar.SubItems.Add(aantal_blauw.ToString());
            MessageBox.Show(message, title);
        }

    }

    public class Grid
    {
        public int X; //column number
        public int Y; //row number
        public int Width = 50;
        public int Height = 50;
        public bool Occupied = false;
        public Stone stone;
        public Grid(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Omtrek(Graphics g)
        {
            Pen blackpen = new Pen(Color.Black, 2);
            g.DrawRectangle(blackpen, X * Width, Y * Height, Width, Height);
        }

    }

    public class Stone
    {
        public Color color;
        public Grid grid;
        //public bool ingesloten = false;
        public Stone(Grid grid, int colour)
        {
            this.grid = grid;

            if (colour % 2 == 0)
            {
                color = Color.Red;
            }
            else
            {
                color = Color.Blue;
            }
             
        }

        public void Create_Stone(Graphics g)
        {
            Brush brush = new SolidBrush(color);
            Rectangle space = new Rectangle(grid.X * grid.Width, grid.Y * grid.Height, grid.Width, grid.Height);
            g.FillEllipse(brush, space);
        }
        public List<Grid> Neighbours(Grid[,] grids)
        {
            List<Grid> buren = new List<Grid>();

            for(int i = Math.Max(0, grid.X - 1); i <= Math.Min(grids.GetLength(0) - 1, grid.X + 1); i++)
            {
                for(int j = Math.Max(0, grid.Y - 1); j <= Math.Min(grids.GetLength(1) - 1, grid.Y + 1); j++)
                {
                    if(grids[i, j].stone != null && grids[i, j].stone.color != color)
                    {
                        buren.Add(grids[i, j]);
                    }
                }
            }

            return buren;

        }

    }
}
