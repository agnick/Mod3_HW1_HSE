using System.Globalization;
using System.Text.RegularExpressions;

namespace JsonUtils
{
    public static class JsonParser
    {
        // States for state machine.
        enum State
        {
            OutsideObject,
            InsideObject,
            InsideString,
            InsideArray
        }

        /// <summary>
        /// Reads a JSON structure from the console input, line by line, and converts it into a list of dictionaries,
        /// each representing a JSON object. The method expects the JSON data to start with a '[' and end with a ']',
        /// with each object formatted as a dictionary in between. This method validates the basic structure of the JSON
        /// input and utilizes a state machine for parsing individual lines into dictionary objects.
        /// </summary>
        /// <returns>A list of dictionaries, where each dictionary represents a single JSON object from the input.</returns>
        /// <exception cref="Exception">Throws an exception if there is an error in reading the data, 
        /// such as incorrect formatting or structural issues with the input JSON.</exception>
        public static List<Dictionary<string, object>> ReadJson()
        {
            List<Dictionary<string, object>> result;

            try
            {
                // Initialize a list to hold each line of input.
                List<string> lines = new List<string>();
                string? line;
                // Continuously read lines from the console until an empty line is encountered.
                while (true)
                {
                    line = Console.ReadLine();
                    // Check if the line is empty.
                    if (string.IsNullOrEmpty(line)) 
                    {
                        break; 
                    }
                    // Add the non-empty line to the list of lines.
                    lines.Add(line); 
                }

                // Validate the start and end of the JSON structure.
                if (lines.Count == 0 || lines[0].Trim() != "[" || lines[^1].Trim() != "]")
                {
                    // Throw exception if the structure is incorrect.
                    throw new Exception(); 
                }

                // Process the lines into dictionaries using a custom state machine parser.
                result = StateMachine(lines.ToArray());
            }
            catch (Exception ex)
            {
                // Rethrow a new exception if any error occurs during processing.
                throw new Exception("Возникла ошибка при чтении данных.");
            }

            // Validate the structure of the parsed data.
            CheckDataStructure(result);

            // Return the list of dictionary objects parsed from the input.
            return result; 
        }

        /// <summary>
        /// Writes a JSON string to the console or to the file. This method ensures that the JSON data being written is not null or empty,
        /// throwing an ArgumentException if this validation fails. It wraps the console output operation within a try-catch block
        /// to handle potential IOExceptions that could occur during the write operation, such as issues with the console stream.
        /// </summary>
        /// <param name="dataJson">The JSON string to be written to the console or file.</param>
        /// <exception cref="ArgumentException">Thrown if the provided JSON string is null or empty, indicating that there is no valid data to write.</exception>
        /// <exception cref="IOException">Rethrown if an IOException is encountered during the console write operation, signaling an issue with outputting the data.</exception>
        public static void WriteJson(string dataJson)
        {
            // Validate that the provided JSON string is not null or empty.
            if (string.IsNullOrEmpty(dataJson))
            {
                throw new ArgumentException();
            }

            try
            {
                // Attempt to write the JSON string to the console.
                Console.WriteLine(dataJson);
            }
            catch (IOException ex)
            {
                // Rethrow an IOException to indicate there was a problem writing to the console.
                throw new IOException();
            }
        }

