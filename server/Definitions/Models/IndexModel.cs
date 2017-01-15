namespace Napack.Server
{
    public class IndexModel
    {
        public IndexModel()
        {
        }

        public IndexModel(string administratorEmail)
        {
            this.AdministratorEmail = administratorEmail;
        }

        public string AdministratorEmail { get; set; }
    }
}