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
    }
}