        /// <summary>
        /// Parses a sequence of lines representing JSON objects into a list of dictionaries. This method utilizes a finite state machine
        /// approach to iterate through each line of the input, distinguishing between being inside or outside of JSON objects and arrays.
        /// It handles the transitions between states based on the syntax encountered in the input lines, such as the start and end of objects
        /// ('{' and '}') and arrays ('[' and ']'). Each JSON object is represented as a dictionary, with key-value pairs extracted from the lines.
        /// Arrays within objects are handled as lists of strings.
        /// </summary>
        /// <param name="lines">An array of strings, each representing a line of JSON data.</param>
        /// <returns>A list of dictionaries, where each dictionary represents a JSON object with its data parsed from the provided lines.</returns>
        private static List<Dictionary<string, object>> StateMachine(string[] lines)
        {
            // Initialize variables for processing the JSON structure.
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            // Initial state: outside of any JSON object.
            State state = State.OutsideObject; 
            Dictionary<string, object> currentObject = new Dictionary<string, object>();
            string currentKey = "";
            // To hold values of an array inside an object.
            List<string> currentArray = new List<string>();

            foreach (string line in lines)
            {
                // Trim whitespace for easier processing.
                string trimmedLine = line.Trim(); 

                switch (state)
                {
                    case State.OutsideObject:
                        // Transition from outside to inside an object.
                        if (trimmedLine.StartsWith("{"))
                        {
                            currentObject = new Dictionary<string, object>();
                            // Change state to inside an object.
                            state = State.InsideObject; 
                        }
                        break;

                    case State.InsideObject:
                        // Transition from inside an object to outside, or process a key-value pair.
                        if (trimmedLine.StartsWith("}"))
                        {
                            // Add the completed object to the result list.
                            result.Add(currentObject);
                            // Change state back to outside an object.
                            state = State.OutsideObject; 
                        }
                        else
                        {
                            // Extract key-value pair from the line.
                            var match = Regex.Match(trimmedLine, "\"(.*?)\":\\s*(.*)");
                            if (match.Success)
                            {
                                // Extract the key.
                                currentKey = match.Groups[1].Value;
                                // Extract the value part.
                                string valuePart = match.Groups[2].Value.Trim(); 

                                if (valuePart.StartsWith("["))
                                {
                                    // Change state to inside an array.
                                    state = State.InsideArray;
                                    // Initialize the array container.
                                    currentArray = new List<string>(); 
                                }
                                else
                                {
                                    // Parse and assign the value to the current key in the object.
                                    currentObject[currentKey] = ParseValue(valuePart);
                                }
                            }
                        }
                        break;

                    case State.InsideArray:
                        // Transition from inside an array back to inside an object, or add an item to the array.
                        if (trimmedLine.StartsWith("]"))
                        {
                            // Assign the array to the current key in the object.
                            currentObject[currentKey] = currentArray;
                            // Change state back to inside an object.
                            state = State.InsideObject; 
                        }
                        else
                        {
                            // Add an item to the current array, trimming unnecessary characters.
                            currentArray.Add(trimmedLine.Trim(' ', ',', '\"'));
                        }
                        break;
                }
            }

            // Return the list of parsed JSON objects.
            return result; 
        }

        /// <summary>
        /// Parses a string value into its corresponding data type. This method attempts to interpret the provided string as a numeric
        /// value (double), a boolean value, or falls back to treating it as a string. It is designed to handle the common data types
        /// found in JSON data, allowing for accurate reconstruction of the original data types from their string representations.
        /// </summary>
        /// <param name="value">The string representation of the value to be parsed.</param>
        /// <returns>The parsed value, converted to its appropriate data type (double, bool, or string).</returns>
        private static object ParseValue(string value)
        {
            // Trim potential trailing commas and whitespace, as found in JSON arrays or object entries.
            value = value.TrimEnd(',', ' ');

            // Attempt to parse the value as a double. If successful, return the double.
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                return number;
            }
            // If the value isn't a number, try parsing it as a boolean. If successful, return the boolean.
            else if (bool.TryParse(value, out bool boolean))
            {
                return boolean;
            }
            // If neither numeric nor boolean, return the value as a string, trimming any enclosing quotation marks.
            else
            {
                return value.Trim('\"');
            }
        }

