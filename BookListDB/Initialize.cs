using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BookListDB
{
    public class Initialize
    {
        public static BookListContext GetContext()
        {
            //            var connectionString = @"Server=LAPTOP-N32LP9FR;Database=ConsoleBookListDB;Trusted_Connection=True;MultipleActiveResultSets=true;";
            var connectionString = @"Server=localhost;Database=ConsoleBookListDB;Trusted_Connection=True;MultipleActiveResultSets=true;";
            DbContextOptionsBuilder<BookListContext> options = new DbContextOptionsBuilder<BookListContext>();
            options.UseSqlServer(connectionString);
            return new BookListContext(options.Options);
        }
    }
}
