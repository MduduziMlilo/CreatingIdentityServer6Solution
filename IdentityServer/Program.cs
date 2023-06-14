using System.Reflection;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Options;
using IdentityServer.Data;
using IdentityServer.Factories;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(optionsAction: (IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder) =>
{
    dbContextOptionsBuilder
        .UseNpgsql(
            serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(name: "Identity"), 
            npgsqlOptionsAction: NpgsqlOptionsAction);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipleFactory>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<ApplicationUser>()
    .AddConfigurationStore(configurationStoreOptions =>
    {
        configurationStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
    } )
    .AddOperationalStore(operationalStoreOptions =>
    {
        operationalStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
    } );

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (await userManager.FindByNameAsync("CitizenOne") == null)
    {
        await userManager.CreateAsync(new ApplicationUser
        {
            UserName = "CitizenOne",
            Email = "citizenone@pymath.ai",
            GivenName = "Citizen",
            FamilyName = "One",
        }, password: "Pa55w0rd!");
    }

    var configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    if (!await configurationDbContext.ApiResources.AnyAsync())
    {
        await configurationDbContext.ApiResources.AddAsync(new ApiResource());      
    }
}

app.Run();

void NpgsqlOptionsAction(NpgsqlDbContextOptionsBuilder npgsqlDbContextOptionsBuilder)
{
    npgsqlDbContextOptionsBuilder.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
}

void ResolveDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder)
{
    dbContextOptionsBuilder.UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>()
        .GetConnectionString("IdentityServer"), npgsqlOptionsAction: NpgsqlOptionsAction);
}