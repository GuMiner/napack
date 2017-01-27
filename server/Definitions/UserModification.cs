namespace Napack.Server
{
    public enum Operation
    {
        UpdateAccessKeys,
        DeleteUser
    };

    public class UserModification
    {
        public string UserId { get; set; }

        public Operation Operation { get; set; }
    }
}
