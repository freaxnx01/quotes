using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Quotes.DataModel;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string author, [FromQuery] string q, [FromQuery] string from, [FromQuery] string to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            // Validate input
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest();

            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(from) && !DateTime.TryParse(from, out DateTime parsedFrom))
                return BadRequest();
            else if (!string.IsNullOrEmpty(from))
                fromDate = parsedFrom;

            if (!string.IsNullOrEmpty(to) && !DateTime.TryParse(to, out DateTime parsedTo))
                return BadRequest();
            else if (!string.IsNullOrEmpty(to))
                toDate = parsedTo;

            if (fromDate != null && toDate != null && fromDate > toDate)
                return BadRequest();

            // Build query
            var query = _context.Quote.AsNoTracking();

            if (!string.IsNullOrEmpty(author))
                query = query.Where(x => EF.Functions.Like(x.Author, $"%{author}%"));

            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => EF.Functions.Like(x.QuoteText, $"%{q}%"));

            if (fromDate != null)
                query = query.Where(x => x.Date >= fromDate);

            if (toDate != null)
                query = query.Where(x => x.Date <= toDate);

            // Get total count and paginated results
            var total = query.Count();
            var items = query
                .OrderByDescending(q => q.Date)
                .ThenByDescending(q => q.ID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new OkObjectResult(new
            {
                items,
                page,
                pageSize,
                total
            });
        }
    }
}