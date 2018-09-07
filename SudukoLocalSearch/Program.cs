using System;
using System.Threading;

namespace SudukoLocalSearch
{
    class Program
    {
        private static readonly Random getRandom = new Random();

        static void Main()
        {
            int[,] input = ReadSudoku();

            long x = DateTime.Now.Ticks;

            Sudoku sudoku = new Sudoku(input);
            Console.WriteLine();
            Console.WriteLine("Randomly filled the blanks:");
            sudoku.WriteSudoku();
            Console.WriteLine("Evaluation: " + sudoku.GetEvaluationValue());

            Sudoku newsudoku = HillClimbing(sudoku);
            Console.WriteLine();
            Console.WriteLine("Solution:");
            newsudoku.WriteSudoku();
            Console.WriteLine("Evaluation: " + newsudoku.GetEvaluationValue());

            Console.WriteLine("Time Elapsed (in seconds): " + TimeSpan.FromTicks(DateTime.Now.Ticks - x).TotalSeconds);

            Console.ReadLine();
        }

        private static Sudoku HillClimbing(Sudoku sudoku)
        {
            int blockLength = (int)Math.Sqrt(sudoku.Length); // For a 9x9 sudoku the blocklength is 3
            int equalCounter = 0; // Counts how many times the swap resulted in an equal score

            // Parameters
            int progressTolerance = 100; // After this many steps no progress --> randomwalk
            int randomWalkSteps = 10; // Amount of randomwalk steps

            // Choose a block
            int blockColumn = GetRandomNumber(0, blockLength); // For a 9x9 sudoku, the randoms can be 0, 1 or 2 with blocklength = 3
            int blockRow = GetRandomNumber(0, blockLength);
            int bestValue = sudoku.GetEvaluationValue();

            Sudoku bestState = sudoku; // To store states before random-walking

            Sudoku newSudoku = GetBestSwap(sudoku, blockRow, blockColumn);

            while (newSudoku.GetEvaluationValue() != 0)
            {
                sudoku = newSudoku;

                blockRow = GetRandomNumber(0, blockLength);
                blockColumn = GetRandomNumber(0, blockLength);

                newSudoku = GetBestSwap(sudoku, blockRow, blockColumn); // Is always better than sudoku

                // No progress
                if (sudoku.GetEvaluationValue() == newSudoku.GetEvaluationValue())
                {
                    equalCounter++;
                    // If there is no progress for more steps then the tolerance or there is a local optimum
                    if (equalCounter > progressTolerance)
                    {
                        // Go back to the best state we had before random walking
                        if (bestValue < newSudoku.GetEvaluationValue())
                            newSudoku = bestState;

                        bestValue = newSudoku.GetEvaluationValue();
                        bestState = newSudoku;
                        newSudoku = RandomWalk(randomWalkSteps, newSudoku);
                        equalCounter = 0;
                    }
                }
            }
            return newSudoku;
        }

