using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Law_Hub.Models
{
    public class Clients_Profile
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string OtherNames { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string Email { get; set; }
        public string Phone_Number { get; set; }
        public string ProfilePicture { get; set; }
    }
}
