using System.Collections.Generic;
using System;
using Xunit;
using ClientAPI.Core.Entities;
using ClientAPI.EntityFramework.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ClientAPI.Web.Controllers;

namespace UnitTest
{
    public class ClientControllerUnitTests : IDisposable
    {
        private readonly DbContextOptions<ClientDbContext> _dbContextOptions;

        public ClientControllerUnitTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ClientDbContext>()
                .UseInMemoryDatabase(databaseName: "TestClientControllerDb")
                .Options;
        }

        public void Dispose()
        {
            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                dbContext.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void GetClients_ReturnsListOfClients()
        {
            // Arrange
            var clients = new List<ClientEntity>
            {
                new ClientEntity
                {
                    Id = 1,
                    Name = "Client 1",
                    Email = "client1@test.com",
                    BecameCustomerDate = DateTime.UtcNow
                },
                new ClientEntity
                {
                    Id = 2,
                    Name = "Client 2",
                    Email = "client2@test.com",
                    BecameCustomerDate = DateTime.UtcNow
                }
            };

            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                dbContext.Clients.AddRange(clients);
                dbContext.SaveChanges();
            }

            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                var controller = new ClientsController(dbContext);

                // Act
                var result = controller.GetClients();

                // Assert
                Assert.IsType<OkObjectResult>(result);

                var okResult = result as OkObjectResult;
                Assert.NotNull(okResult);

                var returnedClients = okResult.Value as List<ClientEntity>;
                Assert.NotNull(returnedClients);
                Assert.Equal(clients.Count, returnedClients.Count);

                foreach (var client in clients)
                {
                    var matchingClient = returnedClients.FirstOrDefault(c => c.Id == client.Id);
                    Assert.NotNull(matchingClient);
                    Assert.Equal(client.Name, matchingClient.Name);
                    Assert.Equal(client.Email, matchingClient.Email);
                    Assert.Equal(client.BecameCustomerDate, matchingClient.BecameCustomerDate);
                }
            }
        }

        [Fact]
        public void GetClient_ExistingId_ReturnsClient()
        {
            // Arrange
            var clientId = 1;
            var client = new ClientEntity
            {
                Id = clientId,
                Name = "Client 1",
                Email = "client1@test.com",
                BecameCustomerDate = DateTime.UtcNow
            };

            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                dbContext.Clients.Add(client);
                dbContext.SaveChanges();
            }

            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                var controller = new ClientsController(dbContext);

                // Act
                var result = controller.GetClient(clientId);

                // Assert
                Assert.IsType<OkObjectResult>(result);

                var okResult = result as OkObjectResult;
                Assert.NotNull(okResult);

                var returnedClient = okResult.Value as ClientEntity;
                Assert.NotNull(returnedClient);
                Assert.Equal(client.Name, returnedClient.Name);
                Assert.Equal(client.Email, returnedClient.Email);
                Assert.Equal(client.BecameCustomerDate, returnedClient.BecameCustomerDate);
            }
        }

        [Fact]
        public void CreateClient_ValidData_ReturnsCreatedResponse()
        {
            // Arrange
            var client = new ClientEntity
            {
                Name = "New Client",
                Email = "newclient@test.com",
                BecameCustomerDate = DateTime.UtcNow
            };

            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                var controller = new ClientsController(dbContext);

                // Act
                var result = controller.CreateClient(client);

                // Assert
                Assert.IsType<CreatedAtActionResult>(result);

                var createdAtActionResult = result as CreatedAtActionResult;
                Assert.NotNull(createdAtActionResult);

                Assert.Equal(nameof(ClientsController.GetClient), createdAtActionResult.ActionName);
                Assert.Equal(client.Id, createdAtActionResult.RouteValues["id"]);
                Assert.Equal(client, createdAtActionResult.Value);

                using (var dbContextForAssertion = new ClientDbContext(_dbContextOptions))
                {
                    var createdClient = dbContextForAssertion.Clients.FirstOrDefault(c => c.Id == client.Id);
                    Assert.NotNull(createdClient);
                    Assert.Equal(client.Name, createdClient.Name);
                    Assert.Equal(client.Email, createdClient.Email);
                    Assert.Equal(client.BecameCustomerDate, createdClient.BecameCustomerDate);
                }
            }
        }

        [Fact]
        public void UpdateClient_ExistingId_ValidData_ReturnsNoContent()
        {
            // Arrange
            var clientId = 1;
            var existingClient = new ClientEntity
            {
                Id = clientId,
                Name = "Client 1",
                Email = "client1@test.com",
                BecameCustomerDate = DateTime.UtcNow
            };

            var updatedClient = new ClientEntity
            {
                Id = clientId,
                Name = "New Client 1",
                Email = "newclient1@test.com",
                BecameCustomerDate = DateTime.UtcNow
            };

            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                dbContext.Clients.Add(existingClient);
                dbContext.SaveChanges();
            }

            using (var dbContext = new ClientDbContext(_dbContextOptions))
            {
                var controller = new ClientsController(dbContext);

                // Act
                var result = controller.UpdateClient(clientId, updatedClient);

                // Assert
                Assert.IsType<NoContentResult>(result);

                using (var dbContextForAssertion = new ClientDbContext(_dbContextOptions))
                {
                    var updatedClientFromDb = dbContextForAssertion.Clients.FirstOrDefault(c => c.Id == clientId);
                    Assert.NotNull(updatedClientFromDb);
                    Assert.Equal(updatedClient.Name, updatedClientFromDb.Name);
                    Assert.Equal(updatedClient.Email, updatedClientFromDb.Email);
                    Assert.Equal(updatedClient.BecameCustomerDate, updatedClientFromDb.BecameCustomerDate);
                }
            }
        }

        [Fact]
        public void DeleteClient_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var clientId = 1;
            var client = new ClientEntity
            {
                Id = clientId,
                Name = "Client 1",
                Email = "client@test.com",
                BecameCustomerDate = DateTime.UtcNow
            };

            var dbContextOptions = new DbContextOptionsBuilder<ClientDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var dbContext = new ClientDbContext(dbContextOptions))
            {
                dbContext.Clients.Add(client);
                dbContext.SaveChanges();
            }

            using (var dbContext = new ClientDbContext(dbContextOptions))
            {
                var controller = new ClientsController(dbContext);

                // Act
                var result = controller.DeleteClient(clientId);

                // Assert
                Assert.IsType<NoContentResult>(result);

                using (var dbContextForAssertion = new ClientDbContext(dbContextOptions))
                {
                    var deletedClient = dbContextForAssertion.Clients.FirstOrDefault(c => c.Id == clientId);
                    Assert.Null(deletedClient);
                }
            }
        }
    }
}