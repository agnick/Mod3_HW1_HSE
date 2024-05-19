namespace Task_2v
{
    public static class ConsoleUtils
    {
        // Private static fields for storage file state.
        private static bool _fileOption = false;
        private static string _filePath = string.Empty;

        // Properties for reading and setting class fields.
        public static bool FileOption
        {
            set { _fileOption = value; }
            get { return _fileOption; }
        }
        public static string FilePath
        {
            set { _filePath = value; }
            get { return _filePath; }
        }

        /// <summary>
        /// A method that outputs a red - marked error.
        /// </summary>
        /// <param name="errorMessage">Transmitted error message.</param>
        public static void PrintBeautyError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// A method that outputs a success message highlighted in green.
        /// </summary>
        /// <param name="successMessage">The success message to be displayed.</param>
        public static void PrintBeautySuccessMessage(string successMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(successMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// A method that outputs a warning message highlighted in yellow.
        /// </summary>
        /// <param name="warningMessage">The warning message to be displayed.</param>
        public static void PrintBeautyWarningMessage(string warningMessage)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(warningMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// Reads the user's selection from a simple text-based options menu and validates the input.
        /// </summary>
        /// <returns>An integer representing the user's choice. This value is guaranteed to be either 1 or 2, corresponding to the available menu options.</returns>
        public static int ReadOptionsMenu()
        {
            int userChoice;
            // Keep asking until a valid input is received.
            while (true)
            {
                // Display the options to the user.
                Console.Write("1. Стандартный поток ввода-вывода\r\n2. Файловый поток ввода-вывода\r\n");
                Console.Write("Укажите номер пункта меню для запуска действия: ");

                // Try to parse the user input as an integer. If parsing is successful and the choice is within the valid range, exit the loop.
                if (int.TryParse(Console.ReadLine(), out userChoice) && userChoice > 0 && userChoice < 3)
                {
                    break;
                }

                // If the input is not valid, display an error message and prompt for input again.
                PrintBeautyError("Неккоректные данные, повторите ввод.");
            }

            return userChoice;
        }

        /// <summary>
        /// Prompts the user for an absolute file path and validates that the file exists.
        /// </summary>
        /// <returns>A string containing the absolute path to an existing file as specified by the user.</returns>
        public static string GetFilePathFromUser()
        {
            string? filePath;
            // Loop until a valid file path is provided.
            while (true)
            {
                // Ask the user to enter the absolute path to a file containing JSON data.
                Console.Write("Введите абсолютный путь к файлу с json-данными: ");
                filePath = Console.ReadLine();

                // Check if the file exists at the specified path.
                if (!File.Exists(filePath))
                {
                    // If the file does not exist, display an error message and ask for the input again.
                    PrintBeautyError("Файл с таким названием не существует. Повторите ввод.");
                    continue;
                }

                break;
            }

            return filePath;
        }

        /// <summary>
        /// Presents a menu for sorting and filtering an array of Employee objects based on user input.
        /// </summary>
        /// <param name="employees">An array of Employee objects to be sorted or filtered.</param>
        public static void SortAndFilterOptionsMenu(JsonUtils.Employee[] employees)
        {
            // Variable to store the user's menu selection.
            int n;
            while (true)
            {
                // Displaying options for filtering or sorting the Employee array.
                Console.Write("1. Произвести фильтрацию по вашему значению\r\n2. Произвести сортировку по вашему значению\r\n");
                Console.Write("Укажите номер пункта меню для запуска действия: ");

                // Validating the user input to ensure it's an integer within the expected range.
                if (int.TryParse(Console.ReadLine(), out n) && n > 0 && n < 3)
                {
                    break; // Exit loop if input is valid.
                }

                // If input is invalid, display an error message.
                PrintBeautyError("Неизвестная команда, повторите ввод.");
            }

            // Variable to hold the result of the filtering operation.
            JsonUtils.Employee[] selected;

            while (true)
            {
                switch (n)
                {
                    case 1: // Filtering option
                        try
                        {
                            // Prompting the user for the name of the field to filter by.
                            PrintBeautyWarningMessage("Названия полей необходимо вводить без кавычек.");
                            Console.Write("Введите кокретное название поля по которому необходимо сделать фильтрацию: ");
                            string? selectValue = Console.ReadLine();
                            // Performing the filtering operation based on the field name.
                            selected = EmployeeUtils.GenerateSelection(employees, selectValue);
                            // Proceeding with the selected filtering option.
                            ProceedSelection(selected);
                        }
                        catch (ArgumentException ex) // Catching and handling exceptions related to filtering.
                        {
                            PrintBeautyError(ex.Message);
                            continue; // Asking for input again in case of exception.
                        }
                        break;
                    case 2: // Sorting option
                        try
                        {
                            // Getting sorting preferences from the user.
                            (string selectValue, bool sortParam) = ChooseSortOptions();
                            // Setting the sorting options based on user input.
                            JsonUtils.Employee.SetSortOptions(selectValue, sortParam);
                            // Sorting the array of employees.
                            Array.Sort(employees);
                            // Notifying the user that sorting was successful.
                            PrintBeautySuccessMessage("Сортировка выполнена успешно!");
                            // Offering the user options for saving the sorted array.
                            ChooseSaveOptions(employees);
                        }
                        catch (Exception ex) // Catching and handling exceptions related to sorting.
                        {
                            PrintBeautyError(ex.Message);
                            continue; // Asking for input again in case of exception.
                        }
                        break;
                }
                break;
            }
        }

        /// <summary>
        /// Prompts the user to choose sorting options based on a specific field and order for sorting an array of objects.
        /// </summary>
        /// <returns>A tuple containing the name of the field to sort by and a boolean indicating the sorting order (true for ascending, false for descending).</returns>
        /// <exception cref="ArgumentException">Thrown if an empty field name is passed or if the field name is invalid.</exception>
        private static (string, bool) ChooseSortOptions()
        {
            // Prompt the user to enter the name of the field for sorting.
            Console.Write("Введите конкретное название поля по которому необходимо сделать сортировку: ");
            string? selectValue = Console.ReadLine();

            // Throw an exception if the field name is null or empty.
            if (string.IsNullOrEmpty(selectValue))
            {
                throw new ArgumentException("Передано пустое название поля.");
            }

            string valueToPass;
            // Match the user input with the corresponding field name in the object.
            switch (selectValue)
            {
                case "employee_id":
                    valueToPass = "EmployeeId";
                    break;
                case "full_name":
                    valueToPass = "FullName";
                    break;
                case "age":
                    valueToPass = "Age";
                    break;
                case "department":
                    valueToPass = "Department";
                    break;
                case "is_manager":
                    valueToPass = "IsManager";
                    break;
                case "salary":
                    valueToPass = "Salary";
                    break;
                case "projects":
                    valueToPass = "Projects";
                    break;
                default:
                    // If the field name doesn't match any known fields, throw an exception.
                    throw new ArgumentException("Передано неккоретное название поля.");
            }

            // Prompt the user to choose the sorting order.
            int sortParam;
            while (true)
            {
                // Display sorting options to the user.
                PrintBeautyWarningMessage("Строковые данные сортируются в алфавитном и обратном алфавитном порядке\nЧисловые данные сортируются в порядке возрастания и убывания\nМассивы сортируются по количеству элементов в порядке возрастания и убывания");
                Console.WriteLine("1. Прямая сортировка\r\n2. Обратная сортировка");
                Console.Write("Укажите номер пункта меню для запуска действия: ");

                // Validate the user's input for sorting order.
                if (int.TryParse(Console.ReadLine(), out sortParam) && sortParam > 0 && sortParam < 3)
                {
                    break; // Exit the loop if a valid option is chosen.
                }

                // Display an error message if an invalid option is entered.
                PrintBeautyError("Неизвестная команда, повторите ввод.");
            }

            // Return the selected field and sorting order as a tuple.
            return (valueToPass, sortParam == 1 ? true : false);
        }

        /// <summary>
        /// Processes the selection result after filtering the array of Employee objects. It checks if the result 
        /// contains any elements and then either proceeds with saving options or displays an error message if the result is empty.
        /// </summary>
        /// <param name="selected">The array of Employee objects resulting from the filter operation.</param>
        private static void ProceedSelection(JsonUtils.Employee[] selected)
        {
            // Check if the 'selected' array is not null and contains elements.
            if (selected is not null && selected.Length != 0)
            {
                // If the array contains elements, display a success message and prompt the user for save options.
                PrintBeautySuccessMessage("Фильтрация выполнена успешно!");
                // Proceed with options to save the filtered results.
                ChooseSaveOptions(selected); 
            }
            else
            {
                // If the array is empty, display an error message indicating no results were found.
                PrintBeautyError("Результат фильтрации пуст.");
            }
        }

        /// <summary>
        /// Presents the user with options to either display the processed employee data in the console or save it to a file. 
        /// The user's choice is then executed accordingly.
        /// </summary>
        /// <param name="employees">An array of Employee objects to be displayed or saved.</param>
        private static void ChooseSaveOptions(JsonUtils.Employee[] employees)
        {
            // Variable to store the user's menu choice.
            int n;
            while (true)
            {
                // Display the options for output: console or file.
                Console.Write("1. Вывести данные в консоль\r\n2. Сохранить данные в файл\r\n");
                Console.Write("Укажите номер пункта меню для запуска действия: ");

                // Validate the user input to ensure it's an integer within the expected range.
                if (int.TryParse(Console.ReadLine(), out n) && n > 0 && n < 3)
                {
                    break;
                }

                // Display an error message if the input is invalid.
                PrintBeautyError("Неизвестная команда, повторите ввод.");
            }

            // Execute the chosen option.
            if (n == 1)
            {
                // If the user chooses to display data in the console, show a success message and print the employee data.
                PrintBeautySuccessMessage("Полученные данные:");
                // Call a method to print employee details to the console.
                PrintEmployees(employees);
            }
            else
            {
                // If the user opts to save the data to a file, call a method to handle file saving options.
                ChooseFileSaveOptions(employees);
            }
        }

        /// <summary>
        /// Provides the user with options to either overwrite the existing file or specify a new file path for saving the employee data in JSON format.
        /// </summary>
        /// <param name="employees">An array of Employee objects to be saved to a file.</param>
        private static void ChooseFileSaveOptions(JsonUtils.Employee[] employees)
        {
            int n = 2;
            // Check if the user has the option to choose file operation mode.
            if (_fileOption)
            {
                while (true)
                {
                    // Prompt the user to choose between overwriting the existing file or specifying a new file path.
                    Console.Write("1. Перезаписать данные в файл, из которого были считаны данные\r\n2. Указать новый путь к файлу для записи\r\n");
                    Console.Write("Укажите номер пункта меню для запуска действия: ");
                    if (int.TryParse(Console.ReadLine(), out n) && n > 0 && n < 3)
                    {
                        break; 
                    }
                    // Display an error message if the input is invalid.
                    PrintBeautyError("Неизвестная команда, повторите ввод.");
                }
            }

            // Flag to indicate if the save operation was successful.
            bool success = false; 
            while (!success)
            {
                // Preserve the original console output.
                TextWriter originalConsoleOut = Console.Out; 

                try
                {
                    // Declare a StreamWriter for file writing.
                    StreamWriter sw = null; 

                    // Overwrite the existing file.
                    if (n == 1)
                    {
                        sw = new StreamWriter(_filePath, false);
                    }
                    else // Prompt the user for a new file path.
                    {
                        Console.Write($"Введите путь к новому файлу, в который необходимо записать данные (пример ввода: myFile.json или ...{Path.DirectorySeparatorChar}myFile.json): ");
                        string? newFilePath = Console.ReadLine();

                        // Validate the file name input.
                        if (string.IsNullOrEmpty(newFilePath))
                        {
                            PrintBeautyError("Передано пустое название файла.");
                            // Ask for input again.
                            continue; 
                        }

                        // Ensure the file name ends with ".json".
                        if (!newFilePath.EndsWith(".json"))
                        {
                            PrintBeautyError("Название файла должно заканчиться на \".json\"");
                            // Ask for input again.
                            continue; 
                        }

                        // Create or overwrite the new file.
                        sw = new StreamWriter(newFilePath, false); 
                    }

                    // Redirect console output to the StreamWriter.
                    using (sw)
                    {
                        Console.SetOut(sw);
                        JsonUtils.JsonParser.WriteJson("[");
                        foreach (JsonUtils.Employee employee in employees)
                        {
                            JsonUtils.JsonParser.WriteJson(employee.ToString());
                        }
                        JsonUtils.JsonParser.WriteJson("]");
                    }

                    // Restore the original console output.
                    Console.SetOut(originalConsoleOut);
                    // Display a success message.
                    PrintBeautySuccessMessage("Данные успешно записаны в файл!");
                    // Indicate successful operation completion.
                    success = true; 
                }
                catch (ArgumentException)
                {
                    // Handle specific exceptions with tailored error messages.
                    PrintBeautyError("Введено некорректное название для файла. Попробуйте снова.");
                }
                catch (IOException)
                {
                    PrintBeautyError("Возникла ошибка при открытии файла и записи данных. Попробуйте снова.");
                }
                catch (Exception ex)
                {
                    // Catch any unexpected exceptions.
                    PrintBeautyError($"Возникла непредвиденная ошибка: {ex.Message}. Попробуйте снова.");
                }
                finally
                {
                    // Ensure the original console output is always restored.
                    Console.SetOut(originalConsoleOut);
                }
            }
        }

        /// <summary>
        /// Prints the details of each employee in the given array to the console in a JSON-like format.
        /// </summary>
        /// <param name="employees">An array of Employee objects to be printed.</param>
        private static void PrintEmployees(JsonUtils.Employee[] employees)
        {
            // Begin the JSON-like array output.
            Console.WriteLine('[');

            // Iterate through each employee in the array and print their details.
            foreach (JsonUtils.Employee employee in employees)
            {
                // Print each employee using their overridden ToString method, which is expected to return a JSON-like string representation.
                Console.WriteLine(employee.ToString());
            }

            // End the JSON-like array output.
            Console.WriteLine(']');
        }
    }
}
