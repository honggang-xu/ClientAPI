using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAPI.Core.Dtos
{
    public class ClientDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateBecameCustomer { get; set; }
    }
}
