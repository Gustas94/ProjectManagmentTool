using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ProjectManagmentTool.Controllers;
using ProjectManagmentTool.Data;

namespace Tests.Unit.Controllers
{
    public class AdminControllerTests
    {
        [Fact]
        public void GetAllUsers_ReturnsOkResult_WithUserList()
        {
            // Arrange: Create sample user list
            var sampleUsers = new List<User>
            {
                new User { Id = "1", FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", CompanyID = 1 },
                new User { Id = "2", FirstName = "Bob", LastName = "Jones", Email = "bob@example.com", CompanyID = 1 }
            }.AsQueryable();

            // Create a mock for the IUserStore<User> and then for UserManager<User>
            var store = new Mock<IUserStore<User>>();
            var mockUserManager = new Mock<UserManager<User>>(
                store.Object,
                null, null, null, null, null, null, null, null);

            // Setup the Users property on UserManager to return our sample user list
            mockUserManager.Setup(um => um.Users).Returns(sampleUsers);

            // Create a mock for RoleManager<Role>
            var mockRoleStore = new Mock<IRoleStore<Role>>();
            var mockRoleManager = new Mock<RoleManager<Role>>(
                mockRoleStore.Object,
                null, null, null, null);

            // Instead of mocking ApplicationDbContext, create a real instance
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDbForUnit")
                .Options;
            var realContext = new ApplicationDbContext(options);

            // (Optional) Seed any data into realContext if your controller methods require it

            // Create an instance of AdminController with dependencies injected
            var controller = new AdminController(
                mockUserManager.Object,
                mockRoleManager.Object,
                realContext
            );

            // Act: Call the GetAllUsers method
            var result = controller.GetAllUsers();

            // Assert: Verify the result is of type OkObjectResult and contains 2 users.
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }
    }
}
