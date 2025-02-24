using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CI_Sudoku2
{
    public partial class Form1 : Form
    {
        int grid = 9;                           // For a 9 * 9 grid
        float sudokuSize = 420;                 // Sudoku length and width
        int normal = 2;                         // Line thickness grid
        int bold = 4;                           // Line thickness grid bold
        Random random = new Random();           // Random for block picking and unfixed numbers filling
        float cellSize;                         // Cell size for drawing
        int S;                                  // S times random walk
        int N;                                  // N times hill climb
        int sudokuScore;                        // Total score, 0 = finished
        Sudoku Sudoku;                          // Sudoku object to store cells


        public Form1()
        {
            InitializeComponent();
            cellSize = sudokuSize / grid;       // Calculate cell size
            sudoku.Paint += Draw;               // Attach Paint event
            Sudoku = new Sudoku(grid);          // Initialize Sudoku object
        }

        private void Draw(object sender, PaintEventArgs e)                                          // Draw a sudoku
        {
            Graphics gr = e.Graphics;
            Pen pen = new Pen(Color.Black, this.normal);                // Normal line
            Pen penBold = new Pen(Color.Black, this.bold);              // Bold line
            Brush brush1 = new SolidBrush(Color.GhostWhite);            // Background color

            gr.FillRectangle(brush1, this.normal, this.normal, sudokuSize, sudokuSize);           // Fill background 
            gr.DrawRectangle(penBold, this.normal, this.normal, sudokuSize, sudokuSize);          // Draw outline bold

            for (int i = 1; i < grid; i++)                                              // Draw grid
            {
                float pos = this.normal + i * cellSize;                                 // X/Y position for lines
                Pen line = (i % 3 == 0) ? penBold : pen;                                // Each third line becomes bold
                gr.DrawLine(line, this.normal, pos, this.normal + sudokuSize, pos);     // Horizontal lines
                gr.DrawLine(line, pos, this.normal, pos, this.normal + sudokuSize);     // Vertical lines
            }

            Font normal = new Font("Arial", cellSize / 2);                          // Font for numbers
            Font bold = new Font("Arial", cellSize / 2, FontStyle.Bold);            // Bold font for fixed numbers if score==0

            Brush black = Brushes.Black;                            // Font color fixed (black)
            Brush gray = Brushes.Gray;                              // Font color unfixed (gray)

            foreach (var cell in Sudoku.Cells)                                          // Draw the numbers
            {
                float x = this.normal + cell.X * cellSize + cellSize / 5;               // Number placement X-axes
                float y = this.normal + cell.Y * cellSize + cellSize / 5;               // Number placement Y-axes

                if (sudokuScore == 0)
                {                                                                   // Sudoku is finished
                    if (cell.Fixed)
                        gr.DrawString(cell.Number.ToString(), bold, black, x, y);           // Fixed number is black and bold
                    else
                        gr.DrawString(cell.Number.ToString(), normal, black, x, y);         // Previously unfixed number is black
                }
                else
                {                                                                   // Sudoku is beginning
                    if (cell.Fixed)
                        gr.DrawString(cell.Number.ToString(), normal, black, x, y);         // Fixed number is black
                    else
                        gr.DrawString(cell.Number.ToString(), normal, gray, x, y);          // Unfixed number is grey
                }
            }
        }

        private void Fill(string text)                                                              // Organize the given numbers (input)
        {
            string[] input = text.Split(' ');                   // Remove spaces inbetween 

            if (input.Length != 81)                            // Check length (9*9=81)
            {
                MessageBox.Show("Input should contain 81 numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Sudoku = new Sudoku(grid);                      // Reset the Sudoku

            for (int i = 0; i < input.Length; i++)
            {
                int col = i % grid;                         // Column
                int row = i / grid;                         // Row
                int block = (row / 3) * 3 + (col / 3);      // Block 
                int number = int.Parse(input[i]);           // Parse number
                bool isFixed = number != 0;                 // Fixed if not zero

                var cell = new Sudoku.Cell(col, row, number, isFixed);          // Create cell

                Sudoku.Columns[col].Cells.Add(cell);                        // Add Cell to Rows
                Sudoku.Rows[row].Cells.Add(cell);                           // Add Cell to Columns
                Sudoku.Blocks[block].Cells.Add(cell);                       // Add Cell to Blocks
                if (!isFixed)
                    Sudoku.Blocks[block].Undefined.Add(cell);               // Add Cell to undefined list of blocks iff it's unfixed
            }

            UnfixedRandomFill(Sudoku);                      // Fill unfixed numbers semi random

            CalculateScore(Sudoku);                         // Initialize the Sudoku (only 1 time per Sudoku)

            sudoku.Invalidate();                            // Update sudoku UI
        }

        private void UnfixedRandomFill(Sudoku Sudoku)                                               // Unfixed cells get semi random number
        {
            foreach (var block in Sudoku.Blocks.Values)                                     // Iterate through blocks (9 times)
            {
                List<int> unusedNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };      // List of posible numbers

                foreach (var cel in block.Cells)                            // Iterate throug cells in block (9 times) (inside this loop 9*9=81 iterations)
                    if (cel.Number != 0)                                    // Remove number from posible numbers if it already excists as fixed cell in the same block
                        unusedNumbers.Remove(cel.Number);                   // Collect unfixed cells

                foreach (var cell in block.Undefined)                          // Assign random remaining numbers to empty cells
                {
                    int randomIndex = random.Next(unusedNumbers.Count);        // Get random index
                    cell.Number = unusedNumbers[randomIndex];                  // Assign random number with index
                    unusedNumbers.RemoveAt(randomIndex);                       // Remove used number from posible numbers list
                }
            }
        }

        private void SearchOperator(Sudoku Sudoku)                                                  // Hill climb (N times), calls EvaluateFunction and Update 
        {
            int randomBlockIndex = random.Next(0, 9);                       // Choose random block (0, .., 8)
            var unfixed = Sudoku.Blocks[randomBlockIndex].Undefined;        // Get list of unfixed cells within this block

            int currentScore;               // Current score to compare against best score
            int topScore = 0;               // Best score (less is better), score improvement needs to be 0 or lower in order to swap

            (Sudoku.Cell, Sudoku.Cell) topCells = (null, null);         // Place to save current best Cells to swap

            for (int i = 0; i < unfixed.Count; i++)                             // Switch all unfixed cells, starting with the first 
            {
                for (int j = i + 1; j < unfixed.Count; j++)                     // Switching it with the second
                {
                    currentScore = EvaluateFunction(Sudoku, unfixed[i], unfixed[j]);     // Score improvement for current switch = new current score

                    if (currentScore <= topScore)                   // If currentscore is better (lower) then previous topscore, it becomes new topscore
                    {
                        topScore = currentScore;                        // Update the top score
                        topCells = (unfixed[i], unfixed[j]);            // Store the best pair of cells
                    }
                }
            }

            if (topCells.Item1 != null && topCells.Item2 != null)           // If we found a better swap, topcells won't be empty, so we swap
            {
                sudokuScore += topScore;                                        // Update sudokuscore
                Update(Sudoku, topCells.Item1, topCells.Item2);                 // Update "missing" and "doubles" lists
                Sudoku.SwitchCells(topCells.Item1, topCells.Item2);             // Switch best cells (only their number)
            }
        }

        private int EvaluateFunction(Sudoku Sudoku, Sudoku.Cell cell1, Sudoku.Cell cell2)           // Calculates possible switch without actually switching
        {
            int score = 0;                                                              // Score is the score-change for the gien switch (between +4 and -4) lower is better

            if (cell1.Y != cell2.Y)                                                     // Y => Rows, if cell 1 and 2 are at the same row, the rowscore can't change 
            {
                if (Sudoku.Rows[cell1.Y].Missing.Contains(cell2.Number))                // If row1 is missing a number, the switch with this number (cell 2) would benefit
                    score--;
                if (Sudoku.Rows[cell2.Y].Missing.Contains(cell1.Number))                // If row2 is missing a number, the switch with this number (cell 1) would benefit
                    score--;

                if (!Sudoku.Rows[cell1.Y].Doubles.Contains(cell1.Number))               // If cell 1 is not double (so unique) in it's previous place, moving it, would be a disadvantage
                    score++;
                if (!Sudoku.Rows[cell2.Y].Doubles.Contains(cell2.Number))               // If cell 2 is not double (so unique) in it's previous place, moving it, would be a disadvantage
                    score++;
            }
            // Calculate Columns
            if (cell1.X != cell2.X)                                                     // X => Columns, if cell 1 and 2 are at the same row, the rowscore can't change 
            {
                if (Sudoku.Columns[cell1.X].Missing.Contains(cell2.Number))             // If column1 is missing a number, the switch with this number (cell 2) would benefit
                    score--;
                if (Sudoku.Columns[cell2.X].Missing.Contains(cell1.Number))             // If column2 is missing a number, the switch with this number (cell 1) would benefit
                    score--;

                if (!Sudoku.Columns[cell1.X].Doubles.Contains(cell1.Number))            // If cell 1 is not double (so unique) in it's previous place, moving it, would be a disadvantage
                    score++;
                if (!Sudoku.Columns[cell2.X].Doubles.Contains(cell2.Number))            // If cell 2 is not double (so unique) in it's previous place, moving it, would be a disadvantage
                    score++;
            }

            return score;           // return sum (between +4 => -4)
        }

        private void Update(Sudoku Sudoku, Sudoku.Cell cell1, Sudoku.Cell cell2)                    // After a switch, the missing and doubles lists need to be updated (exact same logic as EvaluateFunction)
        {
            // Update rows
            if (cell1.Y != cell2.Y)                                                             // Y => Rows, if cell 1 and 2 are at the same row, the row lists can't change
            {
                // Cell1 row
                if (Sudoku.Rows[cell1.Y].Missing.Contains(cell2.Number))                        // If row1 was missing a number, switching with this number, would remove it from the missing list
                    Sudoku.Rows[cell1.Y].Missing.Remove(cell2.Number);
                else
                    Sudoku.Rows[cell1.Y].Doubles.Add(cell2.Number);                             // If it wasn't missing, it is now a double, so we add it to the doubles list

                if (Sudoku.Rows[cell1.Y].Doubles.Contains(cell1.Number))                        // If row1 just switched a number that was in the doubles list, it should be removed from this list
                    Sudoku.Rows[cell1.Y].Doubles.Remove(cell1.Number);
                else
                    Sudoku.Rows[cell1.Y].Missing.Add(cell1.Number);                             // If this number was not a double, it was unique, which means it's now missing

                // Cell2 row
                if (Sudoku.Rows[cell2.Y].Missing.Contains(cell1.Number))                        // If row2 was missing a number, switching with this number, would remove it from the missing list
                    Sudoku.Rows[cell2.Y].Missing.Remove(cell1.Number);
                else
                    Sudoku.Rows[cell2.Y].Doubles.Add(cell1.Number);                             // If it wasn't missing, it is now a double, so we add it to the doubles list

                if (Sudoku.Rows[cell2.Y].Doubles.Contains(cell2.Number))                        // If row2 just switched a number that was in the doubles list, it should be removed from this list
                    Sudoku.Rows[cell2.Y].Doubles.Remove(cell2.Number);
                else
                    Sudoku.Rows[cell2.Y].Missing.Add(cell2.Number);                             // If this number was not a double, it was unique, which means it's now missing
            }

            // Update columns
            if (cell1.X != cell2.X)                                                             // X => Columns, if cell 1 and 2 are at the same column, the column lists can't change
            {
                // Cell1 column
                if (Sudoku.Columns[cell1.X].Missing.Contains(cell2.Number))                     // If column1 was missing a number, switching with this number, would remove it from the missing list
                    Sudoku.Columns[cell1.X].Missing.Remove(cell2.Number);
                else
                    Sudoku.Columns[cell1.X].Doubles.Add(cell2.Number);                          // If it wasn't missing, it is now a double, so we add it to the doubles list

                if (Sudoku.Columns[cell1.X].Doubles.Contains(cell1.Number))                     // If column1 just switched a number that was in the doubles list, it should be removed from this list
                    Sudoku.Columns[cell1.X].Doubles.Remove(cell1.Number);
                else
                    Sudoku.Columns[cell1.X].Missing.Add(cell1.Number);                          // If this number was not a double, it was unique, which means it's now missing

                // Cell2 column
                if (Sudoku.Columns[cell2.X].Missing.Contains(cell1.Number))                     // If column2 was missing a number, switching with this number, would remove it from the missing list
                    Sudoku.Columns[cell2.X].Missing.Remove(cell1.Number);
                else
                    Sudoku.Columns[cell2.X].Doubles.Add(cell1.Number);                          // If it wasn't missing, it is now a double, so we add it to the doubles list

                if (Sudoku.Columns[cell2.X].Doubles.Contains(cell2.Number))                     // If column2 just switched a number that was in the doubles list, it should be removed from this list
                    Sudoku.Columns[cell2.X].Doubles.Remove(cell2.Number);
                else
                    Sudoku.Columns[cell2.X].Missing.Add(cell2.Number);                          // If this number was not a double, it was unique, which means it's now missing
            }
        }

        private void RandomWalk(Sudoku Sudoku)                                                      // Random walk (S times)
        {
            int randomBlockIndex = random.Next(0, 9);                       // Choose random block
            var unfixed = Sudoku.Blocks[randomBlockIndex].Undefined;        // Unfixed is now the list of the unfixed cells in this block (this step isn't necesarry but keeps everything short and understandable)

            (Sudoku.Cell, Sudoku.Cell) randomCells = (null, null);          // Place to save the random cells we are going to switch (2 slots)

            int randomIndex1 = random.Next(unfixed.Count);                  // Random number (not the actual number we pick, but the index of the number we pick)
            randomCells.Item1 = unfixed[randomIndex1];                      // Picking the number with the index and adding it to our first slot

            int randomIndex2 = random.Next(unfixed.Count);                  // Same as before, but..
            while (randomIndex2 == randomIndex1)                            // It cannot be the same index as picked before, if it is the same.. 
                randomIndex2 = random.Next(unfixed.Count);                  // We pick another number
            randomCells.Item2 = unfixed[randomIndex2];                      // Filling the second slot

            int scorechange = EvaluateFunction(Sudoku, randomCells.Item1, randomCells.Item2);       // Calculate score change
            Update(Sudoku, randomCells.Item1, randomCells.Item2);                                   // Update missing and doubles lists

            sudokuScore += scorechange;                                                             // updating score with scorechange
            Sudoku.SwitchCells(randomCells.Item1, randomCells.Item2);                               // Actual switch of cells (numbers)
        }

        private void CalculateScore(Sudoku sudoku)                                                  // Initializes the missing and doubles lists at the beginning
        {
            sudokuScore = 0;                                                                        // Reset sudoku score to start fresh for the calculation

            // Calculate row scores
            foreach (var row in sudoku.Rows.Values)
            {
                HashSet<int> rowNumbers = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };           // Unused numbers (start with all valid numbers in a Sudoku row)
                List<int> rowDoubles = new List<int>();                                             // Use List to save all duplicates, including repeats

                foreach (var cell in row.Cells)                                                     // Loop through all cells in the current row
                {
                    if (!rowNumbers.Remove(cell.Number) && cell.Number != 0)                        // If the cell number cannot be removed from rowNumbers (already removed previously)
                        rowDoubles.Add(cell.Number);                                                // Add it to the Doubles list (it’s a duplicate)
                }

                row.Missing = rowNumbers.ToList();                                                  // Convert the remaining unused numbers to a list (these numbers are missing in the row)
                row.Doubles = rowDoubles;                                                           // Assign the Doubles list to the row

                sudokuScore += rowNumbers.Count;                                                    // Add the count of missing numbers in this row to the overall Sudoku score
            }

            // Calculate column scores
            foreach (var column in sudoku.Columns.Values)                                           // Loop through all columns in the Sudoku
            {
                HashSet<int> colNumbers = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };           // Unused numbers (start with all valid numbers in a Sudoku column)
                List<int> colDoubles = new List<int>();                                             // Use List to record all duplicates, including repeats

                foreach (var cell in column.Cells)                                                  // Loop through all cells in the current column
                {
                    if (!colNumbers.Remove(cell.Number) && cell.Number != 0)                        // If the cell number cannot be removed from colNumbers (already removed previously)
                        colDoubles.Add(cell.Number);                                                // Add it to the Doubles list (it’s a duplicate)
                }

                column.Missing = colNumbers.ToList();                                               // Convert the remaining unused numbers to a list (these numbers are missing in the column)
                column.Doubles = colDoubles;                                                        // Assign the Doubles list to the column

                sudokuScore += colNumbers.Count;                                                    // Add the count of missing numbers in this column to the overall Sudoku score
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)                                // Textbox => label1, if ENTER is pressed 
        {
            if (e.KeyCode == Keys.Enter && textBox1.Text.Length > 0)            // Check if Enter key was pressed and textbox is not empty
            {
                label1.Text = textBox1.Text;                                    // Show text in Label
                textBox1.Clear();                                               // Clear the TextBox
            }
        }

        private void button1_Click(object sender, EventArgs e)                                      // Adds Sudoku to combobox and Fills it in the UI
        {
            if (textBox1.Text.Length > 0)                                       // Check if textbox is not empty
            {
                label1.Text = textBox1.Text;                                    // Show text in Label
                textBox1.Clear();                                               // Clear the TextBox
                comboBox1.Items.Add(label1.Text);                               // Add it to the ComboBox
            }

            Fill(label1.Text);                                                  // Call Fill method with label content
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)                 // Combobox with given 5 Sudoku's
        {
            if (comboBox1.SelectedIndex >= 0 && comboBox1.SelectedIndex <= 4)                       // Let's you choose 1 out of 5
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        label1.Text = "0 0 3 0 2 0 6 0 0 9 0 0 3 0 5 0 0 1 0 0 1 8 0 6 4 0 0 0 0 8 1 0 2 9 0 0 7 0 0 0 0 0 0 0 8 0 0 6 7 0 8 2 0 0 0 0 2 6 0 9 5 0 0 8 0 0 2 0 3 0 0 9 0 0 5 0 1 0 3 0 0";
                        break;
                    case 1:
                        label1.Text = "2 0 0 0 8 0 3 0 0 0 6 0 0 7 0 0 8 4 0 3 0 5 0 0 2 0 9 0 0 0 1 0 5 4 0 8 0 0 0 0 0 0 0 0 0 4 0 2 7 0 6 0 0 0 3 0 1 0 0 7 0 4 0 7 2 0 0 4 0 0 6 0 0 0 4 0 1 0 0 0 3";
                        break;
                    case 2:
                        label1.Text = "0 0 0 0 0 0 9 0 7 0 0 0 4 2 0 1 8 0 0 0 0 7 0 5 0 2 6 1 0 0 9 0 4 0 0 0 0 5 0 0 0 0 0 4 0 0 0 0 5 0 7 0 0 9 9 2 0 1 0 8 0 0 0 0 3 4 0 5 9 0 0 0 5 0 7 0 0 0 0 0 0";
                        break;
                    case 3:
                        label1.Text = "0 3 0 0 5 0 0 4 0 0 0 8 0 1 0 5 0 0 4 6 0 0 0 0 0 1 2 0 7 0 5 0 2 0 8 0 0 0 0 6 0 3 0 0 0 0 4 0 1 0 9 0 3 0 2 5 0 0 0 0 0 9 8 0 0 1 0 2 0 6 0 0 0 8 0 0 6 0 0 2 0";
                        break;
                    case 4:
                        label1.Text = "0 2 0 8 1 0 7 4 0 7 0 0 0 0 3 1 0 0 0 9 0 0 0 2 8 0 5 0 0 9 0 4 0 0 8 7 4 0 0 2 0 8 0 0 3 1 6 0 0 3 0 2 0 0 3 0 2 7 0 0 0 6 0 0 0 5 6 0 0 0 0 8 0 7 6 0 5 1 0 9 0";
                        break;
                }

                Fill(label1.Text);                                                                  // Fill in sudoku choice
            }

            sudoku.Invalidate();                                                                    // Refreshes the UI to chosen sudoku
        }

        private void button4_Click(object sender, EventArgs e)
        {
            S = int.Parse(textBox_MinS.Text);
            N = int.Parse(textBox_MinN.Text);


            DateTime begin = DateTime.Now;

            while (sudokuScore != 0)
            {
                // Hill climb phase
                for (int i = 0; i < N; i++)
                {
                    SearchOperator(Sudoku);

                    if (sudokuScore == 0)
                        break;
                }

                // Random walk phase if not yet solved
                if (sudokuScore != 0)
                {
                    for (int j = 0; j < S; j++)
                        RandomWalk(Sudoku);
                }
            }

            // Record and display elapsed time
            DateTime eind = DateTime.Now;
            TimeSpan tijd = eind - begin;
            Console.WriteLine($"Time taken: {tijd}");

            sudoku.Invalidate(); // Final UI refresh
        }
        private async void button3_Click(object sender, EventArgs e)
        {
            var results = new List<(int S, int N, double MeanTime)>(); // Store results for sorting

            for (int n = int.Parse(textBox_MinN.Text); n <= int.Parse(textBox_MaxN.Text); n += int.Parse(textBox_IncN.Text))
            {
                N = n;

                for (int s = int.Parse(textBox_MinS.Text); s <= int.Parse(textBox_MaxS.Text); s += int.Parse(textBox_IncS.Text))
                {
                    S = s;

                    double totalMilliseconds = 0; // To store cumulative time for mean calculation
                    int iterations = int.Parse(textBox_Runs.Text); // Number of runs per combination

                    await Task.Run(() =>
                    {
                        for (int run = 0; run < iterations; run++)
                        {
                            int loops = 0;
                            Fill(label1.Text); // Reset the Sudoku puzzle to its initial state

                            DateTime begin = DateTime.Now;

                            while (sudokuScore != 0)
                            {
                                // Hill climb phase
                                for (int i = 0; i < N; i++)
                                {
                                    SearchOperator(Sudoku);
                                    if (sudokuScore == 0)
                                        break;
                                }

                                // Random walk phase if not yet solved
                                if (sudokuScore != 0)
                                {
                                    for (int j = 0; j < S; j++)
                                        RandomWalk(Sudoku);
                                }
                                loops++;
                            }

                            // Record elapsed time for this run
                            DateTime eind = DateTime.Now;
                            TimeSpan tijd = eind - begin;
                            totalMilliseconds += tijd.TotalMilliseconds;

                            Console.WriteLine($"Run {run + 1}/{iterations} | S = {S}, N = {N}, loops = {loops}, Time taken: {tijd}");
                        }
                    });

                    // Calculate mean time
                    double meanTime = totalMilliseconds / iterations;
                    results.Add((S, N, meanTime)); // Store result
                    Console.WriteLine($"Mean time for S = {S}, N = {N}: {meanTime:F2} ms");
                }
            }

            // Sort results by mean time and display
            var sortedResults = results.OrderBy(r => r.MeanTime).ToList();
            Console.WriteLine("\nSorted Results:");
            foreach (var result in sortedResults)
            {
                Console.WriteLine($"S = {result.S}, N = {result.N}, Mean Time = {result.MeanTime:F2} ms");
            }

            sudoku.Invalidate();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void TextBox_MinN_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox_MaxN_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox_MinS_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox_MaxS_TextChanged(object sender, EventArgs e)
        {
     
        }
    }

    public class Sudoku
    {
        public Dictionary<int, RowData> Rows { get; } = new Dictionary<int, RowData>();
        public Dictionary<int, ColumnData> Columns { get; } = new Dictionary<int, ColumnData>();
        public Dictionary<int, BlockData> Blocks { get; } = new Dictionary<int, BlockData>();


        public Sudoku(int gridSize)
        {
            for (int i = 0; i < gridSize; i++)
            {
                Rows[i] = new RowData
                {
                    Cells = new List<Cell>(),
                    Doubles = new List<int>(),
                    Missing = new List<int>()
                };

                Columns[i] = new ColumnData
                {
                    Cells = new List<Cell>(),
                    Doubles = new List<int>(),
                    Missing = new List<int>()
                };
            }

            int blockSize = (int)Math.Sqrt(gridSize);
            for (int i = 0; i < blockSize * blockSize; i++)
            {
                Blocks[i] = new BlockData
                {
                    Cells = new List<Cell>(),
                    Undefined = new List<Cell>()
                };
            }
        }

        public IEnumerable<Cell> Cells => Rows.Values.SelectMany(row => row.Cells);

        public void SwitchCells(Cell cell1, Cell cell2)
        {
            int temp = cell1.Number;
            cell1.Number = cell2.Number;
            cell2.Number = temp;
        }

        public class RowData
        {
            public List<Cell> Cells { get; set; } = new List<Cell>();
            public List<int> Doubles { get; set; } = new List<int>();
            public List<int> Missing { get; set; } = new List<int>();
        }

        public class ColumnData
        {
            public List<Cell> Cells { get; set; } = new List<Cell>();
            public List<int> Doubles { get; set; } = new List<int>();
            public List<int> Missing { get; set; } = new List<int>();
        }

        public class BlockData
        {
            public List<Cell> Cells { get; set; } = new List<Cell>();
            public List<Cell> Undefined { get; set; } = new List<Cell>();
        }

        public class Cell
        {
            public int X { get; }               // Column (grid)
            public int Y { get; }               // Row (grid)
            public int Number { get; set; }     // Number (1-9)
            public bool Fixed { get; }          // Fixed = TRUE     Unfixed = FALSE

            public Cell(int x, int y, int number, bool isFixed)
            {
                X = x;
                Y = y;
                Number = number;
                Fixed = isFixed;
            }
        }
    }
}
