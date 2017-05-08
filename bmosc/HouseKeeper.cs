using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// using Globals;

namespace bmosc {
  class HouseKeeper {
    // signal to quit
    private static AutoResetEvent doneCall = null;

    // main housekeeper's timer
    private static Timer hkTimer = null;

    // command key input holder
    private static ConsoleKeyInfo inKey = new ConsoleKeyInfo();

    private static int oldRunLevel;

    private static bool wokerIsIn = false;

    public static int Init() {
      // we need a listener for the signal when housekeeper is done
      doneCall = new AutoResetEvent(false);
      // check if we are successful
      if(doneCall == null) {
        Console.WriteLine("Can't create a signal listener");
        // TODO set an error object
        return Globals.ERR;
      }
      // now, when we have a signal listener, 
      // we can create a main housekeeper's timer
      // but not start it yet
      hkTimer = new Timer(Worker, doneCall, Timeout.Infinite, Timeout.Infinite);
      // check for the result
      if(hkTimer == null) {
        // TODO - set an error object
        Console.WriteLine("Can't create HK Timer");
        return Globals.ERR;
      }
      Console.WriteLine("HK Init is ok now...");
      return Globals.OK;
    }

    public static void DoBusiness() {
      // update a runlevel
      Globals.RunLevel = (int)Globals.RUNLEVEL.IDLE;
      // preserve "starting" runlevel
      oldRunLevel = Globals.RunLevel;

      Console.WriteLine("HouseKeeper: starting now.");
      // and start the timer
      hkTimer.Change(0, Globals.HKRATE); //
                               // wait for when it's done...
      doneCall.WaitOne();
      // and cleanup after...
      Driver.Quit();
      hkTimer.Dispose();
    }

    private static void Worker(Object doneSignal) {

      // just for tunning: display timestamp if running
      //if(Globals.RunLevel == (int)Globals.RUNLEVEL.IDLE) {
      //  Console.WriteLine($"HouseKeeper idle cycle at: {DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff")}");
      //}

      // re-enterance test
      if(!wokerIsIn) {
        wokerIsIn = true;

        // first check if key has been pressed
        if(Console.KeyAvailable) {
          // let's get the key
          inKey = Console.ReadKey(true);
          // and switch depending what it is
          switch(inKey.Key) {
            case ConsoleKey.Q:
              // Q - done, exit
              //Console.WriteLine("HouseKeeper.worker: we got the command to stop, do clean up and exit.");
              // change run level to STOP
              Globals.RunLevel = (int)Globals.RUNLEVEL.QUIT;
              break;
            case ConsoleKey.I:
              // I - idle
              //Console.WriteLine("HouseKeeper.worker: we got the command to go idle.");
              // change run level to IDLE
              Globals.RunLevel = (int)Globals.RUNLEVEL.IDLE;
              Scanner.Stop();
              break;
            case ConsoleKey.R:
              // R - run, data collection
              //Console.WriteLine("HouseKeeper.worker: we got the command to perform run / data collection.");
              Globals.RunLevel = (int)Globals.RUNLEVEL.RUN;
              Scanner.Run();
              break;
          }
        }

        // check if runlevel changed and action needed
        if(oldRunLevel != Globals.RunLevel) {
          // preserve new runlevel
          oldRunLevel = Globals.RunLevel;
          //Console.WriteLine("HK: saving a new runlevel.");

          // andswitch depending of a new runlevel
          switch(Globals.RunLevel) {
            case (int)Globals.RUNLEVEL.QUIT:
              // we have to check if the scanner still alive 
              // before issue done signal
              //if(!glbls.scannerAlive) {
                // scanner is not alive - send done signal
                Console.WriteLine("HouseKeeper: sending the done signal");
                doneCall.Set();
              //}
              break;
            case (int)Globals.RUNLEVEL.IDLE:
              Console.WriteLine("HouseKeeper: turning to idle");
              break;
            case (int)Globals.RUNLEVEL.RUN:
              Console.WriteLine("HouseKeeper: starting to run");
              break;
          }
        }
        // drop busy flag
        wokerIsIn = false;
      }
    }
  }
}
