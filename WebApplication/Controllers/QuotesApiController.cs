using System;
using System.Collections.Generic;
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
        public IActionResult Search(
            string author = null,
            string q = null,
            string from = null,
            string to = null,
            int page = 1,
            int pageSize = 20)
        {
            // Validate pagination params
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest();

            // Validate date params
            DateTime? fromDate = null;
            DateTime? toDate = null;
            
            if (from != null)
            {
                if (!DateTime.TryParse(from, out var parsedFrom))
                    return BadRequest();
                fromDate = parsedFrom;
            }
            
            if (to != null)
            {
                if (!DateTime.TryParse(to, out var parsedTo))
                    return BadRequest();
                toDate = parsedTo;
            }
            
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                return BadRequest();

            // Build query with filters
            var query = _context.Quote.AsNoTracking().AsQueryable();
            
            if (!string.IsNullOrEmpty(author))
                query = query.Where(q => q.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
                
            if (!string.IsNullOrEmpty(q))
                query = query.Where(q => q.QuoteText.Contains(q, StringComparison.OrdinalIgnoreCase));
                
            if (fromDate.HasValue)
                query = query.Where(q => q.Date >= fromDate.Value);
                
            if (toDate.HasValue)
                query = query.Where(q => q.Date <= toDate.Value);

            // Get total count
            var total = query.Count();
            
            // Apply pagination and ordering
            var items = query
                .OrderByDescending(q => q.Date)
                .ThenByDescending(q => q.ID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
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