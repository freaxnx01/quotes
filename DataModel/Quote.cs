using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotes.DataModel
{
    [Table("Quote")]
    public class Quote
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("d. MMMM yyy", new CultureInfo("de-CH"));

        public string Author { get; set; }
        public string AuthorInfo { get; set; }
        public string QuoteText { get; set; }

        public string AuthorWithInfo => !string.IsNullOrEmpty(AuthorInfo) ? $"{Author}, {AuthorInfo}" : Author;
    }
}
