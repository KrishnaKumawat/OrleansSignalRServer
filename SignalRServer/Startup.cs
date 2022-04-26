using System;
using System.Threading.Tasks;
using Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansSignalR.Hubs;
using SignalR.Orleans.Clients;

namespace OrleansSignalR
{
    public class Startup
    {
        // add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connString = "Server=localhost;Port=5432;User Id=postgres;Password=123456a;Database=test01";
            string invariant = "Npgsql";

            services
                .AddSignalR()
                .AddOrleans();

            var client = new ClientBuilder()
                .Configure<ClusterOptions>(option =>
                {
                    option.ClusterId = "v1";
                    option.ServiceId = "OrleansRND";

                })
                .UseAdoNetClustering(options =>
                {
                    options.ConnectionString = connString;
                    options.Invariant = invariant;
                })
                .AddSimpleMessageStreamProvider(SignalR.Orleans.Constants.STREAM_PROVIDER, opt => opt.FireAndForgetDelivery = true)
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(IClientGrain).Assembly).WithReferences();
                    parts.AddApplicationPart(typeof(IUserNotificationGrain).Assembly).WithReferences();
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            client.Connect(CreateRetryFilter()).GetAwaiter().GetResult();
            services.AddSingleton(client);
        }

        //configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFileServer();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/ChatHub");
            });
        }

        private static Func<Exception, Task<bool>> CreateRetryFilter(int maxAttempts = 5)
        {
            var attempt = 0;
            return RetryFilter;

            async Task<bool> RetryFilter(Exception exception)
            {
                attempt++;
                Console.WriteLine($"Cluster client attempt {attempt} of {maxAttempts} failed to connect to cluster.  Exception: {exception}");
                if (attempt > maxAttempts)
                {
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(4));
                return true;
            }
        }
    }
}