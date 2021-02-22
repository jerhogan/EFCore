using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public sealed class Singleton
    {
        public int currentUserId = 0;
        public static Singleton Instance { get; } = new Singleton();

        private Singleton() { }
    }
}
