using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Quotes.Model;

namespace Quotes.Data
{
    public class QuotesDbContext : DbContext
    {
        public QuotesDbContext (DbContextOptions<QuotesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Quote> Quote { get; set; }
    }
}
