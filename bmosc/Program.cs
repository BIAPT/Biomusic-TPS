using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;

namespace bmosc {
  class Program {

    static void Main(string[] args) {
      int rslt;
      //try {
      // initialize an environment
      rslt = Globals.Init();
      if(rslt != Globals.OK) {
        Logger.Msg($"Environment initialization has an error: {rslt} - {Globals.Errors[rslt]}");
        Environment.Exit(rslt);
      }

      // let's init the sensor
      rslt = Driver.Init();
      if(rslt != Globals.OK) {
        Logger.Msg($"Sensor failed to initialize, error# {rslt}");
        Environment.Exit(rslt);
      }

      // start Sound Generator - SuperCollider
      rslt = SndGnrtr.Init();
      if(rslt != Globals.OK) {
        Logger.Err(603);
      }

      // initialize housekeeper
      if(HouseKeeper.Init() != Globals.OK) {
        // TODO - log and send to the server an error code
        Console.WriteLine("CRITICAL ERROR: Can't initialize the HouseKeeper!");
        Environment.Exit(Globals.ERR);
      }

      // initalize scanner
      if(Scanner.Init() != Globals.OK) {
        Console.WriteLine("CRITICAL ERROR: Can't initialize the Scanner!");
        Environment.Exit(Globals.ERR);
      }

      // and start the business
      HouseKeeper.DoBusiness();

      Console.WriteLine("Done! Bye, bye...");

      Environment.Exit(Globals.OK);

    }
  }
}
