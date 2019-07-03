using System;

namespace PingusCore
{
  public class HostStatistics
  {
    private Host host { get; set; }
    public int PingsTransmitted { get; private set; }
    public int ReplySuccess { get; private set; }
    public HostStatistics(Host host)
    {
      this.host = host;
    }
    public void AddSuccess()
    {
      PingsTransmitted++;
      ReplySuccess++;
    }
    public void AddFailure()
    {
      PingsTransmitted++;
    }

    public void DisplayStats(bool isLast)
    {
      string text = $"Host: {host.HostName} : {ReplySuccess} / {PingsTransmitted}";
      if (isLast)
        Console.Write(text);
      else
        Console.WriteLine(text);
    }
  }
}