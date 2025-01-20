using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
    static List<(int[], int)> GenerateLotteryResultsChunk(int[][] regularCombinationsChunk, int maxConsecutive, List<(int[], int)> downloadedResults)
    {
        List<(int[], int)> results = new List<(int[], int)>();
        foreach (var regularComb in regularCombinationsChunk)
        {
            // Check if the combination has too many consecutive numbers (greater than or equal to `maxConsecutive`)
            if (HasTooManyConsecutiveNumbers(regularComb, maxConsecutive))
                continue;  // Skip if the combination has too many consecutive numbers

            foreach (var power in powerNumbers)
            {
                var combination = (regularComb, power);
                if (!downloadedResults.Any(dr => dr.Item1.SequenceEqual(combination.Item1) && dr.Item2 == combination.Item2))
                {
                    results.Add(combination); // Add valid result
                }
            }
        }
        return results;
    }
    static async Task<List<(int[], int)>> GenerateLotteryResults(int maxConsecutive, List<(int[], int)> downloadedResults, int numThreads = 8)
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

        var tasks = chunks.Select(chunk => Task.Run(() => GenerateLotteryResultsChunk(chunk, maxConsecutive, downloadedResults))).ToArray();

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

    // Function to download and process CSV file from the given URL
    static async Task<List<(int[], int)>> DownloadAndProcessCsv(string url, string tempFilePath)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var csvContent = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(tempFilePath, csvContent);

            var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var results = new List<(int[], int)>();

            // Skip the header line
            foreach (var line in lines.Skip(1))
            {
                var columns = line.Split(',');

                // Parse the regular numbers and power number
                var regularNumbers = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    regularNumbers[i] = int.Parse(columns[i + 2]);
                }
                int powerNumber = int.Parse(columns[8]);

                results.Add((regularNumbers, powerNumber));
            }

            return results;
        }
    }

    // Function to display a processing animation
    static void DisplayProcessingAnimation(CancellationToken token)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"   ___                 _               ");
        Console.WriteLine(@"  / _ \__ _____  ___  (_)__  ___ _     ");
        Console.WriteLine(@" / , _/ // / _ \/ _ \/ / _ \/ _ `/     ");
        Console.WriteLine(@"/_/|_|\_,_/_//_/_//_/_/_//_/\_, /      ");
        Console.Write(@"                           /___/");

        var spinner = new[] { '*', '*', '*', '*', '*', '*', '*', '*', '*', '*' };
        int counter = 0;

        while (!token.IsCancellationRequested)
        {
            if (counter <= 9)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(spinner[counter % spinner.Length]);
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                counter++;
                Thread.Sleep(100);
            }  
            else
            {
                counter = 0;
                Console.SetCursorPosition(Console.CursorLeft - 10, Console.CursorTop);
                Console.Write("          ");
                Console.SetCursorPosition(Console.CursorLeft - 10, Console.CursorTop);
            }
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n\n");
    }

    //Print logo
    static void PrintLogo()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\t\t\t    __          __  __                            ");
        Console.WriteLine("\t\t\t   / /   ____  / /_/ /_____  _______  __          ");
        Console.WriteLine("\t\t\t  / /   / __ \\/ __/ __/ __ \\/ ___/ / / /          ");
        Console.WriteLine("\t\t\t / /___/ /_/ / /_/ /_/ /_/ / /  / /_/ /           ");
        Console.WriteLine("\t\t\t/_____/\\____/\\__/\\__/\\____/_/   \\__, /            ");
        Console.WriteLine("\t\t\t   ______                      /____/_            ");
        Console.WriteLine("\t\t\t  / ____/__  ____  ___  _________ _/ /_____  _____");
        Console.WriteLine("\t\t\t / / __/ _ \\/ __ \\/ _ \\/ ___/ __ `/ __/ __ \\/ ___/");
        Console.WriteLine("\t\t\t/ /_/ /  __/ / / /  __/ /  / /_/ / /_/ /_/ / /    ");
        Console.WriteLine("\t\t\t\\____/\\___/_/ /_/\\___/_/   \\__,_/\\__/\\____/_/     \n\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    static async Task Main(string[] args)
    {
        // Print the logo
        PrintLogo();

        // Load the app settings and variables
        IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var defColor = ConsoleColor.White;
        Console.ForegroundColor = defColor;

        // Get user input for the maximum length of consecutive numbers to allow
        Console.Write("Enter the maximum number of consecutive numbers to allow (between 2 and 6): ");
        int maxConsecutive = int.Parse(Console.ReadLine());

        if (maxConsecutive < 2 || maxConsecutive > 6)
        {
            Console.WriteLine("Invalid input! Please enter a number between 2 and 6.");
            return;
        }

        // Define the URL and temporary file path
        string csvUrl = Configuration.GetValue<string>("DownloadCsv");
        string tempFilePath = Path.GetTempFileName();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n\tTemporary file {tempFilePath}, will be deleted in the end.\n");


        // Create a cancellation token source for the processing animation
        var cts = new CancellationTokenSource();

        // Start the processing animation
        var animationThread = new Thread(() => DisplayProcessingAnimation(cts.Token));
        animationThread.Start();

        // Download and process the CSV file

        var downloadedResults = await DownloadAndProcessCsv(csvUrl, tempFilePath);


        // Generate valid lottery results
        var lotteryResults = await GenerateLotteryResults(maxConsecutive, downloadedResults);

        // Stop the processing animation
        cts.Cancel();
        animationThread.Join();

        // Delete the temporary file
        File.Delete(tempFilePath);
        Console.ForegroundColor = defColor;

        // Debugging: Print the number of valid results
        Console.WriteLine($"Total number of valid combinations: {lotteryResults.Count}\n");

        // Example: Print the first 10 valid results (just for testing)
        Console.WriteLine($"First 10 valid combinations (allowing up to {maxConsecutive - 1} consecutive numbers):\n");
        Console.WriteLine("1'st | 2'nd | 3'rd | 4'th | 5'th | 6'th | Power Number\n------------------------------------------------------");

        for (int i = 0; i < Math.Min(10, lotteryResults.Count); i++)
        {
            var result = lotteryResults[i];
            Console.WriteLine("\x1b[92m {0,-4}\x1b[39m| \x1b[92m {1,-4}\u001b[39m| \x1b[92m {2,-4}\u001b[39m| \x1b[92m {3,-4}\u001b[39m| \x1b[92m {4,-4}\u001b[39m| \x1b[92m {5,-4}\u001b[39m|{7,-6}\x1b[93m{6}",
                result.Item1[0], result.Item1[1], result.Item1[2], result.Item1[3], result.Item1[4], result.Item1[5], result.Item2, "");
        }
        Console.ForegroundColor = defColor;

        // Randomly select combinations
        Console.WriteLine("How many random combinations would you like to print on screen?");
        int randomCombination = int.Parse(Console.ReadLine());
        while (randomCombination >= 0)
        {
            if (randomCombination > 0 && randomCombination <= lotteryResults.Count)
            {
                Random rnd = new Random();
                var randomResults = lotteryResults.OrderBy(x => rnd.Next()).Take(randomCombination).ToList();

                Console.WriteLine($"\nRandomly selected {randomCombination} combinations:\n");
                Console.WriteLine("1'st | 2'nd | 3'rd | 4'th | 5'th | 6'th | Power Number\n------------------------------------------------------");
                foreach (var result in randomResults)
                {
                    Console.WriteLine("\x1b[92m {0,-4}\x1b[39m| \x1b[92m {1,-4}\u001b[39m| \x1b[92m {2,-4}\u001b[39m| \x1b[92m {3,-4}\u001b[39m| \x1b[92m {4,-4}\u001b[39m| \x1b[92m {5,-4}\u001b[39m|{7,-6}\x1b[93m{6}", result.Item1[0], result.Item1[1], result.Item1[2], result.Item1[3], result.Item1[4], result.Item1[5], result.Item2, "");
                    Console.ForegroundColor = defColor;
                }
            }
            else
            {
                Console.WriteLine("Invalid input! Please enter a number between 1 and the total number of valid combinations.");
            }
            Console.WriteLine("Would you like to print more?(0 = No)");
            randomCombination = int.Parse(Console.ReadLine());
        }
        // Define the path to save the CSV file
        string filePath = "LotteryResults.csv";

        // Write the results to the CSV file
        Console.Write("\nWrite to file?(Y/N - Will end program): ");
        bool fileWrite = Console.ReadLine().ToLower() == "y" ? true : false;
        if (fileWrite)
        {
            await WriteResultsToCsv(lotteryResults, filePath);
        }


        Console.WriteLine($"Results have been written to: {filePath}");
    }
}