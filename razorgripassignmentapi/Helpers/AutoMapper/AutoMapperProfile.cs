using System;
using AutoMapper;
using razorgripassignmentapi.Data.DTOs;
using razorgripassignmentapi.Data.Models;

namespace razorgripassignmentapi.Helpers.AutoMapper
{
    public class AutoMapperProfile:Profile
    {

        public AutoMapperProfile()
        {
            UserMappings();

        }


        void UserMappings()
        {
            CreateMap<UserForRegisterDTO, ApplicationUser>();


            CreateMap<ApplicationUser, UserToReturnDTO>();


            CreateMap<UserForLoginDTO, ApplicationUser>();
        }
    }


   

}
