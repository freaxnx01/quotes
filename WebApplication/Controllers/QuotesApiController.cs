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
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Validate input parameters
            if (page < 1)
            {
                return BadRequest("Page must be >= 1");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("PageSize must be between 1 and 100");
            }

            if (from.HasValue && to.HasValue && from > to)
            {
                return BadRequest("From date must be <= To date");
            }

            var query = _context.Quote.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(author))
            {
                // Using EF.Functions.Like for case-insensitive SQL LIKE operation
                query = query.Where(q => EF.Functions.Like(q.Author, $"%{author}%"));
            }

            if (!string.IsNullOrEmpty(q))
            {
                // Using EF.Functions.Like for case-insensitive SQL LIKE operation
                query = query.Where(q => EF.Functions.Like(q.QuoteText, $"%{q}%"));
            }

            if (from.HasValue)
            {
                query = query.Where(q => q.Date >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(q => q.Date <= to.Value);
            }

            // Get total count before pagination
            var total = query.Count();

            // Apply ordering and pagination
            var items = query
                .OrderByDescending(q => q.Date)
                .ThenByDescending(q => q.ID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();

            return new ObjectResult(new
            {
                items = items,
                page = page,
                pageSize = pageSize,
                total = total
            });
        }
    }
}