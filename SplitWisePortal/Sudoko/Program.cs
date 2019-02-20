using System;

namespace Sudoko
{
    class Program
    {
        static void Main(string[] args)
        {

            int[,] vs = new int[9, 9] { { 9, 0, 6,5,0,7,0,2,0 },{8,0,0,0,0,0,3,7,0},{0,0,0,3,0,2,0,0,0 },{0,6,0,0,0,0,0,0,2},{0,9,0,0,7,0,0,4,0},{2,0,0,0,0,0,0,9,0},{0,0,0,4,0,3,0,0,0},{0,1,3,0,0,0,0,0,4},{0,4,0,1,0,5,2,0,7} }; 
            SudokuSolver solver = new SudokuSolver(vs);
            Console.WriteLine("Hello World!");
        }
    }
}
