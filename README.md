# Lottery Number Generator

This project is a C# application that generates lottery number combinations. It ensures that the generated combinations do not contain too many consecutive numbers, based on user input. The results can be written to a CSV file.

## Features

- Generates combinations of 6 regular numbers from a range of 1 to 37.
- Includes a power number from a range of 1 to 7.
- Allows the user to specify the maximum number of consecutive numbers in a combination.
- Uses parallel processing to speed up the generation of combinations.
- Outputs the results to a CSV file.

## Requirements

- .NET 8.0
- Visual Studio 2022

## Usage

1. Clone the repository or download the source code.
2. Open the solution in Visual Studio 2022.
3. Build the project to restore the necessary packages.
4. Run the application.

### Running the Application

When you run the application, you will be prompted to enter the maximum number of consecutive numbers to allow in a combination. Enter a number between 2 and 6.

The application will then generate valid lottery combinations and display the first 10 valid results. You will also be prompted to write the results to a CSV file.

### Example
Enter the maximum number of consecutive numbers to allow (between 2 and 6): 3
Total number of valid combinations: 123456
First 10 valid combinations (allowing up to 2 consecutive numbers):
1'st | 2'nd | 3'rd | 4'th | 5'th | 6'th | Power Number
1      2      3      4      5      6  |   1
1      2      3      4      5      6  |   2
... Write to file?(Y/N - Will end program): Y Results have been written to: LotteryResults.csv

## Project Structure

- `Program.cs`: Contains the main logic for generating lottery combinations, checking for consecutive numbers, and writing results to a CSV file.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue to discuss any changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
