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
            {
                return BadRequest(new { error = "page must be >= 1" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "pageSize must be between 1 and 100" });
            }

            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(from))
            {
                if (!DateTime.TryParse(from, out var parsedFrom))
                {
                    return BadRequest(new { error = "from is not a valid date" });
                }
                fromDate = parsedFrom;
            }

            if (!string.IsNullOrEmpty(to))
            {
                if (!DateTime.TryParse(to, out var parsedTo))
                {
                    return BadRequest(new { error = "to is not a valid date" });
                }
                toDate = parsedTo;
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest(new { error = "from cannot be greater than to" });
            }

            var query = _context.Quote.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(x => EF.Functions.Like(x.Author, $"%{author}%"));
            }

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(x => EF.Functions.Like(x.QuoteText, $"%{q}%"));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.Date <= toDate.Value);
            }

            var total = query.Count();

            var items = query
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.ID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new SearchResult
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total
            });
        }
    }

    public class SearchResult
    {
        public List<Quote> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
}