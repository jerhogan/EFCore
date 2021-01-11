using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement
{
    public class Initialize
    {
        public static EmployeeManagementContext GetContext()
        {
            var connectionString = @"Server=LAPTOP-N32LP9FR;Database=SampleEmployeeManagementDB;Trusted_Connection=True;";
            DbContextOptionsBuilder<EmployeeManagementContext> options = new DbContextOptionsBuilder<EmployeeManagementContext>();
            options.UseSqlServer(connectionString);
            return new EmployeeManagementContext(options.Options);
        }
    }
}
