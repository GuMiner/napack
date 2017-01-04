using System.IO;
using MarkdownSharp;
using Nancy;
using Napack.Analyst;
using NLog;

namespace Napack.Server
{
    /// <summary>
    /// Handles Napack Framwork Server Documentation rendering.
    /// </summary>
    public class DocumentationModule : NancyModule
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DocumentationModule(INapackStorageManager napackManager)
            : base("/documentation")
        {
            // Retrieves and displays a documentation file from Markdown.
            Get["/{markdownFile}"] = parameters =>
            {
                // Prohibit going to a different directory.
                string rootDirectory = NapackAnalyst.RootDirectory + "/Content/docs/";
                string markdownFileName = ((string)parameters.markdownFile).Replace("..", string.Empty);
                string markdownFilePath = rootDirectory + markdownFileName;
                logger.Debug(markdownFilePath);

                string markdownFile = File.ReadAllText(markdownFilePath);
                string html = new Markdown(new MarkdownOptions()
                {
                    EmptyElementSuffix = ">",
                }).Transform(markdownFile);

                return View["Documentation", new DocumentationModel(markdownFileName, html)];
            };
        }
    }
}
