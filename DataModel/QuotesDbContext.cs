using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Quotes.Data
{
    public class QuotesDbContext : DbContext
    {
        public QuotesDbContext()
        {
            
        }
        
        public QuotesDbContext (DbContextOptions<QuotesDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            //=> options.UseSqlite(@"DataModel Source=data\quote.db");        
            => options.UseSqlite($"DataModel Source={new FileInfo(Path.Combine("data", "quote.db")).FullName}");
        
        public DbSet<Quote> Quote { get; set; }
    }
}
