using System.Globalization;

namespace Task_2v
{
    public static class EmployeeUtils
    {
        // Defines a collection of valid field names for Employee objects that can be used for filtering or sorting operations.
        private static readonly string[] _fields = { "employee_id", "full_name", "age", "department", "is_manager", "salary", "projects" };

        /// <summary>
        /// Transforms a list of dictionaries into an array of Employee objects. Each dictionary in the list represents 
        /// an employee's data, with key-value pairs corresponding to employee attributes such as ID, name, age, etc.
        /// This method iterates through each dictionary, extracting and converting the data into Employee objects.
        /// </summary>
        /// <param name="data">A list of dictionaries, each representing the data for an individual employee.</param>
        /// <returns>An array of Employee objects created from the input list.</returns>
        /// <exception cref="Exception">Throws an exception if an error occurs during the data conversion process, indicating 
        /// that there might be a mismatch in expected data types or missing mandatory fields.</exception>
        public static List<JsonUtils.Employee> GetEmployeesCollection(List<Dictionary<string, object>> data)
        {
            List<JsonUtils.Employee> employees = new List<JsonUtils.Employee>();

            foreach (var obj in data)
            {
                try
                {
                    // Convert each key-value pair in the dictionary to the appropriate data type and assign to variables.
                    int employeeId = Convert.ToInt32(obj["employee_id"]);
                    string fullName = Convert.ToString(obj["full_name"]);
                    int age = Convert.ToInt32(obj["age"]);
                    string department = Convert.ToString(obj["department"]);
                    bool isManager = Convert.ToBoolean(obj["is_manager"]);
                    double salary = Convert.ToDouble(obj["salary"], CultureInfo.InvariantCulture);
                    string[] projects = ((List<string>)obj["projects"]).ToArray();

                    // Create a new Employee object with the extracted data.
                    var employee = new JsonUtils.Employee(employeeId, fullName, age, department, isManager, salary, projects);
                    // Add the new Employee object to the list.
                    employees.Add(employee); 
                }
                catch (Exception ex)
                {
                    // If any error occurs during data extraction or object creation, throw a new exception.
                    throw new Exception("Ошибка данных.");
                }
            }

            // Convert the list of Employee objects to an array and return.
            return employees; 
        }

        /// <summary>
        /// Filters an array of Employee objects based on a user-specified field and value. This method first validates the
        /// field name provided by the user, ensuring it is one of the recognized fields. Then, it prompts the user to input
        /// a specific value for the selected field. The method filters the array of employees, returning only those that
        /// match the specified criteria.
        /// </summary>
        /// <param name="employees">An array of Employee objects to be filtered.</param>
        /// <param name="selectValue">The field name to filter by, provided by the user.</param>
        /// <returns>An array of Employee objects that match the filtering criteria.</returns>
        /// <exception cref="ArgumentException">Thrown if the field name is empty or does not match any recognized fields.</exception>
        public static JsonUtils.Employee[] GenerateSelection(JsonUtils.Employee[] employees, string selectValue)
        {
            // Check if the selection value is empty or null, indicating an invalid input.
            if (string.IsNullOrEmpty(selectValue))
            {
                throw new ArgumentException("Передано пустое название поля.");
            }

            // Convert the field name to lowercase for case-insensitive comparison.
            string selectValueLower = selectValue.ToLower();

            // Verify that the provided field name exists within the predefined list of fields.
            if (!_fields.Contains(selectValueLower))
            {
                throw new ArgumentException("Передано неккоретное название поля.");
            }

            // Prompt the user for the value to filter by, with specific instructions based on the data type.
            string? userValue;
            while (true)
            {
                ConsoleUtils.PrintBeautyWarningMessage("Строковые значения необходимо вводить без кавычек\nЗначение поля \"projects\" необходимо вводить через запятую (пример ввода: Project A, Project B).");
                Console.Write($"Введите конкретное значение поля {selectValueLower} для организации фильтрации: ");
                userValue = Console.ReadLine();

                // Validate the user's input based on the field type.
                if (FieldValueCheck(selectValue, userValue))
                {
                    break; 
                }

                // If the input is invalid, display an error message and prompt again.
                ConsoleUtils.PrintBeautyError($"Неккоректное значение для поля {selectValueLower}, повторите ввод.");
            }

            // Filter the employees array based on the field value.
            List<JsonUtils.Employee> selectedEmployees = new List<JsonUtils.Employee>();
            foreach (JsonUtils.Employee employee in employees)
            {
                if (EqualityCheck(employee, selectValueLower, userValue))
                {
                    // Add the employee to the list if they match the criteria.
                    selectedEmployees.Add(employee); 
                }
            }

            // Return the filtered list of employees as an array.
            return selectedEmployees.ToArray();
        }

