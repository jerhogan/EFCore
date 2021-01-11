using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagement
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EmployeeManagementContext>
    {
        public EmployeeManagementContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EmployeeManagementContext>();
            optionsBuilder.UseSqlServer(@"Server=LAPTOP-N32LP9FR;Database=SampleEmployeeManagementDB;Trusted_Connection=True;");

            return new EmployeeManagementContext(optionsBuilder.Options);
        }
    }
}
