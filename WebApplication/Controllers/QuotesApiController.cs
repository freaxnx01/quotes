using System;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Quotes.DataModel;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Quotes.DataModel;

namespace Quotes.Controllers
{
    [Route("Api")]
    public class QuotesApiController : Controller
    {
        private readonly QuotesDbContext _context;

        public QuotesApiController(QuotesDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<Quote> GetAll()
        {
            return _context.Quote.OrderByDescending(q => q.ID).ToList();
        }

        [HttpGet("{id}", Name = "GetQuote")]
        public IActionResult GetById(long id)
        {
            var item = _context.Quote.FirstOrDefault(t => t.ID == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpGet("random")]
        public IActionResult GetRandom()
        {
            var ids = _context.Quote.Select(q => q.ID).ToList();
            int randomIndex = new Random().Next(0, ids.Count - 1);
            return GetById(ids[randomIndex]);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string author,
            string q,
            string from,
            string to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest("Page number must be 1 or greater.");
            }
            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100.");
            }

            // Validate and parse date parameters
            DateTime? fromDate = null;
            if (!string.IsNullOrEmpty(from))
            {
                if (!DateTime.TryParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
                {
                    return BadRequest("Invalid 'from' date format. Use yyyy-MM-dd.");
                }
                fromDate = parsedFrom;
            }

            DateTime? toDate = null;
            if (!string.IsNullOrEmpty(to))
            {
                if (!DateTime.TryParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
                {
                    return BadRequest("Invalid 'to' date format. Use yyyy-MM-dd.");
                }
                toDate = parsedTo;
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("'from' date cannot be greater than 'to' date.");
            }

            var query = _context.Quote.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(quote => quote.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(quote => quote.QuoteText.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(quote => quote.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(quote => quote.Date <= toDate.Value);
            }

            // Order results
            query = query.OrderByDescending(quote => quote.Date).ThenByDescending(quote => quote.ID);

            // Get total count before pagination
            var total = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                items,
                page,
                pageSize,
                total
            });
        }
    }
}