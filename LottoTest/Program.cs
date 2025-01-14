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