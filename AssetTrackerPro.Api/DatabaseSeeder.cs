using AssetTrackerPro.Domain.Entities;
using AssetTrackerPro.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerPro.Api;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AssetTrackerDbContext>();
        await context.Database.MigrateAsync();

        if (!await context.Departments.AnyAsync())
        {
            var itDept = new Departments { DeptName = "Bilgi Teknolojileri", Location = "İstanbul" };
            var financeDept = new Departments { DeptName = "Finans", Location = "Ankara" };
            var hrDept = new Departments { DeptName = "İnsan Kaynakları", Location = "İzmir" };
            var opsDept = new Departments { DeptName = "Operasyon", Location = "Bursa" };
            context.Departments.AddRange(itDept, financeDept, hrDept, opsDept);
            await context.SaveChangesAsync();

            var employees = new[]
            {
                new Employees { FullName = "Ahmet Yılmaz", Email = "ahmet.yilmaz@orn.com", DepartmentId = itDept.Id },
                new Employees { FullName = "Elif Kaya", Email = "elif.kaya@orn.com", DepartmentId = financeDept.Id },
                new Employees { FullName = "Mehmet Arslan", Email = "mehmet.arslan@orn.com", DepartmentId = itDept.Id },
                new Employees { FullName = "Ayse Demir", Email = "ayse.demir@orn.com", DepartmentId = hrDept.Id },
                new Employees { FullName = "Emre Aydın", Email = "emre.aydin@orn.com", DepartmentId = opsDept.Id },
                new Employees { FullName = "Zeynep Çelik", Email = "zeynep.celik@orn.com", DepartmentId = financeDept.Id }
            };
            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();

            var assets = new[]
            {
                new Assets
                {
                    AssetName = "Dell Latitude 5520",
                    SerialNumber = "SN-IT-0001",
                    AssetType = "Dizüstü Bilgisayar",
                    PurchaseDate = new DateTime(2023, 2, 15),
                    EmployeeId = employees[0].Id
                },
                new Assets
                {
                    AssetName = "HP ProDesk 400",
                    SerialNumber = "SN-FIN-0001",
                    AssetType = "Masaüstü Bilgisayar",
                    PurchaseDate = new DateTime(2022, 11, 5),
                    EmployeeId = employees[1].Id
                },
                new Assets
                {
                    AssetName = "MacBook Pro 14",
                    SerialNumber = "SN-IT-0003",
                    AssetType = "Dizüstü Bilgisayar",
                    PurchaseDate = new DateTime(2024, 1, 20),
                    EmployeeId = employees[2].Id
                },
                new Assets
                {
                    AssetName = "Lenovo ThinkPad E14",
                    SerialNumber = "SN-HR-0001",
                    AssetType = "Dizüstü Bilgisayar",
                    PurchaseDate = new DateTime(2023, 7, 3),
                    EmployeeId = employees[3].Id
                },
                new Assets
                {
                    AssetName = "Samsung Galaxy S22",
                    SerialNumber = "SN-OPS-0001",
                    AssetType = "Telefon",
                    PurchaseDate = new DateTime(2022, 9, 18),
                    EmployeeId = employees[4].Id
                },
                new Assets
                {
                    AssetName = "Canon MF237",
                    SerialNumber = "SN-OPS-0002",
                    AssetType = "Yazıcı",
                    PurchaseDate = new DateTime(2021, 3, 22),
                    EmployeeId = null
                },
                new Assets
                {
                    AssetName = "iPhone 13",
                    SerialNumber = "SN-FIN-0002",
                    AssetType = "Telefon",
                    PurchaseDate = new DateTime(2022, 12, 1),
                    EmployeeId = employees[5].Id
                }
            };
            context.Assets.AddRange(assets);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var hasher = new PasswordHasher<Users>();
            var admin = new Users { Username = "admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "admin123");
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
