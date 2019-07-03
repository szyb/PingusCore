using System;
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
    private int pingCount = 0;
    private int pingSuccess = 0;

    public PingService()
    {
      logger = new LoggerConfiguration()
        .WriteTo.RollingFile("Logs/PingusCore.log", Serilog.Events.LogEventLevel.Error)
        .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
        .CreateLogger();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      logger.Information("Ping service started");
      _timer = new Timer(DoWork, null, TimeSpan.Zero,
          TimeSpan.FromSeconds(10));

      return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
      try
      {
        pingCount++;
        Ping pingSender = new Ping();
        PingOptions options = new PingOptions();

        // Use the default Ttl value which is 128,
        // but change the fragmentation behavior.
        options.DontFragment = true;

        // Create a buffer of 32 bytes of data to be transmitted.
        string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        int timeout = 5000;
        string host = "192.168.0.1";
        PingReply reply = pingSender.Send(host, timeout, buffer, options);
        if (reply.Status != IPStatus.Success)
        {
          logger.Error(GetErrorInfoFromReply(host, reply));
        }
        else
        {
          pingSuccess++;
          Console.SetCursorPosition(0, Console.CursorTop);
          Console.Write($"Ping success: {pingSuccess} / {pingCount}");
        }
      }
      catch (Exception ex)
      {
        logger.Error(ex.Message + ex.StackTrace);
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
  }
}