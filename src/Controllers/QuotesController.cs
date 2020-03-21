using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Quotes.Data;
using Quotes.Model;

namespace Quotes.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Content")]
    public class QuotesController : Controller
    {
        private readonly QuotesDbContext _context;

        public QuotesController(QuotesDbContext context)
        {
            _context = context;
        }

        public string DbInfo()
        {
            return _context.Database.GetDbConnection().ConnectionString;
        }

        [AllowAnonymous]
        [Route("")]
        [Route("Index")]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            return await ViewAll();
        }


        [Route("Modify")]
        public async Task<IActionResult> Modify()
        {
            return await ViewAll();
        }

        private async Task<IActionResult> ViewAll()
        {
            return View(await _context.Quote.OrderByDescending(q => q.ID).ToListAsync());
        }

        [Route("Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quote
                .SingleOrDefaultAsync(m => m.ID == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Create")]
        //[HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Date,Author,AuthorInfo,QuoteText")] Quote quote)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Modify));
            }
            return View(quote);
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quote.SingleOrDefaultAsync(m => m.ID == id);
            if (quote == null)
            {
                return NotFound();
            }
            return View(quote);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Date,Author,AuthorInfo,QuoteText")] Quote quote)
        {
            if (id != quote.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuoteExists(quote.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Modify));
            }
            return View(quote);
        }

        [Route("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quote
                .SingleOrDefaultAsync(m => m.ID == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quote = await _context.Quote.SingleOrDefaultAsync(m => m.ID == id);
            _context.Quote.Remove(quote);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Modify));
        }

        private bool QuoteExists(int id)
        {
            return _context.Quote.Any(e => e.ID == id);
        }
    }
}
