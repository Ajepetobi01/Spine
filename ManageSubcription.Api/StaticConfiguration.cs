using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ManageSubcription.Api
{
    static class StaticConfiguration
    {
        public static IConfiguration AppSetting { get; }
        static StaticConfiguration()
        {
            AppSetting = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
        }
    }
}