        private static Sudoku GetBestSwap(Sudoku sudoku, int blockRow, int blockColumn)
        // Finds the best possible swap (or if there is no improving swap it returns a clone of the input)
        {
            int blockLength = (int)Math.Sqrt(sudoku.Length); // For a 9x9 sudoku the blocklength is 3

            // Get the starting indices of the block
            int blockIndexColumn = blockColumn * blockLength;
            int blockIndexRow = blockRow * blockLength;

            int bestValue = sudoku.GetEvaluationValue(); // The best value thusfar is the current value
            Sudoku bestState = sudoku.Clone(); // Best state is the current state

            for (int row = blockIndexRow; row < blockIndexRow + blockLength; row++) // For every row in the randomly chosen block
            {
                for (int column = blockIndexColumn; column < blockIndexColumn + blockLength; column++) // And every column
                {
                    // Swap with all the other numbers in the block, not including the ones that already has been swapped.
                    // For a 9x9 sudoku there will be 45 possible swaps (if you also swap a number with itself)
                    for (int y = row; y < blockIndexRow + blockLength; y++)
                    {
                        // So that no number gets swapped with itself or a number that it has already been swapped with
                        int q = blockIndexColumn;
                        if (y == row)
                            q = column + 1;

                        for (int x = q; x < blockIndexColumn + blockLength; x++)
                        {
                            // Only swap non-fixed numbers
                            if (!sudoku.fixedNumbers.Contains(new Tuple<int, int>(row, column)) &&
                                !sudoku.fixedNumbers.Contains(new Tuple<int, int>(y, x)))
                            {
                                sudoku.Swap(row, column, y, x); // Swap two numbers
                                if (sudoku.GetEvaluationValue() <= bestValue) // If the swap resulted in a better (or equal) value
                                {
                                    Sudoku state = sudoku.Clone(); // Clone that sudoku (with the swap)
                                    bestValue = sudoku.GetEvaluationValue(); // The bestvalue is now this new value                                     
                                    bestState = state; // The best state is now this new sudoku
                                }
                                sudoku.Swap(row, column, y, x); // Swap back to not keep original sudoku intact
                            }
                        }
                    }
                }
            }
            return bestState;
        }

        private static Sudoku RandomWalk(int steps, Sudoku sudoku)
        {
            int blockLength = (int)Math.Sqrt(sudoku.Length);

            Sudoku newSudoku = sudoku.Clone();
            for (int i = 0; i < steps; i++)
            {
                // Get a random block
                int blockRow = GetRandomNumber(0, blockLength);
                int blockColumn = GetRandomNumber(0, blockLength);

                // Get 2 random numbers by index from that block
                int randomRow1 = GetRandomNumber(blockRow * blockLength, blockRow * blockLength + blockLength);
                int randomColumn1 = GetRandomNumber(blockColumn * blockLength, blockColumn * blockLength + blockLength);
                int randomRow2 = GetRandomNumber(blockRow * blockLength, blockRow * blockLength + blockLength);
                int randomColumn2 = GetRandomNumber(blockColumn * blockLength, blockColumn * blockLength + blockLength);

                // If the points are not fixed
                if (!newSudoku.fixedNumbers.Contains(new Tuple<int, int>(randomRow1, randomColumn1))
                    && !newSudoku.fixedNumbers.Contains(new Tuple<int, int>(randomRow2, randomColumn2)))
                {
                    // Swap them
                    newSudoku.Swap(randomRow1, randomColumn1, randomRow2, randomColumn2);
                }
                else
                {
                    // Because there is no swap, increase steps to make sure the amount of swaps will be equal to steps;
                    steps++;
                }
            }
            return newSudoku;
        }

        private static int[,] ReadSudoku()
        {
            try
            {
                // Get the length of the first row N, the sudoku size is N * N 
                char[] firstRowArray = Console.ReadLine().Trim().ToCharArray();
                
                int sudokuLength = firstRowArray.Length;
                int[,] sudoku = new int[sudokuLength, sudokuLength];
                string[] allRows = new string[sudokuLength];

                // Add the first row to the sudoku array
                for (int i = 0; i < firstRowArray.Length; i++)
                {
                    sudoku[0, i] = int.Parse(firstRowArray[i].ToString());
                }

                // Add the rest of the rows to the sudoku array
                for (int i = 1; i < sudokuLength; i++)
                {
                    string s = Console.ReadLine().Trim();
                    char[] rowCharArray = s.ToCharArray();

                    for (int j = 0; j < rowCharArray.Length; j++)
                    {
                        sudoku[i, j] = int.Parse(rowCharArray[j].ToString());
                    }
                }
                return sudoku;
            }
            catch (Exception e)
            {
                // Incorrect input
                Console.WriteLine(e.Message);
                Thread.Sleep(3000); // Wait for user to read error
                Environment.Exit(0); // Exit console
                return null;
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
