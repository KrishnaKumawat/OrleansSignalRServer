using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Configuration;

namespace OrleansSignalR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 30000);
                    options.Limits.MaxRequestBodySize = null;
                });
                //webBuilder.UseUrls("http://*:80");
                webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            });
    }
}