using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = Initialize.GetContext();
            context.EnsureSeedDataForContext();
        }
    }
}
