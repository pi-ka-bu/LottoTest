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