namespace CrimeManagementSystem.Exceptions
{
    public class IncidentAssignmentException : Exception
    {
        public IncidentAssignmentException(int incidentId, int officerId, string reason)
            : base($"Cannot assign Incident {incidentId} to Officer {officerId}: {reason}")
        {
        }

        public IncidentAssignmentException(string message)
            : base(message)
        {
        }
    }
}