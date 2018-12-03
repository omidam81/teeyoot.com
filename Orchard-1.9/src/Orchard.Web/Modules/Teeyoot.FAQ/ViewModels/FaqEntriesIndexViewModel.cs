using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.FAQ.Models;

namespace Teeyoot.FAQ.ViewModels
{
    public class FaqEntriesIndexViewModel
    {
        public dynamic[] FaqEntries { get; set; }
        public dynamic Pager { get; set; }
        public FaqEntrySearchViewModel Search { get; set; }

        public FaqSectionRecord[] Sections { get; set; }

        public virtual int CountryId { get; set; }
        public virtual int CultureId { get; set; }
        public virtual string Culture { get; set; }
        public virtual List<KeyValuePair<int, string>> Countries { get; set; }
        public virtual List<KeyValuePair<int, string>> Cultures { get; set; }


        public FaqEntriesIndexViewModel() {
            Search = new FaqEntrySearchViewModel();
        }

        public FaqEntriesIndexViewModel(IEnumerable<dynamic> entries, IEnumerable<FaqSectionRecord> sections, FaqEntrySearchViewModel search, dynamic pager)
        {
            Sections = sections.ToArray();
            FaqEntries = entries.ToArray();
            Search = search;
            Pager = pager;
        }
    }
}