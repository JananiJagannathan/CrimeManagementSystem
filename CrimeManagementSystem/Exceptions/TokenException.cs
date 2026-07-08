namespace CrimeManagementSystem.Exceptions
{
    public class TokenException : Exception
    {
        public TokenException(string message)
            : base(message)
        {
        }

        public static TokenException Expired()
            => new TokenException(
                "Your session has expired. Please log in again to continue.");

        public static TokenException Invalid()
            => new TokenException(
                "Invalid authentication token. Please log in again.");
    }
}