using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace PingusCore
{
  class Program
  {
    // public static IConfiguration configuration { get; set; }
    static async Task Main(string[] args)
    {
      await new HostBuilder()
            .ConfigureAppConfiguration(configApp =>
            {
              configApp.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureHostConfiguration(configHost =>
            {
            })
            .ConfigureServices((hostContext, services) =>
            {
              services.AddOptions();

              services.Configure<AppSettings>(options => hostContext.Configuration.GetSection("AppSettings"));
              services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<AppSettings>>().Value);
              var appSettings = new AppSettings();
              new ConfigureFromConfigurationOptions<AppSettings>(hostContext.Configuration.GetSection("AppSettings"))
                    .Configure(appSettings);
              services.AddSingleton(new AppSettingsProvider(appSettings));

              services.AddHostedService<PingService>();
            })
            .RunConsoleAsync();
    }
  }
}
