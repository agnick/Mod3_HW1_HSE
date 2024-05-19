using System.Reflection;

namespace JsonUtils
{
    public class Employee : IComparable<Employee>
    {
        // Private fields of the class corresponding to the json data fields defined by the variant
        private int _employeeId;
        private string _fullName;
        private int _age;
        private string _department;
        private bool _isManager;
        private double _salary;
        private string[] _projects;

        // Private fields for storing sorting settings.
        private static string _sortField = string.Empty;
        private static int _sortDirection = 1;

        // Properties for reading class fields.
        public int EmployeeId => _employeeId;
        public string FullName => _fullName;
        public int Age => _age;
        public string Department => _department;
        public bool IsManager => _isManager;
        public double Salary => _salary;
        public string[] Projects => _projects;

        // Empty constructor with no parameters.
        public Employee()
        {
            _employeeId = 0;
            _fullName = string.Empty;
            _age = 0;
            _department = string.Empty;
            _isManager = false;
            _salary = 0;
            _projects = new string[0];
        }

        // Class constructor
        public Employee(int employeeId, string fullName, int age, string department, bool isManager, double salary, string[] projects)
        {
            _employeeId = employeeId;
            _fullName = fullName;
            _age = age;
            _department = department;
            _isManager = isManager;
            _salary = salary;
            _projects = projects;
        }

        /// <summary>
        /// Method that saves sorting settings passed by the user.
        /// </summary>
        /// <param name="sortField">The field by which classes will be sorted.</param>
        /// <param name="ascending">Parameter to define the sort order.</param>
        public static void SetSortOptions(string sortField, bool ascending)
        {
            // Setting fields.
            _sortField = sortField;
            _sortDirection = ascending ? 1 : -1;
        }

        /// <summary>
        /// Compares the current Employee object with another Employee object, sorting according to a specified field and direction.
        /// </summary>
        /// <param name="other">The Employee object to compare with the current object.</param>
        /// <returns>An integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified field to sort by does not exist in the Employee class.</exception>
        public int CompareTo(Employee other)
        {
            // If the 'other' object is null, consider the current instance as greater.
            if (other == null)
            {
                return 1;
            }

            // Use reflection to get the property by name specified in _sortField.
            PropertyInfo propertyInfo = typeof(Employee).GetProperty(_sortField);
            // If the property does not exist, throw an ArgumentException.
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Поле '{_sortField}' не найдено в классе Employee.");
            }

            // Retrieve the values of the property from both the current and other instances.
            object thisValue = propertyInfo.GetValue(this);
            object otherValue = propertyInfo.GetValue(other);

            // Compare the values of the specified property.
            int comparisonResult = CompareValues(thisValue, otherValue);

            // Adjust the result based on the sort direction (ascending or descending).
            return comparisonResult * _sortDirection;
        }

        /// <summary>
        /// Compares two values, handling different data types including nulls and arrays, and ensuring type compatibility for direct comparison.
        /// </summary>
        /// <param name="thisValue">The value associated with the current object.</param>
        /// <param name="otherValue">The value associated with the object to compare against.</param>
        /// <returns>An integer that indicates the relative order of the values being compared.</returns>
        /// <exception cref="ArgumentException">Thrown if the values are of a type that cannot be compared.</exception>
        private static int CompareValues(object thisValue, object otherValue)
        {
            // If both values are null, they are considered equal.
            if (thisValue == null && otherValue == null) return 0;
            // If only thisValue is null, it is considered less than otherValue.
            if (thisValue == null) return -1;
            // If only otherValue is null, thisValue is considered greater.
            if (otherValue == null) return 1;

            // Attempt to compare values based on their types.
            switch (thisValue)
            {
                // If thisValue implements IComparable and types are the same, compare them directly.
                case IComparable comparableThis when thisValue.GetType() == otherValue.GetType():
                    return comparableThis.CompareTo(otherValue);
                // If both values are arrays, compare their lengths.
                case Array arrayThis when otherValue is Array arrayOther:
                    return arrayThis.Length.CompareTo(arrayOther.Length);
                // If none of the above conditions are met, the data types are unsupported for comparison.
                default:
                    throw new ArgumentException("Неподдерживаемый тип данных для сравнения.");
            }
        }

        /// <summary>
        /// Converts the employee's data into a JSON-like string format, including basic information and a list of projects.
        /// </summary>
        /// <returns>A string representation of the employee object in a JSON-like format.</returns>
        public override string ToString()
        {
            // Generate a list of projects in JSON array format, or an empty array if there are no projects.
            string projectsList = _projects != null && _projects.Length > 0
                          ? "[\n      " + string.Join(",\n      ", _projects.Select(p => $"\"{p}\"")) + "\n    ]"
                          : "[]";

            // Construct and return the JSON-like string representation of the employee object.
            return "  {\n" +
                   $"    \"employee_id\": {_employeeId},\n" +
                   $"    \"full_name\": \"{_fullName}\",\n" +
                   $"    \"age\": {_age},\n" +
                   $"    \"department\": \"{_department}\",\n" +
                   $"    \"is_manager\": {_isManager.ToString().ToLower()},\n" +
                   $"    \"salary\": {_salary},\n" +
                   $"    \"projects\": {projectsList}\n" +
                   "  },";
        }
    }
}