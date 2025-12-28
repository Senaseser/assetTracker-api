using System.Collections.Generic;

namespace AssetTrackerPro.Domain.Entities;

public class Departments
{
    public int Id { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public string? Location { get; set; }

    public ICollection<Employees> Employees { get; set; } = new List<Employees>();
}
