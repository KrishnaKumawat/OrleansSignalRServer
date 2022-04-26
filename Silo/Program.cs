using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Grains;

#region 001
////define the cluster configuration
//string connString = "Server=localhost;Port=5432;User Id=postgres;Password=123456a;Database=test01";
//string invariant = "Npgsql";
//var builder = new SiloHostBuilder()
//    .UseSignalR()
//    .Configure<ClusterOptions>(option =>
//    {
//        option.ClusterId = "v1";
//        option.ServiceId = "OrleansRND";

//    })
//    .UseAdoNetClustering(options =>
//    {
//        options.ConnectionString = connString;
//        options.Invariant = invariant;
//    })
//    .AddAdoNetGrainStorage("OrleansStorage", options =>
//    {
//        options.Invariant = invariant;
//        options.ConnectionString = connString;
//        options.UseJsonFormat = true;
//    })
//    .UseDashboard()
//    .ConfigureEndpoints(11111, 30000)
//    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
//    .ConfigureLogging(logging =>
//    {
//        logging.AddConsole();
//    });
//var host = builder.Build();
//await host.StartAsync();
#endregion

namespace Silo
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            string connString = "Server=localhost;Port=5432;User Id=postgres;Password=123456a;Database=test01";
            string invariant = "Npgsql";
            var host = new HostBuilder()
            .UseOrleans((context, siloBuilder) =>
            {
                siloBuilder.UseSignalR(builder =>
                {
                    builder.Configure((innerSiloBuilder, config) =>
                    {
                        innerSiloBuilder
                        .Configure<ClusterOptions>(option =>
                        {
                            option.ClusterId = "v1";
                            option.ServiceId = "OrleansRND";

                        }).UseAdoNetClustering(options =>
                        {
                            options.ConnectionString = connString;
                            options.Invariant = invariant;
                        })
                        .AddAdoNetGrainStorage("OrleansStorage", options =>
                        {
                            options.Invariant = invariant;
                            options.ConnectionString = connString;
                            options.UseJsonFormat = true;
                        })
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UserNotificationGrain).Assembly).WithReferences())
                        .ConfigureEndpoints(11111, 30000)
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback);
                    });
                });
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.AddConsole();
            })
            .Build();
            await host.RunAsync();

            return 0;
        }
    }
}