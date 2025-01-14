using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class LotteryGenerator
{
    // Define the range of regular numbers and power numbers
    static int[] regularNumbers = Enumerable.Range(1, 37).ToArray();  // Numbers from 1 to 37
    static int[] powerNumbers = Enumerable.Range(1, 7).ToArray();      // Numbers from 1 to 7

    // Check if a combination contains too many consecutive numbers (maxConsecutive or more consecutive numbers)
    static bool HasTooManyConsecutiveNumbers(int[] regularComb, int maxConsecutive)
    {
        Array.Sort(regularComb);

        for (int i = 0; i < regularComb.Length - 1; i++)
        {
            int consecutiveCount = 1;

            for (int j = i + 1; j < regularComb.Length; j++)
            {
                if (regularComb[j] - regularComb[i] <= maxConsecutive - 1)
                {
                    consecutiveCount++;
                }
                else
                {
                    break;
                }

                if (consecutiveCount >= maxConsecutive)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Function to get combinations of 'k' elements from 'source'
    static IEnumerable<int[]> GetCombinations(int[] source, int k)
    {
        int n = source.Length;
        if (k > n)
            yield break;

        int[] indices = new int[k];
        for (int i = 0; i < k; i++)
            indices[i] = i;

        while (true)
        {
            yield return indices.Select(i => source[i]).ToArray();

            int j = k - 1;
            while (j >= 0 && indices[j] == j + n - k)
                j--;

            if (j < 0)
                yield break;

            indices[j]++;
            for (int i = j + 1; i < k; i++)
                indices[i] = indices[i - 1] + 1;
        }
    }

    // Helper function to generate valid lottery results from a chunk of regular combinations
    static List<(int[], int)> GenerateLotteryResultsChunk(int[][] regularCombinationsChunk, int maxConsecutive)
    {
        List<(int[], int)> results = new List<(int[], int)>();
        foreach (var regularComb in regularCombinationsChunk)
        {
            // Check if the combination has too many consecutive numbers (greater than or equal to `maxConsecutive`)
            if (HasTooManyConsecutiveNumbers(regularComb, maxConsecutive))
                continue;  // Skip if the combination has too many consecutive numbers

            foreach (var power in powerNumbers)
            {
                results.Add((regularComb, power)); // Add valid result
            }
        }
        return results;
    }

    // Function to generate lottery results using parallel processing
    static async Task<List<(int[], int)>> GenerateLotteryResults(int maxConsecutive, int numThreads = 8)
    {
        // Generate all combinations of 6 regular numbers out of 37
        var regularCombinations = GetCombinations(regularNumbers, 6).ToArray();

        // Split the combinations into chunks for parallel processing
        int chunkSize = regularCombinations.Length / numThreads;
        var chunks = new List<int[][]>();

        for (int i = 0; i < numThreads; i++)
        {
            chunks.Add(regularCombinations.Skip(i * chunkSize).Take(chunkSize).ToArray());
        }

        var tasks = chunks.Select(chunk => Task.Run(() => GenerateLotteryResultsChunk(chunk, maxConsecutive))).ToArray();

        // Await all tasks and combine the results
        var results = await Task.WhenAll(tasks);

        // Flatten the list of results
        return results.SelectMany(r => r).ToList();
    }

    // Function to write the results to a CSV file
    static async Task WriteResultsToCsv(List<(int[], int)> results, string filePath)
    {
        // Ensure there are results to write
        if (results.Count == 0)
        {
            Console.WriteLine("No valid results to write to the CSV file.");
            return;
        }

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write header row
            await writer.WriteLineAsync("1st,2nd,3rd,4th,5th,6th,Power Number");

            // Write each result as a CSV line
            foreach (var result in results)
            {
                string regularNumbersCsv = string.Join(", ", result.Item1);
                await writer.WriteLineAsync($"{regularNumbersCsv},{result.Item2}");
            }
        }
        Console.WriteLine($"Results have been written to: {filePath}");
    }

    static async Task Main(string[] args)
    {
        // Get user input for the maximum length of consecutive numbers to allow
        Console.Write("Enter the maximum number of consecutive numbers to allow (between 2 and 6): ");
        int maxConsecutive = int.Parse(Console.ReadLine());

        if (maxConsecutive < 2 || maxConsecutive > 6)
        {
            Console.WriteLine("Invalid input! Please enter a number between 2 and 6.");
            return;
        }

        // Generate valid lottery results
        var lotteryResults = await GenerateLotteryResults(maxConsecutive);

        // Debugging: Print the number of valid results
        Console.WriteLine($"Total number of valid combinations: {lotteryResults.Count}");

        // Example: Print the first 10 valid results (just for testing)
        Console.WriteLine($"First 10 valid combinations (allowing up to {maxConsecutive - 1} consecutive numbers):");
        Console.WriteLine("1'st | 2'nd | 3'rd | 4'th | 5'th | 6'th | Power Number\n-------------------------------------------------------");
        var defColor = Console.ForegroundColor;
        for (int i = 0; i < Math.Min(10, lotteryResults.Count); i++)
        {
            var result = lotteryResults[i];
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" {string.Join("      ", result.Item1)}  ");
            Console.ForegroundColor = defColor;
            Console.Write("|   ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"   {result.Item2}   \n");
        }
        Console.ForegroundColor = defColor;
        // Define the path to save the CSV file
        string filePath = "LotteryResults.csv";

        // Write the results to the CSV file
        Console.Write("Write to file?(Y/N - Will end program): ");
        bool fileWrite = Console.ReadLine().ToLower()=="y"?true:false;
        if (fileWrite)
        {
            await WriteResultsToCsv(lotteryResults, filePath);
        }
        

        Console.WriteLine($"Results have been written to: {filePath}");
    }
}
//public class TablePrinter
//{
//    const string TOP_LEFT_JOINT = "┌";
//    const string TOP_RIGHT_JOINT = "┐";
//    const string BOTTOM_LEFT_JOINT = "└";
//    const string BOTTOM_RIGHT_JOINT = "┘";
//    const string TOP_JOINT = "┬";
//    const string BOTTOM_JOINT = "┴";
//    const string LEFT_JOINT = "├";
//    const string JOINT = "┼";
//    const string RIGHT_JOINT = "┤";
//    const char HORIZONTAL_LINE = '─';
//    const char PADDING = ' ';
//    const string VERTICAL_LINE = "│";

//    private static int[] GetMaxCellWidths(List<string[]> table)
//    {
//        int maximumCells = 0;
//        foreach (Array row in table)
//        {
//            if (row.Length > maximumCells)
//                maximumCells = row.Length;
//        }

//        int[] maximumCellWidths = new int[maximumCells];
//        for (int i = 0; i < maximumCellWidths.Length; i++)
//            maximumCellWidths[i] = 0;

//        foreach (Array row in table)
//        {
//            for (int i = 0; i < row.Length; i++)
//            {
//                if (row.GetValue(i).ToString().Length > maximumCellWidths[i])
//                    maximumCellWidths[i] = row.GetValue(i).ToString().Length + 2;
//            }
//        }

//        return maximumCellWidths;
//    }

//    public static string GetDataInTableFormat(List<string[]> table)
//    {
//        StringBuilder formattedTable = new StringBuilder();
//        Array nextRow = table.FirstOrDefault();
//        Array previousRow = table.FirstOrDefault();

//        if (table == null || nextRow == null)
//            return String.Empty;

//        // FIRST LINE:
//        int[] maximumCellWidths = GetMaxCellWidths(table);
//        for (int i = 0; i < nextRow.Length; i++)
//        {
//            if (i == 0 && i == nextRow.Length - 1)
//                formattedTable.Append(String.Format("{0}{1}{2}", TOP_LEFT_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE), TOP_RIGHT_JOINT));
//            else if (i == 0)
//                formattedTable.Append(String.Format("{0}{1}", TOP_LEFT_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE)));
//            else if (i == nextRow.Length - 1)
//                formattedTable.AppendLine(String.Format("{0}{1}{2}", TOP_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE), TOP_RIGHT_JOINT));
//            else
//                formattedTable.Append(String.Format("{0}{1}", TOP_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE)));
//        }

//        int rowIndex = 0;
//        int lastRowIndex = table.Count - 1;
//        foreach (Array thisRow in table)
//        {
//            // LINE WITH VALUES:
//            int cellIndex = 0;
//            int lastCellIndex = thisRow.Length - 1;
//            foreach (object thisCell in thisRow)
//            {
//                string thisValue = String.Format("{0}{1}{2}", String.Empty.PadLeft((maximumCellWidths[cellIndex] - thisCell.ToString().Length) / 2), thisCell.ToString(), String.Empty.PadRight(((maximumCellWidths[cellIndex] - thisCell.ToString().Length + 1) / 2)));

//                if (cellIndex == 0 && cellIndex == lastCellIndex)
//                    formattedTable.AppendLine(String.Format("{0}{1}{2}", VERTICAL_LINE, thisValue, VERTICAL_LINE));
//                else if (cellIndex == 0)
//                    formattedTable.Append(String.Format("{0}{1}", VERTICAL_LINE, thisValue));
//                else if (cellIndex == lastCellIndex)
//                    formattedTable.AppendLine(String.Format("{0}{1}{2}", VERTICAL_LINE, thisValue, VERTICAL_LINE));
//                else
//                    formattedTable.Append(String.Format("{0}{1}", VERTICAL_LINE, thisValue));

//                cellIndex++;
//            }

//            previousRow = thisRow;

//            // SEPARATING LINE:
//            if (rowIndex != lastRowIndex)
//            {
//                nextRow = table[rowIndex + 1];

//                int maximumCells = Math.Max(previousRow.Length, nextRow.Length);
//                for (int i = 0; i < maximumCells; i++)
//                {
//                    if (i == 0 && i == maximumCells - 1)
//                    {
//                        formattedTable.Append(String.Format("{0}{1}{2}", LEFT_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE), RIGHT_JOINT));
//                    }
//                    else if (i == 0)
//                    {
//                        formattedTable.Append(String.Format("{0}{1}", LEFT_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE)));
//                    }
//                    else if (i == maximumCells - 1)
//                    {
//                        if (i > previousRow.Length)
//                            formattedTable.AppendLine(String.Format("{0}{1}{2}", TOP_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE), TOP_RIGHT_JOINT));
//                        else if (i > nextRow.Length)
//                            formattedTable.AppendLine(String.Format("{0}{1}{2}", BOTTOM_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE), BOTTOM_RIGHT_JOINT));
//                        else if (i > previousRow.Length - 1)
//                            formattedTable.AppendLine(String.Format("{0}{1}{2}", JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE), TOP_RIGHT_JOINT));
//                        else if (i > nextRow.Length - 1)
//                            formattedTable.AppendLine(String.Format("{0}{1}{2}", JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight((maximumCellWidths[i]), HORIZONTAL_LINE), BOTTOM_RIGHT_JOINT));
//                        else
//                            formattedTable.AppendLine(String.Format("{0}{1}{2}", JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight((maximumCellWidths[i]), HORIZONTAL_LINE), RIGHT_JOINT));
//                    }
//                    else
//                    {
//                        if (i > previousRow.Length)
//                            formattedTable.Append(String.Format("{0}{1}", TOP_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE)));
//                        else if (i > nextRow.Length)
//                            formattedTable.Append(String.Format("{0}{1}", BOTTOM_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE)));
//                        else
//                            formattedTable.Append(String.Format("{0}{1}", JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE).PadRight(maximumCellWidths[i], HORIZONTAL_LINE)));
//                    }
//                }
//            }

//            rowIndex++;
//        }

//        // LAST LINE:
//        for (int i = 0; i < previousRow.Length; i++)
//        {
//            if (i == 0)
//                formattedTable.Append(String.Format("{0}{1}", BOTTOM_LEFT_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE)));
//            else if (i == previousRow.Length - 1)
//                formattedTable.AppendLine(String.Format("{0}{1}{2}", BOTTOM_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE), BOTTOM_RIGHT_JOINT));
//            else
//                formattedTable.Append(String.Format("{0}{1}", BOTTOM_JOINT, String.Empty.PadLeft(maximumCellWidths[i], HORIZONTAL_LINE)));
//        }

//        return formattedTable.ToString();
//    }
//}