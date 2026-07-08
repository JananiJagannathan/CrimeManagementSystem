namespace CrimeManagementSystem.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string resource, int id)
            : base($"{resource} with ID {id} was not found in the system.")
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}