        /// <summary>
        /// Validates the structure and content of a list of dictionaries representing JSON objects. This method ensures that each object
        /// contains the correct number of key-value pairs and that each key is expected at its respective position. It further verifies
        /// the data types of the values for specific keys, such as integers for 'employee_id' and 'age', booleans for 'is_manager',
        /// and arrays for 'projects'. An ArgumentException is thrown if the data does not meet the expected structure or type requirements.
        /// </summary>
        /// <param name="data">The list of dictionaries to be validated, where each dictionary represents a JSON object.</param>
        /// <exception cref="ArgumentException">Thrown if the data list is empty, null, or if any object does not conform to the expected
        /// structure or data types.</exception>
        private static void CheckDataStructure(List<Dictionary<string, object>> data)
        {
            // Check for empty or null data.
            if (data.Count == 0 || data == null)
            {
                throw new ArgumentException("Передан пустой файл или неккоректные данные.");
            }

            // Iterate through each object in the data list.
            foreach (var obj in data)
            {
                int objLength = obj.Count;

                // Validate the number of key-value pairs in each object.
                if (objLength != 7)
                {
                    throw new ArgumentException("Количество пар в объекте не соответсвует варианту.");
                }

                // Iterate through each key-value pair in the object.
                for (int i = 0; i < objLength; i++)
                {
                    // The key of the current pair.
                    string objKey = obj.ElementAt(i).Key;
                    // The value of the current pair.
                    var objVal = obj.ElementAt(i).Value; 

                    // Check each key and its corresponding value based on the expected order and data type.
                    switch (i)
                    {
                        case 0:
                            CheckKey(objKey, "employee_id", i + 1); 
                            CheckIntValue(objVal, "employee_id"); 
                            break;
                        case 1:
                            CheckKey(objKey, "full_name", i + 1); 
                            break;
                        case 2:
                            CheckKey(objKey, "age", i + 1);
                            CheckIntValue(objVal, "age");
                            break;
                        case 3:
                            CheckKey(objKey, "department", i + 1);
                            break;
                        case 4:
                            CheckKey(objKey, "is_manager", i + 1);
                            CheckBoolValue(objVal, "is_manager");
                            break;
                        case 5:
                            CheckKey(objKey, "salary", i + 1);
                            CheckDoubleValue(objVal, "salary");
                            break;
                        case 6:
                            CheckKey(objKey, "projects", i + 1);
                            CheckArrayValue(objVal, "projects"); 
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Validates that a given key matches an expected key name. This method is essential for ensuring that the structure of a
        /// JSON object (represented as a dictionary) adheres to a predefined schema, where specific keys are expected in a certain
        /// order. If the key does not match the expected name, an ArgumentException is thrown, indicating a discrepancy in the expected
        /// data structure.
        /// </summary>
        /// <param name="key">The actual key name found in the dictionary.</param>
        /// <param name="expectedKeyName">The expected key name, according to the predefined schema.</param>
        /// <param name="n">The position or order of the key in the dictionary, used for reference in the error message.</param>
        /// <exception cref="ArgumentException">Thrown if the actual key name does not match the expected key name, with an error message
        /// specifying the expected key name and its position.</exception>
        private static void CheckKey(string key, string expectedKeyName, int n)
        {
            // Compare the actual key with the expected key name.
            if (key != expectedKeyName)
            {
                // If they do not match, throw an exception indicating the discrepancy.
                throw new ArgumentException($"{n} ключ должен быть '{expectedKeyName}'.");
            }
        }

        /// <summary>
        /// Validates that the value associated with a specified key is an integer. This check is crucial for ensuring data integrity,
        /// especially when the data schema expects certain keys to have integer values. If the value cannot be parsed as an integer,
        /// an ArgumentException is thrown to indicate the type mismatch.
        /// </summary>
        /// <param name="objVal">The value to be checked, associated with the key.</param>
        /// <param name="keyName">The name of the key, used for reference in the exception message.</param>
        /// <exception cref="ArgumentException">Thrown if the value cannot be parsed as an integer, indicating a schema violation.</exception>
        private static void CheckIntValue(object objVal, string keyName)
        {
            string? objValString = Convert.ToString(objVal);

            if (!int.TryParse(objValString, out _))
            {
                throw new ArgumentException($"Значение '{keyName}' должно быть целым числом.");
            }
        }

        /// <summary>
        /// Validates that the value associated with a specified key is a boolean. This is important for keys that are expected
        /// to represent boolean values (true/false). If the value is not a boolean, an ArgumentException is thrown, which helps
        /// maintain the logical consistency of the data.
        /// </summary>
        /// <param name="objVal">The value to be checked.</param>
        /// <param name="keyName">The name of the key, for which the value is expected to be boolean.</param>
        /// <exception cref="ArgumentException">Thrown if the value is not a boolean, indicating a deviation from the expected data type.</exception>
        private static void CheckBoolValue(object objVal, string keyName)
        {
            if (!(objVal is bool))
            {
                throw new ArgumentException($"Значение '{keyName}' должно принимать true или false.");
            }
        }

        /// <summary>
        /// Validates that the value associated with a specified key is a number (integer or double). This validation ensures
        /// that numerical fields contain appropriate values. An ArgumentException is thrown if the value cannot be parsed as a
        /// double, which covers both integer and floating-point numbers.
        /// </summary>
        /// <param name="objVal">The value to be checked.</param>
        /// <param name="keyName">The key name, used for identifying the value in the exception message.</param>
        /// <exception cref="ArgumentException">Thrown if the value cannot be parsed as a number, indicating an unexpected data type.</exception>
        private static void CheckDoubleValue(object objVal, string keyName)
        {
            string? objValString = Convert.ToString(objVal);

            if (!double.TryParse(objValString, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _))
            {
                throw new ArgumentException($"Значение '{keyName}' должно быть целым или дробным числом.");
            }
        }

        /// <summary>
        /// Validates that the value associated with a specified key is an array. The method checks if the value, when converted
        /// to a string, starts with '[' and ends with ']', which are the indicators of an array in JSON format. This check is
        /// essential for keys expected to map to array values, ensuring the structure matches the expected format.
        /// </summary>
        /// <param name="objVal">The value to be checked.</param>
        /// <param name="keyName">The name of the key associated with the expected array value.</param>
        /// <exception cref="ArgumentException">Thrown if the value does not represent an array, as indicated by the lack of '[' or ']'.</exception>
        private static void CheckArrayValue(object objVal, string keyName)
        {
            string? objValString = Convert.ToString(objVal);

            if (objValString == null || (!objValString.StartsWith('[') && !objValString.EndsWith(']')))
            {
                throw new ArgumentException($"Значение '{keyName}' должно быть массивом.");
            }
        }
    }
}
