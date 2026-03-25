using FoodSafety.Domain;
using FoodSafety.MVC.Controllers;
using FoodSafety.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoodSafety.Test
{
    public class FollowupsControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewWithListOfFollowUps()
        {
            // Arrange
            var dbContext = TestDbContextFactory.Create();
            var loggerMock = new Mock<ILogger<FollowupsController>>();
            var controller = new FollowupsController(dbContext, loggerMock.Object);

            dbContext.FollowUps.Add(new FollowUp { Id = 1, InspectionId = 1, DueDate = DateTime.Today, Status = "Open" });
            await dbContext.SaveChangesAsync();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<FollowUp>>(viewResult.Model);
            Assert.Single(model);
        }
    }
}
