using AssetTrackerPro.Domain.Entities;
using AssetTrackerPro.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AssetTrackerDbContext _dbContext;

    public EmployeesController(AssetTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeItem>> Create([FromBody] CreateEmployeeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("Ad soyad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("E-posta zorunludur.");
        }

        var departmentExists = await _dbContext.Departments.AnyAsync(d => d.Id == request.DepartmentId);
        if (!departmentExists)
        {
            return BadRequest("Departman bulunamadı.");
        }

        var emailExists = await _dbContext.Employees.AnyAsync(e => e.Email == request.Email);
        if (emailExists)
        {
            return Conflict("E-posta benzersiz olmalıdır.");
        }

        var employee = new Employees
        {
            FullName = request.FullName,
            Email = request.Email,
            DepartmentId = request.DepartmentId
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var response = new EmployeeItem
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            DepartmentId = employee.DepartmentId
        };

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeItem>> GetById(int id)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound("Çalışan bulunamadı.");
        }

        return Ok(new EmployeeItem
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            DepartmentId = employee.DepartmentId
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmployeeItem>> Update(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var employee = await _dbContext.Employees.SingleOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return NotFound("Çalışan bulunamadı.");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            employee.FullName = request.FullName;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != employee.Email)
        {
            var emailExists = await _dbContext.Employees.AnyAsync(e => e.Email == request.Email && e.Id != id);
            if (emailExists)
            {
                return Conflict("E-posta benzersiz olmalıdır.");
            }

            employee.Email = request.Email;
        }

        if (request.DepartmentId.HasValue && request.DepartmentId.Value != employee.DepartmentId)
        {
            var departmentExists = await _dbContext.Departments.AnyAsync(d => d.Id == request.DepartmentId.Value);
            if (!departmentExists)
            {
                return BadRequest("Departman bulunamadı.");
            }

            employee.DepartmentId = request.DepartmentId.Value;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new EmployeeItem
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            DepartmentId = employee.DepartmentId
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _dbContext.Employees.SingleOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return NotFound("Çalışan bulunamadı.");
        }

        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    public sealed class CreateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }

    public sealed class UpdateEmployeeRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int? DepartmentId { get; set; }
    }

    public sealed class EmployeeItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }
}
