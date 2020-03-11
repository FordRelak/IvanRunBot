using System;

namespace DB_2._0
{
    class Program
    {
        static void Main(string[] args)
        {
            new Bot().MainAsync().GetAwaiter().GetResult();
        }
    }
}
