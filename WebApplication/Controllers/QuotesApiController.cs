using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] QuoteSearchParams searchParams)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IQueryable<Quote> query = _context.Quote.AsNoTracking();

            if (!string.IsNullOrEmpty(searchParams.Author))
            {
                query = query.Where(q => EF.Functions.Like(q.Author, $"%{searchParams.Author}%"));
            }

            if (!string.IsNullOrEmpty(searchParams.Q))
            {
                query = query.Where(q => EF.Functions.Like(q.QuoteText, $"%{searchParams.Q}%"));
            }

            if (searchParams.From.HasValue)
            {
                query = query.Where(q => q.Date >= searchParams.From.Value);
            }

            if (searchParams.To.HasValue)
            {
                query = query.Where(q => q.Date <= searchParams.To.Value);
            }

            int total = await query.CountAsync();

            List<Quote> items = await query
                .OrderByDescending(q => q.Date)
                .ThenByDescending(q => q.ID)
                .Skip((searchParams.Page - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .ToListAsync();

            return Ok(new
            {
                items,
                page = searchParams.Page,
                pageSize = searchParams.PageSize,
                total
            });
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