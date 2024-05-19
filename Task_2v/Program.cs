/*
   Студент:     Агафонов Никита Максимович    
   Группа:      БПИ234
   Вариант:     2
   Дата:        23.01.2024
*/
namespace Task_2v
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Preserving the original state of console input and output.
            var originalInput = Console.In;
            var originalOutput = Console.Out;

            while (true)
            {
                // Restoring the initial state of console input and output for correct work with data.
                Console.SetIn(originalInput);
                Console.SetOut(originalOutput);

                Console.Clear();

                // Initializing a reference to an array (List) of dictionaries for convenient data storage.
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                // Checking for exceptions.
                try
                {
                    // Calling a method that returns the user's choice for reading json format data.
                    int userChoice = ConsoleUtils.ReadOptionsMenu();

                    // Reading data from the console or file depending on the user's choice.
                    if (userChoice == 1)
                    {
                        Console.WriteLine("Введите или вставьте данные в формате JSON в консоль, для завершении ввода введите пустую строку");
                        ConsoleUtils.PrintBeautyWarningMessage("Данные должны начинаться с символа \"[\" и заканчиваться символом \"]\"");

                        // Using a normal input stream and calling a method that reads json format data and returns the processed data.
                        data = JsonUtils.JsonParser.ReadJson();

                        ConsoleUtils.PrintBeautySuccessMessage("Данные успешно считаны с консоли!");
                    }
                    else
                    {
                        // Getting the path to a file received from the user.
                        string? filePath = ConsoleUtils.GetFilePathFromUser();
                        // Assigning values ​​to the File Option and FilePath fields of a class to store state.
                        ConsoleUtils.FileOption = true;
                        ConsoleUtils.FilePath = filePath;
                        // Using an overridden input stream to read data from a file.
                        using (StreamReader sr = new StreamReader(filePath))
                        {
                            Console.SetIn(sr);
                            // Calling a method that reads json format data and returns the processed data.
                            data = JsonUtils.JsonParser.ReadJson();
                        }

                        ConsoleUtils.PrintBeautySuccessMessage("Данные успешно считаны из файла!");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUtils.PrintBeautyError(ex.Message);
                }
                finally {
                    // Restoring the original input stream, regardless of its changes, for normal console work.
                    Console.SetIn(originalInput); 
                }

                // Сhecking that the returned data is not null and not empty.
                if (data is not null && data.Count != 0)
                {
                    // Checking for exceptions.
                    try
                    {
                        // Calling a method that returns a collection of class instances and passing a reference to it.
                        List<JsonUtils.Employee> employees = EmployeeUtils.GetEmployeesCollection(data);
                        // Calling a method that gives the user the ability to select a sort or filter.
                        ConsoleUtils.SortAndFilterOptionsMenu(employees.ToArray());
                    }
                    catch (Exception ex)
                    {
                        ConsoleUtils.PrintBeautyError(ex.Message);
                    }
                }

                // Repeating the solution at the user's request.
                Console.Write("Для выхода из программы нажмите клавишу ESC, для перезапуска программы нажмите любую другую клавишу: ");
                if (Console.ReadKey(true).Key == ConsoleKey.Escape) { break; }
            }
        }
    }
}