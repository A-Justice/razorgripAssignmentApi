using System;
using System.ComponentModel.DataAnnotations;

namespace razorgripassignmentapi.Data.DTOs
{
    public class UserForRegisterDTO
    {
        [Required]
        [MinLength(4)]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
