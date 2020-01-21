namespace ReflectionDynamicFilter
{
    using Enumerations;
    using Helpers;
    using Models;
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            var listEmployees = GetListEmployees();
            var listFilters1 = new List<FilterModel>()
            {
                new FilterModel("Id", 1, RelationalOperatorsEnum.GreaterThan),
                new FilterModel("Name", "i", RelationalOperatorsEnum.Contains)
            };
            var resultFilter1 = listEmployees.Filter(listFilters1);

            var listFilters2 = new List<FilterModel>()
            {
                new FilterModel("Name", "i", RelationalOperatorsEnum.Contains),
                new FilterModel("Age", 30, RelationalOperatorsEnum.GreaterThan,LogicalOperatorsEnum.Or)
            };
            var resultFilter2 = listEmployees.Filter(listFilters2);
        }

        static List<EmployeeModel> GetListEmployees()
        {
            return new List<EmployeeModel>
            {
                new EmployeeModel(1, "David", "Software Developer", 50),
                new EmployeeModel(2, "Charlotte", "Finance", 30),
                new EmployeeModel(3, "Elizabeth", "Marketing", 40),
                new EmployeeModel(4, "Thomas", "Office Administration", 35)

            };
        }
    }
}
