using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;


namespace CI_Sudoku2
{
    public partial class Form1 : Form
    {
        public int grid = 9;                           // For a 9 * 9 grid
        float sudokuSize = 420;                 // Sudoku length and width
        int normal = 2;                         // Line thickness grid
        int bold = 4;                           // Line thickness grid bold
        float cellSize;                         // Cell size for drawing
        Sudoku Sudoku;                          // Sudoku object to store cells


        public Form1()
        {
            InitializeComponent();
            cellSize = sudokuSize / grid;       // Calculate cell size
            sudoku.Paint += Draw;               // Attach Paint event
            Sudoku = new Sudoku(grid);          // Initialize Sudoku object

            WarmUp();

        }
        private void WarmUp()
        {
            Console.WriteLine("Warming up...");
            //var dummySudoku = new Sudoku(grid);

            // Fill a dummy grid
            string dummyInput = string.Join(" ", Enumerable.Repeat("0", 81)); // Create an empty grid
            Fill(dummyInput);

            // Run dummy algorithms
            CBT(Sudoku, 0, 0);

            Fill(dummyInput);       //Fill again (reset) with dummy input for FC method
            Make_Node_Consistent(Sudoku);
            FC(Sudoku, 0, 0);

            Fill("0 0 3 0 2 0 6 0 0 9 0 0 3 0 5 0 0 1 0 0 1 8 0 6 4 0 0 0 0 8 1 0 2 9 0 0 7 0 0 0 0 0 0 0 8 0 0 6 7 0 8 2 0 0 0 0 2 6 0 9 5 0 0 8 0 0 2 0 3 0 0 9 0 0 5 0 1 0 3 0 0");       //Fill again (reset) with dummy input for FC method
            Make_Node_Consistent(Sudoku);
            var cellsWithDomains = Sudoku.Cells
               .Where(cell => !cell.Fixed) //don't include fixed cells
               .Select(cell => new Sudoku.CellsWithDomain { cell = cell })
               .ToList(); //Create list of CellsWithDomain object, which has the cell and its (dynamic) domain as its properties
            FC_MCV(Sudoku, Sudoku.Cell_Domain);

            Sudoku = new Sudoku(grid); //Reset Sudoku

            Console.WriteLine("Warm-up complete.");
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

                if (cell.Fixed)
                    gr.DrawString(cell.Number.ToString(), normal, black, x, y);         // Fixed number is black
                else if (cell.Number != 0)
                    gr.DrawString(cell.Number.ToString(), normal, gray, x, y);          // Unfixed number is grey
                
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
                cell.Valid = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };     // Create domain for each cell
                

                Sudoku.Columns[col].Cells.Add(cell);                        // Add Cell to Rows
                Sudoku.Rows[row].Cells.Add(cell);                           // Add Cell to Columns
                Sudoku.Blocks[block].Cells.Add(cell);                       // Add Cell to Blocks
                
                if(number != 0) //add fixed numbers to number lists (i.e. the list of numbers that are present in the row, column or block)
                {
                    //Remove fixed numbers from the domains of the row, column and block it is in
                    Sudoku.Columns[col].Missing.Remove(number);
                    Sudoku.Rows[row].Missing.Remove(number);
                    Sudoku.Blocks[block].Missing.Remove(number);
                }
                
            }

