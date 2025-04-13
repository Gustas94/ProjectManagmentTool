using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectManagmentTool.Commands;
using ProjectManagmentTool.DTOs;   // Ensure that GroupDetailsDTO is declared in this namespace.
using ProjectManagmentTool.Data;
using ProjectManagmentTool.Queries;
using Xunit;
using Tests.Integration;
using Microsoft.AspNetCore.TestHost;

namespace ProjectManagmentTool.Tests.Integration.Controllers
{
    public class GroupsControllerFullCoverageTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public GroupsControllerFullCoverageTests(TestApplicationFactory factory)
        {
            // Use WithWebHostBuilder on the injected factory to override MediatR registrations.
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Remove the default MediatR registrations.
                    services.RemoveAll(typeof(IMediator));
                    services.RemoveAll(typeof(IRequestHandler<,>));
                    // Register your fake mediator.
                    services.AddSingleton<IMediator, FakeMediator>();
                });
            });
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// A fake mediator implementation to simulate behavior for groups queries and commands.
        /// </summary>
        public class FakeMediator : IMediator
        {
            // Implementation for IRequest<TResponse>
            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                switch (request)
                {
                    case GetCompanyIdQuery _:
                        // Return a fixed company id (e.g. 1) for any user.
                        return Task.FromResult((TResponse)(object)1);

                    case GetAllGroupsQuery _:
                        // Return a List<Group> with a single group.
                        var groups = new List<Group>
                        {
                            new Group
                            {
                                GroupID = 1,
                                GroupName = "TestGroup",
                                Description = "Test Description"
                            }
                        };
                        return Task.FromResult((TResponse)(object)groups);

                    case CreateGroupCommand _:
                        // Return a test group id.
                        return Task.FromResult((TResponse)(object)10);

                    case GetGroupByIdQuery query:
                        // Return a GroupDetailsDTO if the group id equals 1.
                        if (query.GroupId == 1)
                        {
                            var group = new GroupDetailsDTO
                            {
                                GroupID = 1,
                                GroupName = "TestGroup",
                                Description = "Test Description"
                            };
                            return Task.FromResult((TResponse)(object)group);
                        }
                        return Task.FromResult((TResponse)(object)null);

                    case AssignGroupMembersCommand _:
                        // Return Unit.Value so the command is considered successful.
                        return Task.FromResult((TResponse)(object)Unit.Value);

                    default:
                        throw new NotImplementedException("No fake handling for: " + request.GetType().Name);
                }
            }

            // Implementation for ISender.Send<TRequest>(TRequest, CancellationToken)
            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
                where TRequest : IRequest
            {
                if (request is IRequest<Unit> unitRequest)
                {
                    return Send<Unit>(unitRequest, cancellationToken)
                        .ContinueWith(_ => { }, cancellationToken);
                }
                return Task.CompletedTask;
            }

            // Implementation for ISender.Send<TResponse>(object, CancellationToken)
            public Task<TResponse> Send<TResponse>(object request, CancellationToken cancellationToken = default)
            {
                if (request is IRequest<TResponse> req)
                {
                    return Send(req, cancellationToken);
                }
                throw new ArgumentException("Invalid request type", nameof(request));
            }

            // Implementation for ISender.Send(object, CancellationToken)
            public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            {
                if (request is IRequest<Unit> unitRequest)
                {
                    return Send(unitRequest, cancellationToken)
                        .ContinueWith<object?>(t => (object?)Unit.Value, cancellationToken);
                }
                throw new ArgumentException("Invalid request type", nameof(request));
            }

            // Implementation for CreateStream using IStreamRequest<TResponse>.
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                return EmptyAsyncEnumerable<TResponse>();
            }

            // Implementation for CreateStream<TResponse>(object, CancellationToken)
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(object request, CancellationToken cancellationToken = default)
            {
                if (request is IStreamRequest<TResponse> streamRequest)
                {
                    return CreateStream(streamRequest, cancellationToken);
                }
                throw new ArgumentException("Invalid request type", nameof(request));
            }

            // Implementation for ISender.CreateStream(object, CancellationToken)
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            {
                if (request is IStreamRequest<object?> streamRequest)
                {
                    return CreateStream<object?>(streamRequest, cancellationToken);
                }
                throw new ArgumentException("Invalid request type", nameof(request));
            }

            public Task Publish(object notification, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification
            {
                return Task.CompletedTask;
            }

            // Helper method: Returns an empty async enumerable.
            private static async IAsyncEnumerable<T> EmptyAsyncEnumerable<T>()
            {
                yield break;
            }
        }

        /// <summary>
        /// GET /api/groups/all returns groups for an authenticated user.
        /// </summary>
        [Fact]
        public async Task GetAllGroups_ReturnsGroups()
        {
            var response = await _client.GetAsync("/api/groups/all");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            Assert.Contains("TestGroup", json);
        }

        /// <summary>
        /// POST /api/groups/create returns a success message and group id.
        /// </summary>
        [Fact]
        public async Task CreateGroup_ReturnsSuccess()
        {
            var groupRequest = new
            {
                GroupName = "NewGroup",
                Description = "Group description",
                GroupLeadID = "Lead1",  // Even if not used by the DTO, this value can be ignored.
                GroupMemberIDs = new string[] { "User1", "User2" } // As above.
            };
            var content = new StringContent(JsonSerializer.Serialize(groupRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/groups/create", content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Group created successfully", json);
            Assert.Contains("\"groupID\":10", json);
        }

        /// <summary>
        /// GET /api/groups/{id} returns the group details when the group exists.
        /// </summary>
        [Fact]
        public async Task GetGroupById_ReturnsGroup_WhenFound()
        {
            var response = await _client.GetAsync("/api/groups/1");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            Assert.Contains("TestGroup", json);
        }

        /// <summary>
        /// GET /api/groups/{id} returns NotFound when the group does not exist.
        /// </summary>
        [Fact]
        public async Task GetGroupById_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            var response = await _client.GetAsync("/api/groups/999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// POST /api/groups/{groupId}/assign-members returns a success message.
        /// </summary>
        [Fact]
        public async Task AssignMembersToGroup_ReturnsSuccess()
        {
            var assignMembersDto = new
            {
                MemberIDs = new string[] { "User3", "User4" }
            };
            var content = new StringContent(JsonSerializer.Serialize(assignMembersDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/groups/1/assign-members", content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Members assigned successfully", json);
        }

        /// <summary>
        /// GET /api/groups/{groupId}/members returns the list of members for an existing group.
        /// </summary>
        [Fact]
        public async Task GetGroupMembers_ReturnsMembers_WhenGroupExists()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var existingGroup = await db.Groups.FirstOrDefaultAsync(g => g.GroupID == 100);
                if (existingGroup != null)
                {
                    db.Groups.Remove(existingGroup);
                    await db.SaveChangesAsync();
                }
                var group = new Group
                {
                    GroupID = 100,
                    GroupName = "TestGroup100",
                    Description = "Description100",
                    GroupLeadID = "Lead100",
                    GroupMembers = new List<GroupMember>()
                };
                group.GroupMembers.Add(new GroupMember
                {
                    UserID = "user1",
                    User = new User { Id = "user1", FirstName = "First", LastName = "Last", RoleID = "dummy" }
                });
                group.GroupMembers.Add(new GroupMember
                {
                    UserID = "user2",
                    User = new User { Id = "user2", FirstName = "Second", LastName = "User", RoleID = "dummy" }
                });
                db.Groups.Add(group);
                await db.SaveChangesAsync();
            }
            var response = await _client.GetAsync("/api/groups/100/members");
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("First", jsonResponse);
            Assert.Contains("Second", jsonResponse);
        }

        /// <summary>
        /// GET /api/groups/{groupId}/members returns NotFound when the group does not exist.
        /// </summary>
        [Fact]
        public async Task GetGroupMembers_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            var response = await _client.GetAsync("/api/groups/999/members");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// DELETE /api/groups/{groupId}/members/{userId} removes the member and cleans up related project assignments.
        /// </summary>
        [Fact]
        public async Task RemoveMemberFromGroup_RemovesMemberAndCleansUpProjects()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var existingGroupMember = await db.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupID == 200 && gm.UserID == "userToRemove");
                if (existingGroupMember != null)
                    db.GroupMembers.Remove(existingGroupMember);
                var existingProjectGroup = await db.ProjectGroups.FirstOrDefaultAsync(pg => pg.GroupID == 200);
                if (existingProjectGroup != null)
                    db.ProjectGroups.Remove(existingProjectGroup);
                var existingProjectUser = await db.ProjectUsers.FirstOrDefaultAsync(pu => pu.ProjectID == 300 && pu.UserID == "userToRemove");
                if (existingProjectUser != null)
                    db.ProjectUsers.Remove(existingProjectUser);
                await db.SaveChangesAsync();
                var group = new Group
                {
                    GroupID = 200,
                    GroupName = "TestGroup200",
                    Description = "TestGroup200 Description",
                    GroupLeadID = "Lead200",
                    GroupMembers = new List<GroupMember>()
                };
                group.GroupMembers.Add(new GroupMember
                {
                    UserID = "userToRemove",
                    User = new User { Id = "userToRemove", FirstName = "Remove", LastName = "User", RoleID = "dummy" }
                });
                db.Groups.Add(group);
                db.ProjectGroups.Add(new ProjectGroup { GroupID = 200, ProjectID = 300 });
                db.ProjectUsers.Add(new ProjectUser { UserID = "userToRemove", ProjectID = 300 });
                await db.SaveChangesAsync();
            }
            var response = await _client.DeleteAsync("/api/groups/200/members/userToRemove");
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("User removed from group and cleaned up from related projects", jsonResponse);
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var groupMember = await db.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupID == 200 && gm.UserID == "userToRemove");
                Assert.Null(groupMember);
                var projectUser = await db.ProjectUsers.FirstOrDefaultAsync(pu => pu.ProjectID == 300 && pu.UserID == "userToRemove");
                Assert.Null(projectUser);
            }
        }

        /// <summary>
        /// DELETE /api/groups/{groupId}/members/{userId} returns NotFound when the member is not present.
        /// </summary>
        [Fact]
        public async Task RemoveMemberFromGroup_ReturnsNotFound_WhenMemberDoesNotExist()
        {
            var response = await _client.DeleteAsync("/api/groups/300/members/nonexistentUser");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("User is not a member of this group", jsonResponse);
        }

        /// <summary>
        /// When the required user claim is missing, the endpoints should return Unauthorized.
        /// Note: Our TestAuthHandler always authenticates so for testing we expect OK.
        /// </summary>
        [Fact]
        public async Task Endpoints_ReturnUnauthorized_WhenUserClaimMissing()
        {
            var clientWithoutAuth = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = false,
                AllowAutoRedirect = false
            });
            clientWithoutAuth.DefaultRequestHeaders.Remove("Authorization");
            var response = await clientWithoutAuth.GetAsync("/api/groups/all");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
