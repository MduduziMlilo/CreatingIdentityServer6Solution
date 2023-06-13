using System.Reflection;
using IdentityServer.Data;
using IdentityServer.Factories;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(optionsAction: (serviceProvider, dbContextOptionsBuilder) =>
{
    dbContextOptionsBuilder
        .UseNpgsql(
            serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(name: "Identity"), 
            npgsqlOptionsAction: npgsqlDbContextOptionsBuilder => {
                npgsqlDbContextOptionsBuilder.MigrationsAssembly(
                    typeof(Program).GetTypeInfo().Assembly.GetName().Name);
            });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipleFactory>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
var app = builder.Build();

app.Run();