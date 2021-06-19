using System;
namespace razorgripassignmentapi.Data.DTOs
{
    public class UserToReturnDTO
    {
        public virtual string Id { get; set; }

        public virtual string UserName { get; set; }

        public string BlockedIds { get; set; } 

    }
}
