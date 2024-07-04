using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

class Program
{
    static MySqlConnection dbConnection;

    static void Main()
    {
        // Establishing database connection
        dbConnection = InitializeDbConnection();

        int userChoice;
        do
        {
            // Displaying the main menu with colors
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\n***** Welcome to the Population Report System *****");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("1. View Countries by Population");
            Console.WriteLine("2. View Cities by Population");
            Console.WriteLine("3. View Top N Populated Cities");
            Console.WriteLine("4. View Top N Populated Capital Cities");
            Console.WriteLine("5. Generate Language Speakers Statistics");
            Console.WriteLine("6. Generate Population Breakdown Report");
            Console.WriteLine("7. View Capital Cities by Population");
            Console.WriteLine("8. View Top N Populated Countries");
            Console.WriteLine("9. Exit");
            Console.ResetColor();
            Console.Write("Please select an option: ");
            userChoice = Convert.ToInt32(Console.ReadLine());

            // Handling user choice with switch-case
            switch (userChoice)
            {
                case 1:
                    DisplayCountriesByPopulation();
                    break;
                case 2:
                    DisplayCitiesByPopulation();
                    break;
                case 3:
                    DisplayCapitalCitiesByPopulation();
                    break;
                case 4:
                    Console.Write("Enter the number of top countries to display: ");
                    int countryCount = Convert.ToInt32(Console.ReadLine());
                    DisplayTopNPopulatedCountries(countryCount);
                    break;
                case 5:
                    Console.Write("Enter the number of top cities to display: ");
                    int cityCount = Convert.ToInt32(Console.ReadLine());
                    DisplayTopNPopulatedCities(cityCount);
                    break;
                case 6:
                    Console.Write("Enter the number of top capital cities to display: ");
                    int capitalCityCount = Convert.ToInt32(Console.ReadLine());
                    DisplayTopNPopulatedCapitalCities(capitalCityCount);
                    break;
                case 7:
                    GeneratePopulationBreakdownReport();
                    break;
                case 8:
                    GenerateLanguageSpeakersStatistics();
                    break;
                case 9:
                    Console.WriteLine("Thank you for using the system. Goodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid choice! Please select a valid option.");
                    break;
            }
        } while (userChoice != 9);

        // Closing the database connection
        dbConnection.Close();
    }

    // Method to establish a connection to the database
    static MySqlConnection InitializeDbConnection()
    {
        string connStr = "Server=localhost;Database=world;Uid=root;Pwd=1234;";
        MySqlConnection conn = new MySqlConnection(connStr);

        try
        {
            conn.Open();
            Console.WriteLine("Database connection established successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Unable to connect to the database. Exiting... {ex.Message}");
            Environment.Exit(1);
        }

        return conn;
    }

    // Method to display countries by population in descending order
    static void DisplayCountriesByPopulation()
    {
        string sqlQuery = "SELECT Name, Population FROM country ORDER BY Population DESC";
        ExecuteAndDisplayQuery(sqlQuery);
    }

    // Method to display cities by population in descending order
    static void DisplayCitiesByPopulation()
    {
        string sqlQuery = "SELECT Name, Population FROM city ORDER BY Population DESC";
        ExecuteAndDisplayQuery(sqlQuery);
    }

    // Method to display capital cities by population in descending order
    static void DisplayCapitalCitiesByPopulation()
    {
        string sqlQuery = "SELECT city.Name, city.Population FROM city JOIN country ON city.ID = country.Capital ORDER BY city.Population DESC";
        ExecuteAndDisplayQuery(sqlQuery);
    }

    // Method to display top N populated countries
    static void DisplayTopNPopulatedCountries(int n)
    {
        string sqlQuery = $"SELECT Name, Population FROM country ORDER BY Population DESC LIMIT {n}";
        ExecuteAndDisplayQuery(sqlQuery);
    }

    // Method to display top N populated cities
    static void DisplayTopNPopulatedCities(int n)
    {
        string sqlQuery = $"SELECT Name, Population FROM city ORDER BY Population DESC LIMIT {n}";
        ExecuteAndDisplayQuery(sqlQuery);
    }

    // Method to display top N populated capital cities
    static void DisplayTopNPopulatedCapitalCities(int n)
    {
        string sqlQuery = $"SELECT city.Name, city.Population FROM city JOIN country ON city.ID = country.Capital ORDER BY city.Population DESC LIMIT {n}";
        ExecuteAndDisplayQuery(sqlQuery);
    }

    // Method to generate population breakdown report by continent
    static void GeneratePopulationBreakdownReport()
    {
        string sqlQuery = "SELECT Continent, SUM(Population) AS TotalPopulation FROM country GROUP BY Continent";
        ExecuteAndDisplayQuery(sqlQuery);
    }

    // Method to generate language speakers statistics
    static void GenerateLanguageSpeakersStatistics()
    {
        // Dictionary to store language populations
        var languagePopulations = new Dictionary<string, int>
        {
            { "Chinese", 0 },
            { "English", 0 },
            { "Hindi", 0 },
            { "Spanish", 0 },
            { "Arabic", 0 }
        };

        // Query to fetch language populations from the database
        string sqlQuery = "SELECT Language, SUM(Population) AS TotalSpeakers FROM countrylanguage GROUP BY Language";
        MySqlCommand cmd = new MySqlCommand(sqlQuery, dbConnection);

        try
        {
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string language = reader.GetString("Language");
                        int totalSpeakers = reader.GetInt32("TotalSpeakers");

                        if (languagePopulations.ContainsKey(language))
                        {
                            languagePopulations[language] = totalSpeakers;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No language data found.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving language data: {ex.Message}");
            return;
        }

        // Displaying the language speakers statistics
        Console.WriteLine("Language Speakers Statistics:");
        foreach (var pair in languagePopulations.OrderByDescending(pair => pair.Value))
        {
            double percentage = (double)pair.Value / GetTotalWorldPopulation() * 100;
            Console.WriteLine($"{pair.Key}: {pair.Value} (Percentage: {percentage:F2}%)");
        }
    }

    // Method to execute a query and display the results
    static void ExecuteAndDisplayQuery(string query)
    {
        MySqlCommand cmd = new MySqlCommand(query, dbConnection);
        MySqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Console.WriteLine($"{reader[0]}: {reader[1]}");
        }

        reader.Close();
    }

    // Method to get the total world population
    static int GetTotalWorldPopulation()
    {
        string query = "SELECT SUM(Population) FROM country";
        MySqlCommand cmd = new MySqlCommand(query, dbConnection);
        object result = cmd.ExecuteScalar();

        if (result != null && result != DBNull.Value)
        {
            return Convert.ToInt32(result);
        }

        return 0;
    }
}
