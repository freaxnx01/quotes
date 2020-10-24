using System;
using System.IO;
using System.Linq;
using Quotes.DataModel;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(new FileInfo(Path.Combine("data", "quote.db")).FullName);
            
            using (var db = new QuotesDbContext())
            {
                Console.WriteLine(db.Quote.ToList().Count);
            }

            Console.WriteLine("Hello World!");
        }
    }
}