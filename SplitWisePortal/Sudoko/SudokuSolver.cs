using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoko
{
   public class SudokuSolver
    {
        // True values for row, grid, and region constraint matrices
        // mean that they contain that candidate, inversely,
        // True values in the candidate constraint matrix means that it
        // is a possible value for that cell.
        Candidate[,] m_cellConstraintMatrix;
        Candidate[] m_rowConstraintMatrix;
        Candidate[] m_colConstraintMatrix;
        Candidate[,] m_regionConstraintMatrix;

        // Actual puzzle grid (uses 0s for unsolved squares)
        int[,] m_grid;

        // Another convenience structure. Easy and expressive way
        // of passing around row, column information.

        struct Cell
        {
            public int row, col;
            public Cell(int r, int c) { row = r; col = c; }
        }

        // helps avoid iterating over solved squares
        HashSet<Cell> solved;
        HashSet<Cell> unsolved;

        // Tracks the cells changed due to propagation (i.e. the rippled cells)
        Stack<HashSet<Cell>> changed;

        HashSet<Cell>[] bucketList;
        int steps;

        public SudokuSolver(int[,] initialGrid)
        {
            m_grid = new int[9, 9];
            m_cellConstraintMatrix = new Candidate[9, 9];
            m_rowConstraintMatrix = new Candidate[9];
            m_colConstraintMatrix = new Candidate[9];
            m_regionConstraintMatrix = new Candidate[9, 9];
            solved = new HashSet<Cell>();
            unsolved = new HashSet<Cell>();
            changed = new Stack<HashSet<Cell>>();
            bucketList = new HashSet<Cell>[10];
            steps = 0;

            // initialize constraints

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    // copy grid, and turn on all Candidates for every cell
                    m_grid[row, col] = initialGrid[row, col];
                    m_cellConstraintMatrix[row, col] = new Candidate(9, true);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                m_rowConstraintMatrix[i] = new Candidate(9, false);
                m_colConstraintMatrix[i] = new Candidate(9, false);
                bucketList[i] = new HashSet<Cell>();
            }
            bucketList[9] = new HashSet<Cell>();

            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    m_regionConstraintMatrix[row, col] = new Candidate(9, false);

            InitializeMatrices();
            PopulateCandidates();
         var xyz =   NextCell();
            SolveRecurse(xyz);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(m_grid[i,j]);
                    Console.Write("\t");
                    
                }
                Console.WriteLine("\n");
            }
        }
        private void InitializeMatrices()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    // if the square is solved update the candidate list
                    // for the row, column, and region
                    if (m_grid[row, col] > 0)
                    {
                        int candidate = m_grid[row, col];
                        m_rowConstraintMatrix[row][candidate] = true;
                        m_colConstraintMatrix[col][candidate] = true;
                        m_regionConstraintMatrix[row / 3, col / 3][candidate] = true;
                    }
                }
            }
        }


        private void PopulateCandidates()
        {
            //Add possible candidates by checking
            //the rows, columns and grid
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    //if solved, then there are no possible candidates
                    if (m_grid[row, col] > 0)
                    {
                        m_cellConstraintMatrix[row, col].SetAll(false);
                        solved.Add(new Cell(row, col));
                    }
                    else
                    {
                        // populate each cell with possible candidates
                        // by checking the row, col, and grid associated 
                        // with that cell
                        foreach (int candidate in m_rowConstraintMatrix[row])
                            m_cellConstraintMatrix[row, col][candidate] = false;
                        foreach (int candidate in m_colConstraintMatrix[col])
                            m_cellConstraintMatrix[row, col][candidate] = false;
                        foreach (int candidate in m_regionConstraintMatrix[row / 3, col / 3])
                            m_cellConstraintMatrix[row, col][candidate] = false;

                        Cell c = new Cell(row, col);
                        unsolved.Add(c);
                    }
                }
            }
        }

        private Cell NextCell()
        {
            if (unsolved.Count == 0)
                return new Cell(-1, -1); // easy way to singal a solved puzzle

            Cell min = unsolved.First();
            foreach (Cell cell in unsolved)
                min = (m_cellConstraintMatrix[cell.row, cell.col].Count < m_cellConstraintMatrix[min.row, min.col].Count) ? cell : min;

            return min;
        }

    
        private void SelectCandidate(Cell aCell, int candidate)
        {
            HashSet<Cell> changedCells = new HashSet<Cell>();

            // place candidate on grid
            m_grid[aCell.row, aCell.col] = candidate;

            // remove candidate from cell constraint matrix
            m_cellConstraintMatrix[aCell.row, aCell.col][candidate] = false;

            // add the candidate to the cell, row, col, region constraint matrices
            m_colConstraintMatrix[aCell.col][candidate] = true;
            m_rowConstraintMatrix[aCell.row][candidate] = true;
            m_regionConstraintMatrix[aCell.row / 3, aCell.col / 3][candidate] = true;

            /**** RIPPLE ACROSS COL, ROW, REGION ****/

            // (propagation)
            // remove candidates across unsolved cells in the same
            // row and col.
            for (int i = 0; i < 9; i++)
            {
                // only change unsolved cells containing the candidate
                if (m_grid[aCell.row, i] == 0)
                {
                    if (m_cellConstraintMatrix[aCell.row, i][candidate] == true)
                    {
                        // remove the candidate
                        m_cellConstraintMatrix[aCell.row, i][candidate] = false;

                        //update changed cells (for backtracking)
                        changedCells.Add(new Cell(aCell.row, i));
                    }
                }
                // only change unsolved cells containing the candidate
                if (m_grid[i, aCell.col] == 0)
                {
                    if (m_cellConstraintMatrix[i, aCell.col][candidate] == true)
                    {
                        // remove the candidate
                        m_cellConstraintMatrix[i, aCell.col][candidate] = false;

                        //update changed cells (for backtracking)
                        changedCells.Add(new Cell(i, aCell.col));
                    }
                }
            }

            // (propagation)
            // remove candidates across unsolved cells in the same
            // region.
            int grid_row_start = aCell.row / 3 * 3;
            int grid_col_start = aCell.col / 3 * 3;
            for (int row = grid_row_start; row < grid_row_start + 3; row++)
                for (int col = grid_col_start; col < grid_col_start + 3; col++)
                    // only change unsolved cells containing the candidate
                    if (m_grid[row, col] == 0)
                    {
                        if (m_cellConstraintMatrix[row, col][candidate] == true)
                        {
                            // remove the candidate
                            m_cellConstraintMatrix[row, col][candidate] = false;

                            //update changed cells (for backtracking)
                            changedCells.Add(new Cell(row, col));
                        }
                    }

            // add cell to solved list
            unsolved.Remove(aCell);
            solved.Add(aCell);
            changed.Push(changedCells);
        }

        private void UnselectCandidate(Cell aCell, int candidate)
        {
            // 1) Remove selected candidate from grid
            m_grid[aCell.row, aCell.col] = 0;

            // 2) Add that candidate back to the cell constraint matrix.
            //    Since it wasn't selected, it can still be selected in the 
            //    future
            m_cellConstraintMatrix[aCell.row, aCell.col][candidate] = true;

            // 3) Remove the candidate from the row, col, and region constraint matrices
            m_rowConstraintMatrix[aCell.row][candidate] = false;
            m_colConstraintMatrix[aCell.col][candidate] = false;
            m_regionConstraintMatrix[aCell.row / 3, aCell.col / 3][candidate] = false;

            // 4) Add the candidate back to any cells that changed from
            //    its selection (propagation).
            foreach (Cell c in changed.Pop())
            {
                m_cellConstraintMatrix[c.row, c.col][candidate] = true;
            }

            // 5) Add the cell back to the list of unsolved
            solved.Remove(aCell);
            unsolved.Add(aCell);
        }
        private bool SolveRecurse(Cell nextCell)
        {
            // Our base case: No more unsolved cells to select, 
            // thus puzzle solved
            if (nextCell.row == -1)
                return true;

            // Loop through all candidates in the cell
            foreach (int candidate in m_cellConstraintMatrix[nextCell.row, nextCell.col])
            {
                Console.WriteLine("{4} -> ({0}, {1}) : {2} ({3})", nextCell.row, nextCell.col,
                    m_cellConstraintMatrix[nextCell.row, nextCell.col], m_cellConstraintMatrix[nextCell.row, nextCell.col].Count, steps++);

                SelectCandidate(nextCell, candidate);

                // Move to the next cell.
                // if it returns false backtrack
                if (SolveRecurse(NextCell()) == false)
                {
                    ++steps;
                    Console.WriteLine("{0} -> BACK", steps);
                    UnselectCandidate(nextCell, candidate);
                    continue;
                }
                else // if we recieve true here this means the puzzle was solved earlier
                    return true;
            }

            // return false if path is unsolvable
            return false;

        }

    }
}
