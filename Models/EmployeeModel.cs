namespace ReflectionDynamicFilter.Models
{
    public class EmployeeModel
    {
        public EmployeeModel(
            int id, 
            string name, 
            string department, 
            int age)
        {
            Id = id;
            Name = name;
            Department = department;
            Age = age;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public int Age { get; set; }
    }
}
