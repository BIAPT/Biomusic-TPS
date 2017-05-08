using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ventuz.OSC;


namespace bmosc {
  class SndGnrtr {
    public static Process exe = null;

    private static UdpWriter ocsWriter = null;


    private static int InitOCSWriter() {
      try {
        ocsWriter = new UdpWriter(Globals.OSCADDRESS, Globals.OSCPORT);
      } catch(Exception e) {
        Logger.Err(601);
      }
      if(ocsWriter == null) {
        Logger.Err(602);
      }
      Logger.Msg($"Connection to the audio generator has been created at: {Globals.OSCADDRESS}:{Globals.OSCPORT}");
      return Globals.OK;
    }

    public static int Init() {

      //var cmd = new ProcessStartInfo("taskkill", $"/im scide.exe /f");
      //cmd.UseShellExecute = false;
      //try {
      //  Process.Start(cmd);
      //} catch(Exception e) {
      //}

      //Thread.Sleep(2000);

      //cmd = new ProcessStartInfo("taskkill", $"/im sclang.exe /f");
      //cmd.UseShellExecute = false;
      //try {
      //  Process.Start(cmd);
      //} catch(Exception e) {
      //}

      //Thread.Sleep(2000);

      //cmd = new ProcessStartInfo("taskkill", $"/im scsynth.exe /f");
      //cmd.UseShellExecute = false;
      //try {
      //  Process.Start(cmd);
      //} catch(Exception e) {
      //}

      //Thread.Sleep(2000);


      //var clntProcess = new ProcessStartInfo();
      //clntProcess.CreateNoWindow = false;
      //clntProcess.UseShellExecute = true;
      //clntProcess.FileName = Globals.SCPATH;
      //exe = Process.Start(clntProcess);
      //if(exe != null) {
      //  Thread.Sleep(3000);
      //  Logger.Msg($"Soun Generation (SuperCollider) process has been created with: {exe}");
      //}

      // start a writer
      var rslt = InitOCSWriter();

      return Globals.OK;
    }
    public static void Stop() {
      //OcsKillServer();
      Thread.Sleep(2000);
      if(exe != null) {
        exe.Kill();
      }
    }

    //Globals.OCSWRITER.Send(new OscBundle(0, new OscElement("/data", rnd.Next(0, 500) + 100)));
    /*
To open the soundfile ready fro recording please send:  
("/recordingFile", 1, "filename1");
no filename extensions! it will automatically be WAV.

*************REPEAT FOR EACH VIDEO*****************
To fade sound synthesis IN, when video starts, please send:
("/fadePause", 1);

To send data at a rate of ~10 Hz, please send:
("/data", HBR, dSMEDA/dt, DTMP);
dSMEDA/dt is the first derivative of the smoothed EDA SMEDA

To fade sound synthesis OUT, when video approaches its end, please send 2.5 seconds BEFORE the end:
("/fadePause", 0);
**************************************************************

To close the soundfile and normalize it please send:  
("/recordingFile", 0);

     */

    public static void OscSendData(Dictionary<string, double> data) {
      ocsWriter.Send(new OscBundle(0, new OscElement("/tps", data["hbr"], data["eda"], data["temp"], data["ts"])));
      Console.WriteLine($"Scanner: hbr = {data["hbr"]}\teda = {data["eda"]}\ttemp = {data["temp"]}\tts = {data["ts"]}");
    }

  }
}
