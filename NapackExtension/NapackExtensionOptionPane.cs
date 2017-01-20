using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace NapackExtension
{
    public class NapackExtensionOptionPane : DialogPage
    {
        private string napackServer = "https://napack.net";
        private int maxResults = 42;

        [Category("Global")]
        [DisplayName("Napack Server Name")]
        [Description("The name of the Napack Framework Server to connect to. Must not include a trailing forward slash.")]
        public string NapackServer { get { return napackServer; } set { napackServer = value; } }

        [Category("Search")]
        [DisplayName("Max Search Results")]
        [Description("The maximum number of results to return from a single search. The Napack Framwork Server may return less results than requested by this field.")]
        public int MaxResults { get { return maxResults; } set { maxResults = value; } }
    }
}