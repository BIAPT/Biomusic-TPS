using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bmosc {
  public class Scanner {

    public static Dictionary<string, double> data = null;

    private static Timer scnrTimer = null;

    private static int cycleCounter = 0;

    public static int noSignalCntr = 0;
    public static bool noSignal = false;

    private static int prsntCntr = 0;
    private static bool prsntChkInProgress = false;
    private static DateTime prsntChkStart;

    private static DateTime oldTstp = DateTime.Now;



    public static int Init() {

      // create a  timer
      // but not start it yet
      scnrTimer = new Timer(Worker, null, Timeout.Infinite, Timeout.Infinite);
      // check for the result
      if(scnrTimer == null) {
        // TODO - set an error object
        return Logger.Err(402);
      }
      Console.WriteLine("Scanner: timer has been created");

      // creatin a basic data structure
      data = new Dictionary<string, double>();
      data["hbr"] = 0;
      data["eda"] = 0;
      data["temp"] = 0;
      data["ts"] = 0;

      return Globals.OK;
    }

    private static bool working = false;
    private static void Worker(Object doneSignal) {
      var rslt = Globals.ERR;
      if(working) {
        return;
      } else {
        working = true;
      }

        // for the beginning we'll work only with time stamp now
        CollectData(DateTime.Now);
      working = false;
    }

    private static void CollectData(DateTime tStamp) {
      data["ts"] = Double.Parse(tStamp.ToString("yyMMddHHmmssfff")) / 1000;
      Driver.GetDataOSC(data);

      SndGnrtr.OscSendData(data);

    }

    public static void Run() {
      // reset flags
      cycleCounter = 0;
      noSignalCntr = 0;
      noSignal = false;

      // activate device
      Driver.Run();

      // and start the timer
      scnrTimer.Change(0, Globals.SCANRATE);
      // and rase the scanner alive flag
    }


    public static void Stop() {
      // stop the timer
      scnrTimer.Change(Timeout.Infinite, Timeout.Infinite);
      Thread.Sleep(Globals.SCANRATE);

      // and stop the device
      Driver.Stop();
      ////Util.SaveData(data, cycleCounter);
      //if(!Globals.DELETEDATA) {
      //  Util.SaveData(data, 0);
      //}
    }
 
  }
}
