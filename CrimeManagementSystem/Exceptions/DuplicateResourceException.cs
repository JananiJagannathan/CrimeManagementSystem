namespace CrimeManagementSystem.Exceptions
{
    public class DuplicateResourceException : Exception
    {
        public DuplicateResourceException(string resource, string field, string value)
            : base($"{resource} with {field} '{value}' already exists in the system.")
        {
        }

        public DuplicateResourceException(string message)
            : base(message)
        {
        }
    }
}