using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Law_Hub.Models
{
    public class ProfilePictureViewModel
    {
        [NotMapped]
        public IFormFile ProfilePicture { get; set; }
        public string Picture_Path { get; set; }
    }
}
