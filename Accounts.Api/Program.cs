using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Accounts.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //    var config = new ConfigurationBuilder()
            // .AddJsonFile("appsettings.json", optional: false)
            // .Build();

            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                var host = CreateHostBuilder(args).Build();
                host.Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                 .ConfigureLogging(logging =>
                 {
                     logging.ClearProviders();
                     logging.SetMinimumLevel(LogLevel.Trace);
                 })
                .UseNLog();  // NLog: Setup NLog for Dependency injection

        // to use MS default logging
        //.ConfigureLogging((context, logging) =>
        //{
        //    logging.ClearProviders();
        //    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
        //    logging.AddDebug();
        //    logging.AddConsole();
        //});
    }
}
