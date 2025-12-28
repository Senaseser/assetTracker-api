using System.Collections.Generic;

namespace AssetTrackerPro.Domain.Entities;

public class Employees
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }

    public Departments? Department { get; set; }
    public ICollection<Assets> Assets { get; set; } = new List<Assets>();
}