        /// <summary>
        /// Validates the user's input value for a specified field, ensuring it conforms to the expected data type and format.
        /// This method checks the value against the type required by the field (e.g., integer for 'employee_id' and 'age',
        /// boolean for 'is_manager', and double for 'salary'). For fields that do not have a specific data type requirement
        /// (e.g., string fields), it simply verifies that the input is not empty.
        /// </summary>
        /// <param name="selectValue">The field name for which the user value is being validated.</param>
        /// <param name="userValue">The value entered by the user for the specified field.</param>
        /// <returns>A boolean indicating whether the user's value is valid for the specified field.</returns>
        private static bool FieldValueCheck(string selectValue, string userValue)
        {
            bool status;

            // Check the userValue based on the field specified in selectValue.
            switch (selectValue)
            {
                case "employee_id" or "age":
                    // For 'employee_id' and 'age', the value must be convertible to an integer.
                    status = int.TryParse(userValue, out _);
                    break;
                case "is_manager":
                    // For 'is_manager', the value must be either "true" or "false".
                    status = userValue == "true" || userValue == "false";
                    break;
                case "salary":
                    // For 'salary', the value must be convertible to a double, considering culture-specific format.
                    status = double.TryParse(userValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
                    break;
                default:
                    // For any other field, the value should not be empty.
                    status = !string.IsNullOrEmpty(userValue);
                    break;
            }

            // Return the status indicating the validity of the userValue for the selectValue field.
            return status;
        }

        /// <summary>
        /// Compares a specified field value of an Employee object against a user-provided value to determine equality.
        /// This method supports various data types and structures, including integers, strings, booleans, and string arrays,
        /// enabling flexible and dynamic filtering based on different criteria. It ensures that the comparison is appropriate
        /// for the data type of each field (e.g., numeric comparison for IDs and ages, string comparison for names and departments,
        /// boolean comparison for managerial status, and array comparison for projects).
        /// </summary>
        /// <param name="employee">The Employee object whose field value is to be compared.</param>
        /// <param name="selectValue">The name of the field to be compared.</param>
        /// <param name="userValue">The value provided by the user for comparison.</param>
        /// <returns>A boolean indicating whether the employee's field value matches the user's provided value.</returns>
        private static bool EqualityCheck(JsonUtils.Employee employee, string selectValue, string userValue)
        {
            bool status = false;

            // Determine the field specified by selectValue and compare its value in the employee object with userValue.
            switch (selectValue)
            {
                case "employee_id":
                    // Compare integer values for employee ID.
                    status = employee.EmployeeId == int.Parse(userValue);
                    break;
                case "full_name":
                    // Direct string comparison for full name.
                    status = employee.FullName == userValue;
                    break;
                case "age":
                    // Compare integer values for age.
                    status = employee.Age == int.Parse(userValue);
                    break;
                case "department":
                    // Direct string comparison for department.
                    status = employee.Department == userValue;
                    break;
                case "is_manager":
                    // Boolean comparison for managerial status.
                    status = employee.IsManager == bool.Parse(userValue);
                    break;
                case "salary":
                    // Compare double values for salary, considering culture-specific formatting.
                    status = employee.Salary == double.Parse(userValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "projects":
                    // Compare string arrays for projects, ignoring case and considering order.
                    var userProjects = userValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(p => p.Trim())
                                        .ToArray();
                    status = employee.Projects.SequenceEqual(userProjects, StringComparer.OrdinalIgnoreCase);
                    break;
            }

            // Return the result of the comparison.
            return status; 
        }
    }
}
