using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace PingusCore
{
  public class PingService : IHostedService, IDisposable
  {
    private Timer _timer;
    private ILogger logger;
    Dictionary<Host, HostStatistics> stats = new Dictionary<Host, HostStatistics>();
    bool firstRoundCompleted = false;
    public PingService()
    {
      logger = new LoggerConfiguration()
        .WriteTo.RollingFile("Logs/PingusCore.log", Serilog.Events.LogEventLevel.Debug)
        .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
        .CreateLogger();
      foreach (var h in AppSettingsProvider.AppSettings.Hosts)
      {
        stats.Add(h, new HostStatistics(h));
      }
      SettingsCheck();
    }

    private void SettingsCheck()
    {
      int timeoutSum = AppSettingsProvider.AppSettings.Hosts.Sum(p => p.TimeoutInSeconds);
      if (timeoutSum > AppSettingsProvider.AppSettings.PingIntervalInSeconds)
        logger.Warning("AppSettings: Sum of timeouts is exceeding the ping round interval. It is not recommended in long time process running.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      logger.Information("Ping service started");
      _timer = new Timer(DoWork, null, TimeSpan.Zero,
          TimeSpan.FromSeconds(AppSettingsProvider.AppSettings.PingIntervalInSeconds));

      return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
      bool shouldOverwritePreviousResult = true;
      foreach (var hostCfg in stats.Keys)
      {
        var hostStats = stats[hostCfg];
        var result = PerformPing(hostCfg, shouldOverwritePreviousResult);
        if (result)
        {
          hostStats.AddSuccess();
        }
        else
        {
          hostStats.AddFailure();
          shouldOverwritePreviousResult = false;
        }
      }
      if (!firstRoundCompleted)
        DisplaySummary(false);
      else
        DisplaySummary(shouldOverwritePreviousResult);
      firstRoundCompleted = true;
    }

    private void DisplaySummary(bool shouldOverwritePreviousResult)
    {
      int hostCount = stats.Count;
      int YOffset = shouldOverwritePreviousResult == true ? hostCount - 1 : 0;
      //logger.Information($"overwrite:{shouldOverwritePreviousResult}; offset:{YOffset}");
      Console.SetCursorPosition(0, Console.CursorTop - YOffset);
      int i = 0;
      foreach (var h in stats.Keys)
      {
        bool isLast = (i == hostCount - 1 ? true : false);
        stats[h].DisplayStats(isLast);
        i++;
      }
    }

    private string GetErrorInfoFromReply(string host, PingReply reply)
    {
      return $"Host: {host}; Reply from address: {reply.Address.ToString()}; Status:{reply.Status.ToString()};";
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      Console.WriteLine();
      logger.Information("Ping service stopped");
      _timer?.Change(Timeout.Infinite, 0);
      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }

    private bool PerformPing(Host hostCfg, bool shouldOverwritePreviousResult)
    {
      try
      {
        Ping pingSender = new Ping();
        PingOptions options = new PingOptions();

        // Use the default Ttl value which is 128,
        // but change the fragmentation behavior.
        options.DontFragment = true;

        // Create a buffer of 32 bytes of data to be transmitted.
        string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        PingReply reply = pingSender.Send(hostCfg.HostName, hostCfg.TimeoutInSeconds * 1000, buffer, options);
        if (reply.Status != IPStatus.Success)
        {
          if (shouldOverwritePreviousResult)
            Console.WriteLine();
          logger.Error(GetErrorInfoFromReply(hostCfg.HostName, reply));
          return false;
        }
        else
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        if (shouldOverwritePreviousResult)
          Console.WriteLine();
        logger.Error(ex.Message, ex);
        return false;
      }
    }
  }
}