using ClientAPI.Core.Entities;
using ClientAPI.EntityFramework.Data.Context;
using Microsoft.AspNetCore.Mvc;

namespace ClientAPI.Web.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly ClientDbContext _clientDbContext;

        public ClientsController(ClientDbContext dbContext)
        {
            _clientDbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetClients()
        {
            var clients = _clientDbContext.Clients.ToList();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public IActionResult GetClient(int id)
        {
            var client = _clientDbContext.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
                return NotFound();

            return Ok(client);
        }

        [HttpPost]
        public IActionResult CreateClient(ClientEntity client)
        {
            _clientDbContext.Clients.Add(client);
            _clientDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClient(int id, ClientEntity updatedClient)
        {
            var client = _clientDbContext.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
                return NotFound();

            client.Name = updatedClient.Name;
            client.Email = updatedClient.Email;
            client.BecameCustomerDate = updatedClient.BecameCustomerDate;
            _clientDbContext.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            var client = _clientDbContext.Clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
                return NotFound();

            _clientDbContext.Clients.Remove(client);
            _clientDbContext.SaveChanges();

            return NoContent();
        }
    }
}
