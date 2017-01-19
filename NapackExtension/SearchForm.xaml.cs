using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NapackExtension
{
    /// <summary>
    /// Interaction logic for SearchForm.xaml.
    /// </summary>
    [ProvideToolboxControl("NapackExtension.SearchForm", true)]
    public partial class SearchForm : UserControl
    {
        public SearchForm()
        {
            InitializeComponent();
        }

        private async void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            this.progressBar.IsIndeterminate = true;

            // TODO this needs complete redesign (use user's napack settings, support item clicking, etc) when it's not 10:30 PM.
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync("https://napack.net/search/query?$skip=0&$top=20&search=" + this.searchInputBox.Text))
                    {
                        string responseText = await response.Content.ReadAsStringAsync();
                        JToken token = JValue.Parse(responseText);
                        foreach (var result in token["results"] as JArray)
                        {
                            this.resultsBox.Items.Add(result["name"].Value<string>() + ": " + result["description"].Value<string>());
                        }
                    }
                }
            }
            finally
            {
                this.progressBar.IsIndeterminate = false;
            }   
        }
    }
}
