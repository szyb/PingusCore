using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace PingusCore
{
  public class PingService : IHostedService, IDisposable
  {
    private Timer _timer;

    public PingService()
    {
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      Console.WriteLine("Timed Background Service is starting.");

      _timer = new Timer(DoWork, null, TimeSpan.Zero,
          TimeSpan.FromSeconds(10));

      return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
      Ping pingSender = new Ping();
      PingOptions options = new PingOptions();

      // Use the default Ttl value which is 128,
      // but change the fragmentation behavior.
      options.DontFragment = true;

      // Create a buffer of 32 bytes of data to be transmitted.
      string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      byte[] buffer = Encoding.ASCII.GetBytes(data);
      int timeout = 5000;
      Console.WriteLine("start");
      PingReply reply = pingSender.Send("192.168.0.10", timeout, buffer, options);
      if (reply.Status != IPStatus.Success)
      {
        Console.WriteLine(reply.Status);
        // Console.WriteLine("Address: {0}", reply.Address.ToString());
        // Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
        // Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
        // Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
        // Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
      }
      Console.WriteLine("stop");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      Console.WriteLine("Timed Background Service is stopping.");
      _timer?.Change(Timeout.Infinite, 0);
      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}