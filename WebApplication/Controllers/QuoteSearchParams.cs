using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Quotes.Controllers
{
    public class QuoteSearchParams : IValidatableObject
    {
        public string Author { get; set; }
        public string Q { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 20;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From.HasValue && To.HasValue && From.Value > To.Value)
            {
                yield return new ValidationResult("The 'from' date cannot be greater than the 'to' date.", new[] { nameof(From), nameof(To) });
            }
        }
    }
}