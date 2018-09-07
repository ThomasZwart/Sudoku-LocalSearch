using System;
using System.Collections.Generic;

namespace SudukoLocalSearch
{
    class Sudoku
    {
        public int[,] grid;
        public HashSet<int>[] rowsUniques;
        public HashSet<int>[] columnUniques;
        public HashSet<Tuple<int, int>> fixedNumbers;

        private static readonly Random getRandom = new Random();

        public Sudoku(int[,] _sudoku)
        {
            grid = _sudoku;
            SetFixedNumbers();
            RandomFillSudoku();
            PopulateHashSets();
        }

        public void Swap(int row1, int column1, int row2, int column2)
        {
            // Swaps two numbers
            int temp = grid[row1, column1];
            grid[row1, column1] = grid[row2, column2];
            grid[row2, column2] = temp;

            // The 4 hashsets corresponding get cleared
            rowsUniques[row1].Clear();
            rowsUniques[row2].Clear();
            columnUniques[column1].Clear();
            columnUniques[column2].Clear();

            // And refilled
            for (int i = 0; i < Length; i++)
            {
                rowsUniques[row1].Add(grid[row1, i]);
                rowsUniques[row2].Add(grid[row2, i]);
                columnUniques[column1].Add(grid[i, column1]);
                columnUniques[column2].Add(grid[i, column2]);
            }
        }

        public Sudoku Clone() // Clones the current sudoku
        {
            // The grid cloned
            Sudoku clone = new Sudoku(grid.Clone() as int[,])
            // A new array for the hashsets
            {
                columnUniques = columnUniques.Clone() as HashSet<int>[],
                rowsUniques = rowsUniques.Clone() as HashSet<int>[]
            };

            // All hashsets cloned in arrays
            for (int i = 0; i < Length; i++)
            {
                clone.columnUniques[i] = new HashSet<int>(columnUniques[i]);
                clone.rowsUniques[i] = new HashSet<int>(rowsUniques[i]);
            }
            
            clone.fixedNumbers = new HashSet<Tuple<int, int>>(fixedNumbers);       
            return clone;
        }

        public int this[int index1, int index2] // Used to access the grid variable easily
        {
            get { return grid[index1, index2]; }
            
            set { grid[index1, index2] = value; }
        }

        public void SetFixedNumbers() // Sets all numbers that aren't blank as fixed numbers in the sudoku based on their indices                                                   
        {
            fixedNumbers = new HashSet<Tuple<int, int>>();
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (grid[i,j] != 0)
                    {
                        fixedNumbers.Add(new Tuple<int, int>(i, j)); // Tuple of (row, column) coordinates in the grid
                    }
                }
            }
        }

        public void PopulateHashSets() // Populates the hashsets for every column and row, to see how many unique numbers there are
        {
            columnUniques = new HashSet<int>[Length];
            rowsUniques = new HashSet<int>[Length];

            for (int i = 0; i < Length; i++) // Fills the hashset for every column and row
            {
                columnUniques[i] = new HashSet<int>();
                rowsUniques[i] = new HashSet<int>();
                for (int j = 0; j < Length; j++)
                {
                    columnUniques[i].Add(grid[j, i]);
                    rowsUniques[i].Add(grid[i, j]);
                }
            }
        }

        public int Length // The length of the sudoku
        {
            get { return grid.GetLength(0); }
        }

        public int GetEvaluationValue()
        {
            int evaluationValue = 0;

            for (int i = 0; i < Length; i++)
            {
                // The amount of possible unique numbers minus the unique numbers in that column/row in equal to the amount of missing numbers
                evaluationValue += Length - columnUniques[i].Count;
                evaluationValue += Length - rowsUniques[i].Count;
            }
            return evaluationValue;
        }

        public void WriteSudoku()
        {
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    Console.Write(grid[i, j]);
                }
                Console.Write("\n");
            }
        }

        public void RandomFillSudoku() // Randomly fills all the blank spaces in the sudoku so that all possible numbers appear once in every block
        {
            int blockLength = (int)Math.Sqrt(Length);
            List<int> numberAvailable = new List<int>();

            for (int i = 0; i < blockLength; i++)
            {
                for (int j = 0; j < blockLength; j++)
                {
                    // Stores the available numbers so 9 different random numbers will get chosen each block
                    for (int l = 1; l <= Length; l++)
                    {
                        numberAvailable.Add(l);
                    }

                    // Every fixed number gets removed from the available numbers
                    for (int y = i * blockLength; y < i * blockLength + blockLength; y++)
                    {
                        for (int x = j * blockLength; x < j * blockLength + blockLength; x++)
                        {
                            if (grid[y, x] != 0) 
                            {
                                numberAvailable.Remove(grid[y, x]);
                            }
                        }
                    }

                    // The rest of the blank spaces in this block get randomly filled with the available numbers
                    for (int y = i * blockLength; y < i * blockLength + blockLength; y++)
                    {
                        for (int x = j * blockLength; x < j * blockLength + blockLength; x++)
                        {
                            if (grid[y, x] == 0) 
                            {
                                int r = GetRandomNumber(0, numberAvailable.Count); // Get a random number from available numbers
                                grid[y, x] = numberAvailable[r];
                                numberAvailable.Remove(grid[y,x]); // The number is now unavailable in this block
                            }
                        }
                    }
                    numberAvailable.Clear();
                }
            }
        }

        public static int GetRandomNumber(int min, int max)
        {
            lock (getRandom)
            {
                return getRandom.Next(min, max);
            }
        }
    }
}
