namespace CrimeManagementSystem.Exceptions
{
    public class PasswordException : Exception
    {
        public PasswordException(string message)
            : base(message)
        {
        }

        public static PasswordException IncorrectPassword()
            => new PasswordException(
                "The current password you entered is incorrect. Please try again.");

        public static PasswordException WeakPassword()
            => new PasswordException(
                "The new password does not meet security requirements. " +
                "Password must be at least 6 characters long.");
    }
}