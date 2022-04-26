using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Grains;

var host = await StartSilo();
Console.ReadLine();
host.StopAsync().Wait();

static async Task<ISiloHost> StartSilo()
{
    string connString = "Server=localhost;Port=5432;User Id=postgres;Password=123456a;Database=test01";
    string invariant = "Npgsql";
    var host = new SiloHostBuilder()
        .UseSignalR()
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
        .AddMemoryGrainStorage(SignalR.Orleans.Constants.STORAGE_PROVIDER)

        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UserNotificationGrain).Assembly).WithReferences())
        .ConfigureEndpoints(11111, 30000)
        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
        .ConfigureLogging((context, logging) =>
        {
            logging.AddConsole();
        })
        .Build();
    await host.StartAsync();
    return host;
}
