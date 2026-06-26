using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult Search(
            [FromQuery] string author,
            [FromQuery] string q,
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1)
                return BadRequest("page must be >= 1.");
            if (pageSize < 1 || pageSize > 100)
                return BadRequest("pageSize must be between 1 and 100.");

            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (from != null)
            {
                if (!DateTime.TryParseExact(from, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedFrom))
                    return BadRequest("from is not a valid date (expected yyyy-MM-dd).");
                fromDate = parsedFrom;
            }

            if (to != null)
            {
                if (!DateTime.TryParseExact(to, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTo))
                    return BadRequest("to is not a valid date (expected yyyy-MM-dd).");
                toDate = parsedTo;
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                return BadRequest("from must not be later than to.");

            IQueryable<Quote> query = _context.Quote.AsNoTracking();

            if (!string.IsNullOrEmpty(author))
            {
                string authorLower = author.ToLower();
                query = query.Where(x => x.Author.ToLower().Contains(authorLower));
            }

            if (!string.IsNullOrEmpty(q))
            {
                string qLower = q.ToLower();
                query = query.Where(x => x.QuoteText.ToLower().Contains(qLower));
            }

            if (fromDate.HasValue)
                query = query.Where(x => x.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.Date <= toDate.Value);

            int total = query.Count();

            List<Quote> items = query
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.ID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new { items, page, pageSize, total });
        }
    }
}