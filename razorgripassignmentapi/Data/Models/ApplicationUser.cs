using System;
using Microsoft.AspNetCore.Identity;

namespace razorgripassignmentapi.Data.Models
{
    public class ApplicationUser:IdentityUser
    {
        //set the blocked Ids as comma seperated list
        //Remove you search for the index of the id in the comma seperated list and remove with the , that follows
        //To add .. append the Id and add ,
        public string BlockedIds { get; set; } = "";

    }
}
