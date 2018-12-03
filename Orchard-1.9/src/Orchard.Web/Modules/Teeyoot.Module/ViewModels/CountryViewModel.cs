using System.Collections.Generic;

namespace Teeyoot.Module.ViewModels
{
    public class CountryViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<SelectedCultureItem> Cultures { get; set; }
        public IEnumerable<int> SelectedCultures { get; set; }
        public int DefaultCultureId { get; set; }

        public CountryViewModel(IEnumerable<SelectedCultureItem> cultures)
        {
            Cultures = cultures;
        }

        public CountryViewModel()
        {
        }
    }
}
