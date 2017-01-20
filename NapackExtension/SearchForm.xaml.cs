using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Newtonsoft.Json.Linq;

namespace NapackExtension
{
    /// <summary>
    /// Interaction logic for SearchForm.xaml.
    /// </summary>
    [ProvideToolboxControl("NapackExtension.SearchForm", true)]
    public partial class SearchForm : UserControl
    {
        private List<SearchResult> lastSearchResults;

        public SearchForm()
        {
            InitializeComponent();
        }

        private async Task<string> GetMostRecentNapackVersionStringAsync(string napackServer, string napackName)
        {
            string baseUri = napackServer + "/napacks/" + napackName;
            JToken details = await this.GetAsync(new Uri(baseUri));

            int mostRecentMajorVersion = (details["validVersions"] as JArray).Max(token => token.Value<int>());
            JToken majorVersion = await this.GetAsync(new Uri(baseUri + "/" + mostRecentMajorVersion.ToString()));
            string endVersion = (majorVersion["versions"] as JArray).Last.Value<string>();
            return mostRecentMajorVersion + "." + endVersion;
        }
        
        private async Task<Tuple<List<string>, List<string>>> GetNapackDetailsAsync(string napackServer, string napackName)
        {
            string baseUri = napackServer + "/napacks/" + napackName;

            string mostRecentVersion = await this.GetMostRecentNapackVersionStringAsync(napackServer, napackName);
            JToken version = await this.GetAsync(new Uri(baseUri + "/" + mostRecentVersion));
            
            return Tuple.Create(
                (version["authors"] as JArray).Select(item => item.Value<string>()).ToList(),
                (version["dependencies"] as JArray).Select(item => item.Value<string>()).ToList());
        }

        private async Task<List<SearchResult>> GetSearchResultsAsync(string napackServer, int skip, int top, string search)
        {
            Uri uri = new Uri(napackServer + "/search/query?$skip=" + skip + "&$top=" + top + "&search=" + search);
            JToken token = await this.GetAsync(uri);

            List<SearchResult> results = (token["results"] as JArray).Select(result =>
            {
                string napackName = result["name"].Value<string>();
                return new SearchResult(napackName, result["description"].Value<string>(), napackServer + "/api/" + napackName, result["licenseType"].Value<string>());
            }).ToList();

            return results;
        }

        private async Task<JToken> GetAsync(Uri uri)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(uri))
                {
                    return JValue.Parse(await response.Content.ReadAsStringAsync());
                }
            }
        }

        private void WebBrowserLinkOpener(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }

        private async void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            this.progressBar.IsIndeterminate = true;
            try
            {
                // Data binding isn't true data binding. Just like 'free software' doesn't mean free as in true freedom.
                this.dataGrid.SelectedItem = null;
                this.authorsListBox.Items.Clear();
                this.dependenciesListBox.Items.Clear();
                this.licenseLabel.Visibility = Visibility.Hidden;

                lastSearchResults = await this.GetSearchResultsAsync(NapackCommandsPackage.Options.NapackServer, 
                    0, NapackCommandsPackage.Options.MaxResults, this.searchInputBox.Text);
                this.dataGrid.ItemsSource = lastSearchResults;
                foreach (DataGridColumn column in this.dataGrid.Columns)
                {
                    column.Width = DataGridLength.SizeToCells;
                }
            }
            finally
            {
                this.progressBar.IsIndeterminate = false;
            }   
        }

        private async void GetButtonClick(object sender, RoutedEventArgs e)
        {
            SearchResult result = this.dataGrid.SelectedItem as SearchResult;
            if (result != null)
            {
                this.detailsProgressBar.IsIndeterminate = true;

                try
                {
                    string mostRecentMajorVersion = await GetMostRecentNapackVersionStringAsync(NapackCommandsPackage.Options.NapackServer, result.Name);
                    NapackCommandsPackage.Instance.AddNapackToProject(result.Name, mostRecentMajorVersion);
                }
                finally
                {
                    this.detailsProgressBar.IsIndeterminate = false;
                }
            }
        }

        private async void ItemSelected(object sender, RoutedEventArgs e)
        {
            SearchResult result = this.dataGrid.SelectedItem as SearchResult;
            if (result != null)
            {
                this.detailsProgressBar.IsIndeterminate = true;

                try
                {
                    this.authorsListBox.Items.Clear();
                    this.dependenciesListBox.Items.Clear();
                    Tuple<List<string>, List<string>> napackDetails = await this.GetNapackDetailsAsync(NapackCommandsPackage.Options.NapackServer, result.Name);
                    foreach (string item in napackDetails.Item1)
                    {
                        this.authorsListBox.Items.Add(item);
                    }

                    foreach (string item in napackDetails.Item2)
                    {
                        this.dependenciesListBox.Items.Add(item);
                    }

                    this.licenseLabel.Content = result.GetLicenseType();
                    this.licenseLabel.Visibility = Visibility.Visible;
                }
                finally
                {
                    this.detailsProgressBar.IsIndeterminate = false;
                }
            }
        }

        private class SearchResult
        {
            private string licenseType;

            public SearchResult(string name, string description, string api, string licenseType)
            {
                this.Name = name;
                this.Description = description;
                this.API = api;
                this.licenseType = licenseType;
            }

            public string Name { get; private set; }

            public string Description { get; private set; }

            public string API { get; private set; }

            public string GetLicenseType() => this.licenseType + " License";
        }
    }
}
