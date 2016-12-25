namespace Napack.Server
{
    public class DocumentationModel
    {
        public DocumentationModel()
        {
        }

        public DocumentationModel(string name, string html)
        {
            this.Name = name;
            this.Html = html;
        }

        public string Html { get; set; }

        public string Name { get; set; }
    }
}