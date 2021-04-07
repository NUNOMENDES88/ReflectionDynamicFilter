namespace ReflectionDynamicFilter.Models
{
    public class EmployeeModel
    {
        public EmployeeModel(
            int id, 
            string name, 
            string department, 
            int age,
            int salary)
        {
            Id = id;
            Name = name;
            Department = department;
            Age = age;
            Salary = new SalaryModel() { Value = salary };
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public int Age { get; set; }
        public SalaryModel Salary { get; set; }
    }

    public class SalaryModel
    {
        public int Value { get; set; }
    }



}
