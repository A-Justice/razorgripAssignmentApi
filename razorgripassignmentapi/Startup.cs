using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using razorgripassignmentapi.Data.DbContext;
using razorgripassignmentapi.Helpers;
using razorgripassignmentapi.Data.Models;
using Microsoft.Extensions.Hosting;
using razorgripassignmentapi.SignalR;
using Microsoft.AspNetCore.Http.Connections;
using AutoMapper;
using razorgripassignmentapi.Helpers.AutoMapper;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace razorgripassignmentapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("rgripdefault", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

            //GearHost
            services.AddDbContext<RazorgripDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Azure"), i => i.EnableRetryOnFailure()), ServiceLifetime.Transient);

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("Personal");

            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();

            services.AddMvc(/*mvcOptions => mvcOptions.Filters.Add(new CorsHeaderFilter())*/)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddAutoMapper(c => c.AddProfile<AutoMapperProfile>(), typeof(Startup));


            //.AddJsonOptions(options =>
            //{
            //    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //});

            services.AddStackExchangeRedisCache(options=> {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = "razorgripassignment";
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<RazorgripDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            ).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    // ValidIssuer = Configuration["Issuer"],
                    ValidateAudience = false,
                    // ValidAudience = Configuration["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Settings.Secret))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Path.Value.StartsWith("/signalr/messaging") && context.Request.Query.TryGetValue("token", out StringValues token))
                        {
                           
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var exception = context.Exception;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromHours(1);
            });

            services.AddSession();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseCors("rgripdefault");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<MessagingHub>("signalr/messaging", options =>
                {
                    options.Transports =
                        HttpTransportType.WebSockets |
                        HttpTransportType.LongPolling;
                });
            });


            //app.UseHttpsRedirection();

            app.UseRouting();

           

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

           
        }
    }
}
