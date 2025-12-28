using AssetTrackerPro.Domain.Entities;
using AssetTrackerPro.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly AssetTrackerDbContext _dbContext;

    public DepartmentsController(AssetTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentWithEmployees>>> GetAll()
    {
        var departments = await _dbContext.Departments
            .AsNoTracking()
            .Include(d => d.Employees)
            .Select(d => new DepartmentWithEmployees
            {
                Id = d.Id,
                DeptName = d.DeptName,
                Location = d.Location,
                Employees = d.Employees.Select(e => new EmployeeItem
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email
                }).ToList()
            })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("{id:int}/employees")]
    public async Task<ActionResult<IEnumerable<EmployeeItem>>> GetEmployees(int id)
    {
        var exists = await _dbContext.Departments.AnyAsync(d => d.Id == id);
        if (!exists)
        {
            return NotFound("Departman bulunamadı.");
        }

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.DepartmentId == id)
            .Select(e => new EmployeeItem
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentWithEmployees>> Create([FromBody] CreateDepartmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DeptName))
        {
            return BadRequest("Departman adı zorunludur.");
        }

        var exists = await _dbContext.Departments.AnyAsync(d => d.DeptName == request.DeptName);
        if (exists)
        {
            return Conflict("Departman adı benzersiz olmalıdır.");
        }

        var department = new Departments
        {
            DeptName = request.DeptName,
            Location = request.Location
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();

        var response = new DepartmentWithEmployees
        {
            Id = department.Id,
            DeptName = department.DeptName,
            Location = department.Location,
            Employees = new List<EmployeeItem>()
        };

        return CreatedAtAction(nameof(GetAll), new { id = department.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentWithEmployees>> Update(int id, [FromBody] UpdateDepartmentRequest request)
    {
        var department = await _dbContext.Departments
            .Include(d => d.Employees)
            .SingleOrDefaultAsync(d => d.Id == id);

        if (department is null)
        {
            return NotFound("Departman bulunamadı.");
        }

        if (!string.IsNullOrWhiteSpace(request.DeptName) && request.DeptName != department.DeptName)
        {
            var nameExists = await _dbContext.Departments.AnyAsync(d => d.DeptName == request.DeptName && d.Id != id);
            if (nameExists)
            {
                return Conflict("Departman adı benzersiz olmalıdır.");
            }

            department.DeptName = request.DeptName;
        }

        if (request.Location is not null)
        {
            department.Location = request.Location;
        }

        await _dbContext.SaveChangesAsync();

        var response = new DepartmentWithEmployees
        {
            Id = department.Id,
            DeptName = department.DeptName,
            Location = department.Location,
            Employees = department.Employees.Select(e => new EmployeeItem
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email
            }).ToList()
        };

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _dbContext.Departments.SingleOrDefaultAsync(d => d.Id == id);
        if (department is null)
        {
            return NotFound("Departman bulunamadı.");
        }

        _dbContext.Departments.Remove(department);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    public sealed class CreateDepartmentRequest
    {
        public string DeptName { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public sealed class UpdateDepartmentRequest
    {
        public string? DeptName { get; set; }
        public string? Location { get; set; }
    }

    public sealed class DepartmentWithEmployees
    {
        public int Id { get; set; }
        public string DeptName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public List<EmployeeItem> Employees { get; set; } = new();
    }

    public sealed class EmployeeItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
