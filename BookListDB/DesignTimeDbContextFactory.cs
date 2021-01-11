using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BookListDB
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BookListContext>
    {
        public BookListContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookListContext>();
            //            optionsBuilder.UseSqlServer(@"Server=LAPTOP-N32LP9FR;Database=ConsoleBookListDB;Trusted_Connection=True;MultipleActiveResultSets=true;");
            optionsBuilder.UseSqlServer(@"Server=localhost;Database=ConsoleBookListDB;Trusted_Connection=True;MultipleActiveResultSets=true;");
            return new BookListContext(optionsBuilder.Options);
        }
    }
}
