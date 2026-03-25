using Microsoft.EntityFrameworkCore;          
using Microsoft.Extensions.Logging;
using Moq;
using FoodSafety.MVC.Controllers;
using FoodSafety.MVC.Data;
using FoodSafety.Domain;
using FoodSafety.MVC.ViewModels;


namespace FoodSafety.MVC.Data
{
    public static class TestDbContextFactory
    {
        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
    }

}