using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotes.Model
{
    [Table("Quote")]
    public class Quote
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted
        { get
            {
                return Date.ToString("d. MMMM yyy", new CultureInfo("de-CH"));
            }
        }

        public string Author { get; set; }
        public string AuthorInfo { get; set; }
        public string QuoteText { get; set; }

        public string AuthorWithInfo
        {
            get
            {
                if (!string.IsNullOrEmpty(AuthorInfo))
                {
                    return $"{Author}, {AuthorInfo}";
                }

                return Author;
            }
        }
    }
}
