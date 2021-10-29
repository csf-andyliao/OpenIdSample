using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace AuthorizationServer
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DbContext>();
                await context.Database.EnsureCreatedAsync(cancellationToken);
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var client = await manager.FindByClientIdAsync("client_credential", cancellationToken);

                if (client == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor()
                    {
                        ClientId = "client_credential",
                        ClientSecret = "client_credential",
                        DisplayName = "ClientCredential",
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "weather_api_1",
                            OpenIddictConstants.Permissions.ResponseTypes.Code
                        }
                    }, cancellationToken);
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DbContext>();
                await context.Database.EnsureCreatedAsync(cancellationToken);
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var client = await manager.FindByClientIdAsync("authorization_code", cancellationToken);

                if (client == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor()
                    {
                        ClientId = "authorization_code",
                        ClientSecret = "authorization_code",
                        DisplayName = "Authorizationcode",
                        RedirectUris = { new Uri("https://localhost:44381/home/callback") },
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "weather_api_1",
                            OpenIddictConstants.Permissions.ResponseTypes.Code
                        }
                    }, cancellationToken);
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DbContext>();
                await context.Database.EnsureCreatedAsync(cancellationToken);
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var client = await manager.FindByClientIdAsync("authorization_code2", cancellationToken);

                if (client == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor()
                    {
                        ClientId = "authorization_code2",
                        ClientSecret = "authorization_code",
                        DisplayName = "Authorizationcode2",
                        RedirectUris = { new Uri("https://localhost:44381/home/callback") },
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "weather_api_1",
                            OpenIddictConstants.Permissions.ResponseTypes.Code
                        }
                    }, cancellationToken);
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DbContext>();
                await context.Database.EnsureCreatedAsync(cancellationToken);
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var client = await manager.FindByClientIdAsync("authorization_code3", cancellationToken);

                if (client == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor()
                    {
                        ClientId = "authorization_code3",
                        ClientSecret = "authorization_code",
                        DisplayName = "Authorizationcode3",
                        RedirectUris = { new Uri("https://localhost:44381/home/callback") },
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.Endpoints.Introspection,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "weather_api_1",
                            OpenIddictConstants.Permissions.ResponseTypes.Code
                        }
                    }, cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
