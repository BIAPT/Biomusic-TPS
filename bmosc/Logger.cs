using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmosc {
  public static class Logger {
    public static int Err(int err) {
      string msg = $"Error# {err} : {Globals.Errors[err]}.";
      LogMessage(msg);
      return err;

    }

    public static int Msg(string message) {
      string msg = $"[{DateTime.Now.ToString("yy.MM.dd-HH:mm:ss")}] {message}";
      LogMessage(msg);
      return Globals.OK;
    }

    private static void LogMessage(string msg) {
#if !SILENT
      Console.WriteLine(msg);
#endif
      if(Globals.logFile != null) {
        Globals.logFile.WriteLine(msg);
      }
    }
  }
}
