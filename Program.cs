using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using websiteLogin.Models;
using ContosoUniversity.Data;
using Microsoft.Extensions.DependencyInjection;

namespace websiteLogin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                Hostconfig.certPath = context.Configuration["CertPath"];
                Hostconfig.certPassword = context.Configuration["CertPassword"];
            }).ConfigureWebHostDefaults(webBuilder =>
            {
                var host = Dns.GetHostEntry("websiteLogin.dk");
                webBuilder.ConfigureKestrel(opt =>
                {
                    opt.Listen(host.AddressList[0], 80);
                    opt.Listen(host.AddressList[0], 443, listOpt =>
                    {
                        listOpt.UseHttps(Hostconfig.certPath, Hostconfig.certPassword);
                    });
                });
                webBuilder.UseStartup<Startup>();
            });
    }

    public static class Hostconfig
    {
        public static string certPath { get; set; }
        public static string certPassword { get; set; }
    }
}