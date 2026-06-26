using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
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

        // Search endpoint with pagination and optional filters
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string author,
            [FromQuery] string q,
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Validate pagination parameters
            if (page < 1)
                return BadRequest("page must be >= 1");
            if (pageSize < 1 || pageSize > 100)
                return BadRequest("pageSize must be between 1 and 100");

            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!string.IsNullOrEmpty(from))
            {
                if (!DateTime.TryParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    return BadRequest("Invalid from date format");
                fromDate = parsed;
            }
            if (!string.IsNullOrEmpty(to))
            {
                if (!DateTime.TryParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    return BadRequest("Invalid to date format");
                toDate = parsed;
            }
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                return BadRequest("from date cannot be after to date");

            var query = _context.Quote.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(author))
            {
                var authorPattern = $"%{author.ToLower()}%";
                query = query.Where(qt => EF.Functions.Like(qt.Author.ToLower(), authorPattern));
            }
            if (!string.IsNullOrEmpty(q))
            {
                var textPattern = $"%{q.ToLower()}%";
                query = query.Where(qt => EF.Functions.Like(qt.QuoteText.ToLower(), textPattern));
            }
            if (fromDate.HasValue)
                query = query.Where(qt => qt.Date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(qt => qt.Date <= toDate.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(qt => qt.Date)
                .ThenByDescending(qt => qt.ID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                items,
                page,
                pageSize,
                total
            };
            return Ok(result);
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
    }
}