using AssetTrackerPro.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly AssetTrackerDbContext _dbContext;

    public AssetsController(AssetTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssetListItem>>> GetAll()
    {
        var assets = await _dbContext.Assets
            .AsNoTracking()
            .Include(a => a.Employee)
            .ThenInclude(e => e.Department)
            .Select(a => new AssetListItem
            {
                Id = a.Id.ToString(),
                AssetName = a.AssetName,
                PurchaseDate = a.PurchaseDate,
                SerialNumber = a.SerialNumber,
                Employee = a.Employee != null
                    ? new EmployeeItem
                    {
                        Id = a.Employee.Id,
                        FullName = a.Employee.FullName,
                        Email = a.Employee.Email,
                        DepartmentId = a.Employee.DepartmentId,
                        Department = a.Employee.Department != null
                            ? new DepartmentItem
                            {
                                Id = a.Employee.Department.Id,
                                DeptName = a.Employee.Department.DeptName,
                                Location = a.Employee.Department.Location
                            }
                            : null
                    }
                    : null
            })
            .ToListAsync();

        return Ok(assets);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> AssignAsset(int id, [FromBody] AssignAssetRequest request)
    {
        var asset = await _dbContext.Assets.SingleOrDefaultAsync(a => a.Id == id);
        if (asset is null)
        {
            return NotFound("Varlık bulunamadı.");
        }

        if (request.EmployeeId.HasValue)
        {
            var employeeExists = await _dbContext.Employees.AnyAsync(e => e.Id == request.EmployeeId.Value);
            if (!employeeExists)
            {
                return BadRequest("Çalışan bulunamadı.");
            }
        }

        asset.EmployeeId = request.EmployeeId;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    public sealed class AssignAssetRequest
    {
        public int? EmployeeId { get; set; }
    }

    public sealed class AssetListItem
    {
        public string Id { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public EmployeeItem? Employee { get; set; }
    }

    public sealed class DepartmentItem
    {
        public int Id { get; set; }
        public string DeptName { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public sealed class EmployeeItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public DepartmentItem? Department { get; set; }
    }
}
