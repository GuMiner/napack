using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace NapackExtension
{
    [Guid("9E769749-7E6F-4A0A-BE15-DB12E5C714DB")]
    public class SearchFormToolWindow : ToolWindowPane
    {
        private SearchForm searchForm;
        
        public SearchFormToolWindow() : base(null)
        {
            this.Caption = "Search for Napacks";

            this.searchForm = new SearchForm();
            base.Content = searchForm;
        }
    }
}