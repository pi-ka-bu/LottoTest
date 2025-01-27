# Lottery Number Generator

This project is a C# application that generates lottery number combinations. It ensures that the generated combinations do not contain too many consecutive numbers, based on user input. The results can be written to a CSV file and displayed on the console.

## Features

- Generates combinations of regular numbers from a specified range.
- Includes a power number from a specified range.
- Allows the user to specify the maximum number of consecutive numbers in a combination.
- Uses parallel processing to speed up the generation of combinations.
- Filters out combinations that have already appeared in a downloaded CSV file.
- Outputs the results to a CSV file.
- Displays a processing animation while generating combinations.
- Allows the user to print a specified number of random combinations on the console.

## Requirements

- .NET 8.0
- Visual Studio 2022

## Configuration

The application uses an `appsettings.json` file to configure various settings. Create an `appsettings.json` file in the project directory with the following content:


## Usage

1. Clone the repository or download the source code.
2. Open the solution in Visual Studio 2022.
3. Build the project to restore the necessary packages.
4. Run the application.

### Running the Application

When you run the application, you will be prompted to enter the maximum number of consecutive numbers to allow in a combination. Enter a number between 2 and the value specified in `RegularCombinations` in the `appsettings.json` file.

The application will then download a CSV file from the specified URL, process it, and generate valid lottery combinations. You will be prompted to enter the number of random combinations you would like to print on the console. The results will be displayed in a table format with even spaces.

You will also be prompted to write the results to a CSV file.

### Example

```
Enter the maximum number of consecutive numbers to allow (between 2 and 6): 3 
Temporary file C:\Users\Username\AppData\Local\Temp\tmp1234.tmp, will be deleted in the end. 
Total number of valid combinations: 123456
How many random combinations would you like to print on screen? 5
Randomly selected 5 combinations:
1'st | 2'nd | 3'rd | 4'th | 5'th | 6'th | Power Number
1    |  2   |  3   |  4   |  5   |  6   |  1 7    |  8   |  9   | 10   | 11   | 12   |  2 ... 
Would you like to print more?(-1 = No) -1
Write to file?(Y/N - Will end program): Y 
Results have been written to: LotteryResults.csv
```

## Project Structure

- `Program.cs`: Contains the main logic for generating lottery combinations, checking for consecutive numbers, downloading and processing the CSV file, and writing results to a CSV file.
- `appsettings.json`: Configuration file for the application settings.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue to discuss any changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
