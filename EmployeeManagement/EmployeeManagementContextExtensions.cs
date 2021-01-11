using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement
{
    public static class EmployeeManagementContextExtensions
    {
        public static void EnsureSeedDataForContext(this EmployeeManagementContext context)
        {
            if (context.Departments.Any())
            {
                return;
            }

            Department department = new Department
            {
                DepartmentName = "Technology",
                Employees = new List<Employee>
                {
                    new Employee() {EmployeeName = "Jack"},
                    new Employee() {EmployeeName = "Kim"},
                    new Employee() {EmployeeName = "Shen"}
                }
            };

            context.Departments.Add(department);

            Employee employee = new Employee
            {
                EmployeeName = "Akhil Mittal",
                DepartmentId = 1
            };

            context.Employees.Add(employee);
            context.SaveChanges();
        }
    }
}
