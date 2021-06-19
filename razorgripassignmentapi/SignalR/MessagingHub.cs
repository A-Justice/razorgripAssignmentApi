using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using razorgripassignmentapi.Data.DTOs;
using razorgripassignmentapi.Data.Models;
using System.Linq;
using razorgripassignmentapi.Data.DbContext;

namespace razorgripassignmentapi.SignalR
{
    public class MessagingHub:Hub
    {
        public object locker { get; set; } = new object();

        public UserManager<ApplicationUser> UserManager { get; set; }
        public static Dictionary<string, UserToReturnDTO> usersListDictionary = new Dictionary<string, UserToReturnDTO>();
        public IMapper Mapper { get; set; }
        public RazorgripDbContext DbContext { get; set; }
        public MessagingHub(UserManager<ApplicationUser> userManager, IMapper mapper,RazorgripDbContext dbContext)
        {
            UserManager = userManager;
            Mapper = mapper;
            DbContext = dbContext;
        }

        public async Task SendMessage(Message message)
        {
            try
            {
                await Clients.User(message.ReceiverId).SendAsync("ReceiveMessage", message);
                await DbContext.Messages.AddAsync(message);
                await DbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                 
            }
           
        }

        public async Task BlockUser(string userToBlockId)
        {
            var id = Context.UserIdentifier;
            if (id != null)
            {
                var user = DbContext.Users.Find(id);
                if (!user.BlockedIds.Contains($"{userToBlockId},"))
                {
                    user.BlockedIds += $"{userToBlockId},";
                    await DbContext.SaveChangesAsync();
                }
                
                var userForReturn = Mapper.Map<UserToReturnDTO>(user);
                if (userForReturn!=null && usersListDictionary.ContainsKey(id))
                {
                    usersListDictionary.Remove(id);
                    usersListDictionary.Add(id, userForReturn);
                    List<UserToReturnDTO> list = usersListDictionary.Values.ToList();
                    await Clients.All.SendAsync("UserJoined", list);
                }
            }           
        }

        public async Task UnBlockUser(string userToBlockId)
        {
            var id = Context.UserIdentifier;
            if (id != null)
            {
                var user = DbContext.Users.Find(id);
                user.BlockedIds = user.BlockedIds.Replace($"{userToBlockId},","");
                await DbContext.SaveChangesAsync();
                var userForReturn = Mapper.Map<UserToReturnDTO>(user);
                if (userForReturn != null && usersListDictionary.ContainsKey(id))
                {
                    usersListDictionary.Remove(id);
                    usersListDictionary.Add(id, userForReturn);
                    List<UserToReturnDTO> list = usersListDictionary.Values.ToList();
                    await Clients.All.SendAsync("UserJoined", list);
                }
            }
        }


        public override async Task OnConnectedAsync()
        {
            
            var id = Context.UserIdentifier;
            if (id != null)
            {
                var User = await UserManager.FindByIdAsync(id);
                var userForReturn = Mapper.Map<UserToReturnDTO>(User);
                if (userForReturn!=null && !usersListDictionary.ContainsKey(id)){
                    lock (locker) {
                        usersListDictionary.Add(id, userForReturn);
                    }
                }

                List<UserToReturnDTO> list = usersListDictionary.Values.ToList();
                await Clients.All.SendAsync("UserJoined", list);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var id = Context.UserIdentifier;
            if (id != null && usersListDictionary.ContainsKey(id))
            {
                lock (locker)
                {
                    usersListDictionary.Remove(id);
                }
                
                List<UserToReturnDTO> list = usersListDictionary.Values.ToList();
                await Clients.All.SendAsync("UserLeft", list);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task GetInitialMessagesBetweenTwoParty(string requesterId,string secondId)
        {
            try
            {
                var firstBatch = DbContext.Messages.Where(i => i.SenderId == requesterId && i.ReceiverId == secondId).ToList();
                var secondBatch = DbContext.Messages.Where(i => i.SenderId == secondId && i.ReceiverId == requesterId).ToList();

                firstBatch.AddRange(secondBatch);
                await Clients.Caller.SendAsync("ReceiveMessages", firstBatch);
                //await Clients.User(requesterId).SendAsync("ReceiveMessages", firstBatch);

            }
            catch
            {
                
            }
        }

    }
}
