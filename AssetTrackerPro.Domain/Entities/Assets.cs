using System;

namespace AssetTrackerPro.Domain.Entities;

public class Assets
{
    public int Id { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public int? EmployeeId { get; set; }

    public Employees? Employee { get; set; }
}
