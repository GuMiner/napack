namespace Napack.Server
{
    public class VersionsModel
    {
        public VersionsModel()
        {
        }

        public VersionsModel(NapackMetadata metadata)
        {
            this.Metadata = metadata;
        }

        public NapackMetadata Metadata { get; set; }
    }
}