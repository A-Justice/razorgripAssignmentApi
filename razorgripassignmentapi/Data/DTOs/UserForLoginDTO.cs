using System;
using System.ComponentModel.DataAnnotations;

namespace razorgripassignmentapi.Data.DTOs
{
    public class UserForLoginDTO
    {
        [Required]
        public string UserNameOrEmail { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
