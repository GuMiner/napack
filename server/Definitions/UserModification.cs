namespace Napack.Server
{
    public enum Operation
    {
        UpdateAccessKeys,
        DeleteUser
    };

    public class UserModification
    {
        string UserId { get; set; }

        public Operation Operation { get; set; }
    }
}
