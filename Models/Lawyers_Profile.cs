using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Law_Hub.Models
{
    public class Lawyers_Profile
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Other_Titles { get; set; }
        public string First_Name { get; set; }
        public string Other_Names { get; set; }
        public string Last_Name { get; set; }
        public string Sex { get; set; }
        public string Maiden_Name { get; set; }
        public string Phone_Number { get; set; }
        public string Email { get; set; }
        public string Office_Address { get; set; }
        public string Experiences { get; set; }
        [NotMapped]
        public IFormFile Certificates_Paths { get; set; }
        public string Certificate_path { get; set; }
        public string ProfilePicture { get; set; }
    }
}
