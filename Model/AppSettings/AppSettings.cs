using System.Collections.Generic;

namespace PingusCore
{
  public class AppSettings
  {
    public int PingIntervalInSeconds { get; set; }

    public IEnumerable<Host> Hosts { get; set; }

    public bool LogConfig { get; set; }
    public bool LogStats { get; set; }
  }
}