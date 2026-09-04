using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Celtic.Api.Controllers;
using Celtic.Api.DTOs;
using Celtic.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Celtic.UnitTests.Controllers;

public class TeamsControllerTests
{
    private TeamsController CreateController(ITeamService service, string role = "Admin")
    {
        var controller = new TeamsController(service);
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        return controller;
    }

    [Fact]
    public async Task GetTeams_ReturnsOkWithList()
    {
        // Arrange
        var mockService = new Mock<ITeamService>();
        mockService.Setup(s => s.GetAllTeamsAsync())
            .ReturnsAsync(new List<TeamDto>
            {
                new TeamDto(Guid.NewGuid(), "Stripes", "#006837", true, 6),
                new TeamDto(Guid.NewGuid(), "Hoops", "#F59E0B", true, 6)
            });

        var controller = CreateController(mockService.Object);

        // Act
        var result = await controller.GetTeams();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<List<TeamDto>>(okResult.Value);
        Assert.Equal(2, returned.Count);
    }

    [Fact]
    public async Task CreateTeam_NonAdmin_Returns403()
    {
        // Arrange
        var mockService = new Mock<ITeamService>();
        var controller = CreateController(mockService.Object, role: "User");

        // Act
        var result = await controller.CreateTeam(new CreateTeamRequest { Name = "New Team" });

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }
}