            sudoku.Invalidate();                            // Update sudoku UI
        }

        private bool CBT(Sudoku Sudoku, int row_idx, int col_idx) //Chronological Backtracking 
        {
            //Base Case: Stop at the end of the grid, it should then be solved (true)
            if (row_idx == grid)
            {
                return true;
            }
            int next_row;
            if (col_idx == grid - 1) //Go to next row if the current column is the last column, otherwise remain in the same row
            {
                next_row = row_idx + 1;
            }
            else
            {
                next_row = row_idx;
            }
            int next_col = (col_idx + 1) % grid; //The modulo ensures we cycle back to 0 when we are at the end of the grid
            Sudoku.Cell cell = Sudoku.Rows[row_idx].Cells[col_idx];

            if (cell.Fixed)
            {
                return CBT(Sudoku, next_row, next_col); //recursive call with next row and next column, so that we skip fixed numbers
            }
            int block_idx = (row_idx / 3) * 3 + (col_idx / 3);

            for (int num = 1; num <= grid; num++)
            {
                //We try every number from 1 to 9 (from low to high) until we find a number that does not violate any constraints
                if (Sudoku.Rows[row_idx].Missing.Contains(num) &&
                    Sudoku.Columns[col_idx].Missing.Contains(num) &&
                    Sudoku.Blocks[block_idx].Missing.Contains(num)) //Only place the number if no constraints are violated
                {
                    cell.Number = num; //Place number
                    Sudoku.Rows[row_idx].Missing.Remove(num); //Update domains (missing lists) for future reference
                    Sudoku.Columns[col_idx].Missing.Remove(num);
                    Sudoku.Blocks[block_idx].Missing.Remove(num);

                    if (CBT(Sudoku, next_row, next_col))
                    {
                        return true; //Only go to the next cell if a number was placed in the current cell
                    }

                    //Backtrack by undoing placement
                    cell.Number = 0;
                    Sudoku.Rows[row_idx].Missing.Add(num);
                    Sudoku.Columns[col_idx].Missing.Add(num);
                    Sudoku.Blocks[block_idx].Missing.Add(num);
                }
            }
            return false;
        }

        private void Make_Node_Consistent(Sudoku Sudoku)
        {
            foreach(var row in Sudoku.Rows.Values)
            {
                foreach(var cell in row.Cells)
                {

                    if(cell.Fixed) //if cell is fixed the domain should only contain the number that is in the cell
                    {
                        cell.Valid.Clear();
                        cell.Valid.Add(cell.Number);
                    }

                    else //if the cell is not fixed, we need to check what numbers are present in the row, column and block of the cell and remove these numbers from the domain of this cell
                    {
                        var col = Sudoku.Columns[cell.X];                   //Get column of this cell
                        int block_idx = (cell.Y / 3) * 3 + (cell.X / 3);    //calculate index of the block this cell belongs to
                        var block = Sudoku.Blocks[block_idx];               //Get block of this cell

                        cell.Valid.IntersectWith(row.Missing);
                        cell.Valid.IntersectWith(col.Missing);
                        cell.Valid.IntersectWith(block.Missing);
                        //The domain of the cell is equal to the intersection of the domains of its row, column and block
                        //i.e. everything that is still missing in the row, column and block
                    }
                }
            }
        }

        private bool FC(Sudoku Sudoku, int row_idx, int col_idx)
        {
            //This method is similar to the CBT algorithm, but it uses constraints to manipulate the domains of cells/variables
            if (row_idx == grid) //Base case: All rows are processed
                return true;

            int next_row;
            if(col_idx == grid - 1)
            {
                next_row = row_idx + 1;
            }

            else
            {
                next_row = row_idx;
            }

            int next_col = (col_idx + 1) % grid;

            var cell = Sudoku.Rows[row_idx].Cells[col_idx];
            if (cell.Fixed)
            {
                return FC(Sudoku, next_row, next_col); //Skip fixed cells
            }


            foreach (var num in cell.Valid.ToList()) //Iterate over a copy of the domain
            {
                cell.Number = num;

                var changes = ApplyConstraints(Sudoku, cell, num);      //Apply constraints and keep track of the changes that were made
                if(changes != null && FC(Sudoku, next_row, next_col))   //If changes == null, some domain has become empty making the update invalid
                                                                        //If the recursive call is also true, then we return true because the number we filled in is not violating constraints
                {
                    return true;
                }

                UndoChanges(Sudoku, changes, cell, num); //Restore changes when backtracking
                
            }

            // If no valid number works, reset the cell and backtrack
            cell.Number = 0;
            return false;
        }

        private bool FC_MCV(Sudoku Sudoku, List<Sudoku.CellsWithDomain> Cell_Domain) //Forward Checking with Most-Constrained-Variable Heuristic
        {
            //Base Case: All Cells are processed, i.e. List is empty
            if (Cell_Domain.Count == 0)
            {
                return true;
            }
            
            Cell_Domain = Cell_Domain.OrderBy(c => c.Domain_Size).ToList();     //sort cells with domains on domain size into a new list (sort of Stack)

            var selectedCell = Cell_Domain.First();                             //Take first cell (most-constrained)
            Sudoku.Cell cell = selectedCell.cell;
            Cell_Domain.Remove(selectedCell);                                   //remove first cell from list (Pop off stack)


            foreach (var num in cell.Valid.ToList()) //Iterate over a copy of the domain
            {
                cell.Number = num;

                var changes = ApplyConstraints2(Sudoku, cell, num);      //Apply constraints and keep track of the changes that were made
                if (changes != null)    //If changes == null, some domain has become empty making the update invalid                       
                {

                    if (FC_MCV(Sudoku, Cell_Domain))
                    {
                        return true; //If the recursive call is true, then we return true because the number we filled in is not violating constraints
                    }
                    
                }

                UndoChanges(Sudoku, changes, cell, num); //Restore changes when backtracking

            }

            // If no valid number works, reset the cell and backtrack
            cell.Number = 0;
            return false;
        }

        private List<(Sudoku.Cell, int)> ApplyConstraints(Sudoku Sudoku, Sudoku.Cell cell, int num) 
            //This method is used for FC and does not loop over the whole row, column and block to update the domains every time a number is filled into a cell
            //This saves time as we dont have to go through the whole row, column and block every time
            //We go through the sudoku row by row, therefore we can skip the cells we already filled in when updating domains
        {
            var changes = new List<(Sudoku.Cell, int)>();

            var (row, col, block, block_idx) = Sudoku.Get_Cell_Info(cell);

            var relevant_row = row.Cells.Skip(cell.X);                                          //Skip all cells in the row until the cell we just filled in (i.e. everything that was already filled in)
            var relevant_col = col.Cells.Skip(cell.Y);                                          //Skip all cells in the column until the cell we just filled in
            var relevant_block = block.Cells.Skip(block_idx);                                   //Skip all cells in the block that has already been filled in
            var relevant_cells = relevant_row.Concat(relevant_col).Concat(relevant_block);      //Concatenate the lists of relevant cells, so we can use a single loop

            foreach (var relevant_cell in relevant_cells)   //row.Cells.Concat(col.Cells).Concat(block.Cells)) 
                //SWITCHED TO DIFFERENT STRATEGY FOR THE PURPOSE OF EFFICIENCY (PUT IN REPORT!)
            {
                if (relevant_cell != cell && !relevant_cell.Fixed && relevant_cell.Valid.Contains(num))
                {
                    relevant_cell.Valid.Remove(num);
                    changes.Add((relevant_cell, num));

                    if (relevant_cell.Valid.Count == 0) // Empty domain found
                    {
                        foreach (var (changedCell, removedNum) in changes)
                        {
                            changedCell.Valid.Add(removedNum); //Undo the changes we made during this call of Apply_Constraints
                        }
                        return null; // Indicate an invalid state
                    }
                }
            }
            row.Missing.Remove(num);
            col.Missing.Remove(num);
            block.Missing.Remove(num);
            return changes; // Return all changes for later restoration
        }

        private List<(Sudoku.Cell, int)> ApplyConstraints2(Sudoku Sudoku, Sudoku.Cell cell, int num) 
            //For FC_MCV it is necessary to go over the whole row, column and block
            //We do not go row by row anymore, therefore we have to update the full rows, columns and blocks every time we fill in a number into a cell
            //(Because we are not keeping track of what has or has not been filled in yet)
        {
            var changes = new List<(Sudoku.Cell, int)>();
            var (row, col, block, block_idx) = Sudoku.Get_Cell_Info(cell);
            var relevant_cells = row.Cells.Concat(col.Cells).Concat(block.Cells);      //Concatenate the lists of relevant cells, so we can use a single loop

            foreach (var relevant_cell in relevant_cells)  
            {
                if (relevant_cell != cell && !relevant_cell.Fixed && relevant_cell.Valid.Contains(num))
                {
                    relevant_cell.Valid.Remove(num);
                    changes.Add((relevant_cell, num));

                    if (relevant_cell.Valid.Count == 0) // Empty domain found
                    {
                        foreach (var (changedCell, removedNum) in changes)
                        {
                            changedCell.Valid.Add(removedNum); //Undo the changes we made during this call of Apply_Constraints2
                        }
                        return null; // Indicate an invalid state
                    }
                }
            }
            row.Missing.Remove(num);
            col.Missing.Remove(num);
            block.Missing.Remove(num);
            return changes; // Return all changes for later restoration
        }

        private void UndoChanges(Sudoku Sudoku, List<(Sudoku.Cell, int)> changes, Sudoku.Cell cell, int num)
            //This method is used when backtracking to undo the changes that were made during Apply_Constraints(2)
            //These changes initially did not cause empty domains, but have to be undone due to problems in a following state
        {
            if (changes == null)
                return;

            foreach (var (changed_cell, removed_num) in changes)
            {
                changed_cell.Valid.Add(removed_num); // Restore removed values
            }

            var (row, col, block, block_idx) = Sudoku.Get_Cell_Info(cell);

            row.Missing.Add(num);
            col.Missing.Add(num);
            block.Missing.Add(num);
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

        private void button4_Click(object sender, EventArgs e) //Single Solve CBT
        {
            Fill(label1.Text);

            Stopwatch stopwatch = new Stopwatch();         //Create stopwatch
            stopwatch.Start();                             //start stopwatch to measure time that ellapses

            CBT(Sudoku, 0, 0);

            stopwatch.Stop();

            long elapsedTicks = stopwatch.ElapsedTicks;
            double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency;
            if (comboBox1.SelectedIndex != -1)
                Console.WriteLine($"Chronological Backtracking -> {comboBox1.Items[comboBox1.SelectedIndex]}: " + elapsedMilliseconds + " ms");
            else
                Console.WriteLine($"Chronological Backtracking -> {elapsedMilliseconds} ms");
        }
        private void button2_Click(object sender, EventArgs e) //Multiple Solve CBT
        {
            int iterations = int.Parse(textBox_Runs.Text);
            double[] solving_times = new double[iterations];
            for (int run = 0; run < iterations; run++)
            {
                Fill(label1.Text);

                Stopwatch stopwatch = new Stopwatch();         //Create stopwatch
                stopwatch.Start();                             //start stopwatch to measure time that ellapses

                CBT(Sudoku, 0, 0);

                stopwatch.Stop();

                long elapsedTicks = stopwatch.ElapsedTicks;
                double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency;
                solving_times[run] = elapsedMilliseconds;
            }

            double total_time = 0.000;
            foreach (double time in solving_times)
            {
                total_time = total_time + time;
            }

            double avg_time = total_time / iterations;
            if (comboBox1.SelectedIndex != -1)
                Console.WriteLine($"Average time with Chronological Backtracking for {iterations} iterations -> {comboBox1.Items[comboBox1.SelectedIndex]}: " + avg_time + " ms");
            else
                Console.WriteLine($"Average time with Chronological Backtracking for {iterations} iterations -> {avg_time} ms");
            sudoku.Invalidate();
        }
        private void button3_Click(object sender, EventArgs e) //Single Solve FC
        {
            Fill(label1.Text); //reset Sudoku to initial state

            Make_Node_Consistent(Sudoku);

            Stopwatch stopwatch = new Stopwatch();  //create stopwatch
            stopwatch.Start();                     //Start stopwatch to measur time that elapses during FC method

            FC(Sudoku, 0, 0); //execute forward checking algorithm

            stopwatch.Stop();                      //Stop stopwatch
            long elapsedTicks = stopwatch.ElapsedTicks;
            double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency; //Calculate elapsed time for FC method
            if (comboBox1.SelectedIndex != -1)
                Console.WriteLine($"Forward Checking row by row -> {comboBox1.Items[comboBox1.SelectedIndex]}: " + elapsedMilliseconds + " ms");
            else
                Console.WriteLine($"Forward Checking row by row -> {elapsedMilliseconds} ms");
        }
        private void button5_Click(object sender, EventArgs e) //Multiple Solve FC
        {
            int iterations = int.Parse(textBox_Runs.Text);
            double[] solving_times = new double[iterations];
            for (int run = 0; run < iterations; run++)
            {
                Fill(label1.Text); //reset Sudoku to initial state
                Make_Node_Consistent(Sudoku);
                Stopwatch stopwatch = new Stopwatch();  //create stopwatch
                stopwatch.Start();                     //Start stopwatch to measur time that elapses during FC method

                FC(Sudoku, 0, 0); //execute forward checking algorithm

                stopwatch.Stop();                      //Stop stopwatch
                long elapsedTicks = stopwatch.ElapsedTicks;
                double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency; //Calculate elapsed time for FC method
                solving_times[run] = elapsedMilliseconds;
            }

            double total_time = 0.000;
            foreach (double time in solving_times)
            {
                total_time = total_time + time;
            }

            double avg_time = total_time / iterations;
            if (comboBox1.SelectedIndex != -1)
                Console.WriteLine($"Average Time with Forward Checking row by row for {iterations} iterations -> {comboBox1.Items[comboBox1.SelectedIndex]}: " + avg_time + " ms");
            else
                Console.WriteLine($"Average Time with Forward Checking row by row for {iterations} iterations -> {avg_time} ms");
            sudoku.Invalidate();
        }

        private void button6_Click(object sender, EventArgs e) //Single Solve FC_MCV
        {
            Fill(label1.Text); //reset Sudoku to initial state

            Make_Node_Consistent(Sudoku); //first make the sudoku node consistent
            var cellsWithDomains = Sudoku.Cells
                .Where(cell => !cell.Fixed) //don't include fixed cells
                .Select(cell => new Sudoku.CellsWithDomain { cell = cell })
                .ToList(); //Create list of CellsWithDomain object, which has the cell and its (dynamic) domain as its properties

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            FC_MCV(Sudoku, cellsWithDomains);

            stopwatch.Stop();
            long elapsedTicks = stopwatch.ElapsedTicks;
            double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency; //Calculate elapsed time for FC method
            if (comboBox1.SelectedIndex != -1)
                Console.WriteLine($"Forward Checking with MCV -> {comboBox1.Items[comboBox1.SelectedIndex]}: " + elapsedMilliseconds + " ms");
            else
                Console.WriteLine($"Forward Checking with MCV -> {elapsedMilliseconds} ms");
            sudoku.Invalidate(); 
        }
        private void button7_Click(object sender, EventArgs e) //Multiple Solve FC_MCV
        {
            int iterations = int.Parse(textBox_Runs.Text);
            double[] solving_times = new double[iterations];

            for (int run = 0; run < iterations; run++)
            {
                Fill(label1.Text); //reset Sudoku to initial state

                Make_Node_Consistent(Sudoku); //first make the sudoku node consistent
                var cellsWithDomains = Sudoku.Cells
                    .Where(cell => !cell.Fixed) //don't include fixed cells
                    .Select(cell => new Sudoku.CellsWithDomain { cell = cell })
                    .ToList(); //Create list of CellsWithDomain object, which has the cell and its (dynamic) domain as its properties

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                FC_MCV(Sudoku, cellsWithDomains);

                stopwatch.Stop();
                long elapsedTicks = stopwatch.ElapsedTicks;
                double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency; //Calculate elapsed time for FC method
                solving_times[run] = elapsedMilliseconds;
            }

            double total_time = 0.000;
            foreach (double time in solving_times)
            {
                total_time = total_time + time;
            }

            double avg_time = total_time / iterations;
            if (comboBox1.SelectedIndex != -1)
                Console.WriteLine($"Average Time with Forward Checking with MCV for {iterations} iterations -> {comboBox1.Items[comboBox1.SelectedIndex]}: " + avg_time + " ms");
            else
                Console.WriteLine($"Average Time with Forward Checking with MCV for {iterations} iterations -> {avg_time} ms");
            sudoku.Invalidate();

        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    public class Sudoku
    {
        public Dictionary<int, RowData> Rows { get; } = new Dictionary<int, RowData>();
        public Dictionary<int, ColumnData> Columns { get; } = new Dictionary<int, ColumnData>();
        public Dictionary<int, BlockData> Blocks { get; } = new Dictionary<int, BlockData>();
        public List<CellsWithDomain> Cell_Domain { get; set; } = new List<CellsWithDomain>();

        public Sudoku(int gridSize)
        {

            for (int i = 0; i < gridSize; i++)
            {
                Rows[i] = new RowData
                {
                    Cells = new List<Cell>(),
                    Missing = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                };

                Columns[i] = new ColumnData
                {
                    Cells = new List<Cell>(),
                    Missing = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                };
            }

            int blockSize = (int)Math.Sqrt(gridSize);
            for (int i = 0; i < blockSize * blockSize; i++)  //Doe je nu niet gwn de wortel in het kwadraat, dus dan kom je toch altijd weer uit op gridSize?
            {
                Blocks[i] = new BlockData
                {
                    Cells = new List<Cell>(),
                    Missing = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                };
            }
        }

        public (RowData row, ColumnData col, BlockData block, int block_idx) Get_Cell_Info(Cell cell)
        {
            var row = Rows[cell.Y];
            var col = Columns[cell.X];
            int block_idx = (cell.Y / 3) * 3 + (cell.X / 3);
            var block = Blocks[block_idx];
            return (row, col, block, block_idx);
        }

        public IEnumerable<Cell> Cells => Rows.Values.SelectMany(row => row.Cells);

        public class RowData
        {
            public List<Cell> Cells { get; set; } = new List<Cell>();
            public HashSet<int> Missing { get; set; } = new HashSet<int>();

        }

        public class ColumnData
        {
            public List<Cell> Cells { get; set; } = new List<Cell>();
            public HashSet<int> Missing { get; set; } = new HashSet<int>();
        }

        public class BlockData
        {
            public List<Cell> Cells { get; set; } = new List<Cell>();
            public HashSet<int> Missing { get; set; } = new HashSet<int>();
        }

        public class Cell
        {
            public int X { get; }               // Column (grid)
            public int Y { get; }               // Row (grid)
            public int Number { get; set; }     // Number (1-9)
            public bool Fixed { get; set; }          // Fixed = TRUE     Unfixed = FALSE

            public HashSet<int> Valid;             // List of valid values or the domain of the cell

            public Cell(int x, int y, int number, bool isFixed)
            {
                X = x;
                Y = y;
                Number = number;
                Fixed = isFixed;
            }
        }
        public class CellsWithDomain
        {
            public Cell cell { get; set; }
            public int Domain_Size => cell.Valid.Count;  //Calculates the domain size of the cell dynamically
        }
    }

    
}
