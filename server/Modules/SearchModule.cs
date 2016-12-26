using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;

namespace Napack.Server
{
    /// <summary>
    /// Handles Napack Framwork Server Documentation rendering.
    /// </summary>
    public class SearchModule : NancyModule
    {
        // TODO configurable
        private const int MaxResultsPerPage = 50;

        public SearchModule(INapackStorageManager napackManager)
            : base("/search")
        {
            // Retrieves and displays the basic search page.
            Get["/"] = parameters =>
            {
                return View["Search"];
            };

            // Retrieves a sequence of search results.
            Get["/query"] = parameters =>
            {
                string query = (string)this.Request.Query["search"];
                int skip = int.Parse(this.Request.Query["$skip"]);
                int top = Math.Min(int.Parse(this.Request.Query["$top"]), SearchModule.MaxResultsPerPage);

                List<NapackSearchIndex> packagesFound = napackManager.FindPackages(query, skip, top);
                return this.Response.AsJson(new
                {
                    Results = packagesFound.Select(package => package.ToAnonymousType()),
                    CanContinue = packagesFound.Count == top
                });
            };

            // Retrives the detailed information about a search result.
            Get["/result/{packageName}"] = parameters =>
            {
                string packageName = parameters.packageName;
                NapackMetadata metadata = napackManager.GetPackageMetadata(packageName);
                NapackStats stats = napackManager.GetPackageStatistics(packageName);

                return this.Response.AsJson(new
                {
                    Authors = stats.AllAuthors,
                    AuthorizedUsers = metadata.AuthorizedUserIds,
                    MoreInformation = metadata.MoreInformation,
                    Tags = metadata.Tags
                });
            };
        }
    }
}
