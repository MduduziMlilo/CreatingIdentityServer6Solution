using System.Reflection;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.EntityFramework.Options;
using Duende.IdentityServer.Models;
using IdentityServer.Data;
using IdentityServer.Factories;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ApiResource = Duende.IdentityServer.Models.ApiResource;
using ApiScope = Duende.IdentityServer.Models.ApiScope;
using Client = Duende.IdentityServer.Models.Client;
using Secret = Duende.IdentityServer.Models.Secret;

var builder = WebApplication.CreateBuilder(args: args);

builder.Services.AddDbContext<ApplicationDbContext>(optionsAction: (IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder) =>
{
    dbContextOptionsBuilder
        .UseNpgsql(
            connectionString: serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(name: "Identity"), 
            npgsqlOptionsAction: NpgsqlOptionsAction);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipleFactory>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<ApplicationUser>()
    .AddConfigurationStore(storeOptionsAction: configurationStoreOptions =>
    {
        configurationStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
    } )
    .AddOperationalStore(storeOptionsAction: operationalStoreOptions =>
    {
        operationalStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
    } );

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.MapRazorPages();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (await userManager.FindByNameAsync(userName: "CitizenOne") == null)
    {
        await userManager.CreateAsync(user: new ApplicationUser
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
        await configurationDbContext.ApiResources.AddAsync(entity: new ApiResource
        {
            Name = Guid.NewGuid().ToString(),
            DisplayName = "API",
            Scopes = new List<string>
            {
                "https://www.example.com/api"
            }
        }.ToEntity());

        await configurationDbContext.SaveChangesAsync();
    }
    
    if (!await configurationDbContext.ApiScopes.AnyAsync())
    {
        await configurationDbContext.ApiScopes.AddAsync(entity: new ApiScope
        {
            Name = "https://www.example.com/api",
            DisplayName = "API"
        }.ToEntity());
        
        await configurationDbContext.SaveChangesAsync();

    }
    
    if (!await configurationDbContext.Clients.AnyAsync())
    {
        await configurationDbContext.Clients.AddRangeAsync(entities: new[]
        {
            new Client
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecrets = new List<Secret>
                {
                    { new(value: "secret".Sha512()) }
                },
                ClientName = "Console Application",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string>
                {
                    "https://www.example.com/api"
                },
                AllowedCorsOrigins = new List<string>
                {
                    "https://api:7001"
                }
            }.ToEntity(),
            new Client
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecrets = new List<Secret>
                {
                    { new(value: "secret".Sha512()) }
                },
                ClientName = "Web Application",
                AllowedGrantTypes = GrantTypes.Code,
                AllowedScopes = new List<string>
                {
                    "https://www.example.com/api",
                    "openid", "email", "profile"
                },
                RedirectUris = new List<string>
                {
                    "https://webapplication:7002/signin-oidc"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    "https://webapplication:7002/signout-callback-oidc"
                }
            }.ToEntity(),
            new Client
            {
                ClientId = Guid.NewGuid().ToString(),
                RequireClientSecret = false,
                ClientName = "Single Page Application",
                AllowedGrantTypes = GrantTypes.Code,
                AllowedCorsOrigins = new List<string>
                {
                    "https://singlepageapplication:7003"
                },
                AllowedScopes = new List<string>
                {
                    "https://www.example.com/api",
                    "openid", "email", "profile"
                },
                RedirectUris = new List<string>
                {
                    "http://signlepageapplication:7003/authentication/login-callback"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    "http://signlepageapplication:7003/authentication/logout-callback"
                }
            }.ToEntity()
        });
        
        await configurationDbContext.SaveChangesAsync();

    }

    if (!await configurationDbContext.IdentityResources.AnyAsync())
    {
        await configurationDbContext.IdentityResources.AddRangeAsync(entities: new[]
        {
            new IdentityResources.OpenId().ToEntity(),
            new IdentityResources.Profile().ToEntity(),
            new IdentityResources.Email().ToEntity()
        });
        await configurationDbContext.SaveChangesAsync();
    }

}

app.Run();

void NpgsqlOptionsAction(NpgsqlDbContextOptionsBuilder npgsqlDbContextOptionsBuilder)
{
    npgsqlDbContextOptionsBuilder.MigrationsAssembly(assemblyName: typeof(Program).GetTypeInfo().Assembly.GetName().Name);
}

void ResolveDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder)
{
    dbContextOptionsBuilder.UseNpgsql(connectionString: serviceProvider.GetRequiredService<IConfiguration>()
        .GetConnectionString(name: "IdentityServer"), npgsqlOptionsAction: NpgsqlOptionsAction);
}