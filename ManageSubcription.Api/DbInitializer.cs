using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Core.ManageSubcription.Helpers;
using Spine.Core.ManageSubcription.ViewModel;
using Spine.Data;
using Spine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageSubcription.Api
{
    public class DbInitializer
    {
        //private readonly IConfiguration configuration;


        public static void Initialize(SpineContext context, IServiceProvider services, IMapper mapper, 
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            // Get a logger
            var logger = services.GetRequiredService<ILogger<DbInitializer>>();

            // Make sure the database is created
            // We already did this in the previous step
            context.Database.EnsureCreated();

            var model = new CompanyParam
            {
                FirstName = "Supper-Admin",
                LastName = "Admin",
                Email = StaticConfiguration.AppSetting["Supper-Admin:Email"],
                BusinessName = "Admin",
                BusinessType = "Admin",
                PhoneNumber = "080xxxxxxxx",
                OperatingSector = "Admin",
            };

            var User = context.Users.FirstOrDefault(p => p.Email == model.Email);
            if (User == null)
            {
                logger.LogInformation("Start seeding the database.");

                var password = StaticConfiguration.AppSetting["Supper-Admin:Password"];
                var ownerRole = context.Roles.FirstOrDefault(x => x.IsOwnerRole && !x.IsDeleted);
                //var adminRole = new IdentityRole("SupperAdmin");
                //if (!context.Roles.Any())
                //{
                //    roleManager.CreateAsync(adminRole).GetAwaiter().GetResult();
                //}
                var user = mapper.Map<ApplicationUser>(model);
                user.IsBusinessOwner = true;
                user.EmailConfirmed = true;
                user.CompanyId = user.Id;
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.RoleId = ownerRole == null ? Guid.Empty : ownerRole.Id;
                user.DateOfBirth = DateTime.Now;
                user.CreatedBy = user.Id;
                user.CreatedOn = Constants.GetCurrentDateTime();
                var create = Task.Run(() => userManager.CreateAsync(user, password)).Result;
                if (!create.Succeeded)
                {
                    logger.LogInformation("Unable to complete signup, Please try again");
                }
                if (ownerRole != null)
                {
                    var respon = Task.Run(() => userManager.AddToRoleAsync(user, ownerRole.Name)).Result;
                }


                logger.LogInformation("Finished seeding the database.");
            }
            else
            {
                logger.LogInformation("The database was already seeded");
                return;
            }

            
        }
    }
}
