namespace ReflectionDynamicFilter
{
    using Enumerations;
    using Helpers;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            var listEmployees = GetListEmployees();
           
            //Filter direct by method
            var listFilters1 = new List<FilterModel>()
            {
                new FilterModel("Salary.Type.Description", "Type1", RelationalOperatorsEnum.Equal),
                new FilterModel("Salary.Value", 1000, RelationalOperatorsEnum.Equal),
                new FilterModel("Name", "Mendes", RelationalOperatorsEnum.Contains, LogicalOperatorsEnum.Or),
            };
            var resultFilter1 = listEmployees.Filter(listFilters1);

            //Generate predicate
            Func<EmployeeModel, bool> newPredicate = FilterHelper.GeneratePredicate<EmployeeModel>(listFilters1);
            var resultFilter2 = listEmployees.Where(newPredicate);

            var listFilters2 = new List<FilterModel>()
            {
                new FilterModel("Name", "tte", RelationalOperatorsEnum.Contains),
                new FilterModel("Age", 30, RelationalOperatorsEnum.Equal),
                new FilterModel("Salary.Value", 2000, RelationalOperatorsEnum.GreaterThan)
            };
            var resultFilter3 = listEmployees.Filter(listFilters2);
        }

        static List<EmployeeModel> GetListEmployees()
        {
            return new List<EmployeeModel>
            {
                new EmployeeModel(1, "David", "Software Developer", 50,1000,"Type1"),
                new EmployeeModel(2, "Charlotte", "Finance", 30, 5000,"Type1"),
                new EmployeeModel(3, "Elizabeth", "Marketing", 40, 3000,"Type1"),
                new EmployeeModel(4, "Thomas", "Office Administration", 35, 4000,"Type1")
            };
        }
    }
}
