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
            [FromQuery(Name = "q")] string quoteText,
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest(new { error = "Page must be greater than or equal to 1" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "PageSize must be between 1 and 100" });
            }

            // Parse and validate date parameters
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(from))
            {
                if (!DateTime.TryParse(from, out DateTime parsedFrom))
                {
                    return BadRequest(new { error = "Invalid 'from' date format. Use yyyy-MM-dd" });
                }
                fromDate = parsedFrom;
            }

            if (!string.IsNullOrEmpty(to))
            {
                if (!DateTime.TryParse(to, out DateTime parsedTo))
                {
                    return BadRequest(new { error = "Invalid 'to' date format. Use yyyy-MM-dd" });
                }
                toDate = parsedTo;
            }

            // Validate date range
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest(new { error = "'from' date cannot be after 'to' date" });
            }

            // Build query with AsNoTracking()
            var query = _context.Quote.AsNoTracking().AsQueryable();

            // Apply author filter (case-insensitive, contains)
            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(q => EF.Functions.Like(q.Author, $"%{author}%"));
            }

            // Apply quote text filter (case-insensitive, contains)
            if (!string.IsNullOrEmpty(quoteText))
            {
                query = query.Where(q => EF.Functions.Like(q.QuoteText, $"%{quoteText}%"));
            }

            // Apply date filters
            if (fromDate.HasValue)
            {
                query = query.Where(q => q.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(q => q.Date <= toDate.Value);
            }

            // Get total count
            var total = query.Count();

            // Order by Date descending, then ID descending as tiebreaker
            query = query.OrderByDescending(q => q.Date).ThenByDescending(q => q.ID);

            // Apply pagination
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Return paginated response
            var response = new PaginatedResponse<Quote>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return Ok(response);
        }
    }
}