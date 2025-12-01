using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents the manager-employee relationship in the system.
/// </summary>
public class ManagerEmployee : BaseEntity
{
    private ManagerEmployee()
    {
    }

    public ManagerEmployee(Guid managerId, Guid employeeId)
    {
        ManagerId = managerId;
        EmployeeId = employeeId;
    }

    public Guid ManagerId { get; private set; }
    public Guid EmployeeId { get; private set; }

    // Navigation properties
    public User? Manager { get; private set; }
    public User? Employee { get; private set; }
}