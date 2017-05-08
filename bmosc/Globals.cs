using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;

namespace bmosc {
  public static class Globals {

    public static int OK { get { return 0; } }
    public static int ERR { set; get; }

    public static System.IO.StreamWriter logFile = null;

    public static NameValueCollection settings;

    public static int[] VIDEOSEQUENCE = null;
    public static List<Dictionary<string, string>> SBJEXPERIENCE = null;
    public static Dictionary<string, string> DMGR = null;

    public static string fileNameTS = "";
    public static string subjDirPath = "";
    public static string rsltAudioFileName = "";
    public static int crntVideoId;
    public static double crntVideoDrtn;
    public static bool isRecording = false;
    public static bool isFadeOutSent = false;
    public static bool prsntCheck = false;
    public static bool isPrsnt = false;
    public static double activityMarker = 0;

    public static int RunLevel = (int)Globals.RUNLEVEL.INIT;
    public enum RUNLEVEL {
      QUIT,
      INIT,
      IDLE,
      RUN
    };

    public enum ACTIVITY {
      STRWAIT,
      SQTPRGR,
      SQRWAIT,
      VIDEO,
      EXPRNC,
      RSLTPREP,
      RSLTPLAY,
      POSTRSLT
    };
    /*
     Activity marker codes:
     0  - starting to scan waiting for signal quality test request
     1.1 ... 1.n - doing a signal qualit test; ".n" - pass number
     2.1 ... 2.n - post signal quality test waiting; ".n" - quality test result
     3.n...3.k - playing video # .n/.../.k , i.e. for video 12.mp4 MRK = 3.12
     4.n...4.k - waiting for subject experience, like a video marker

       */

    private static string oscAddress = "127.0.0.1";
    public static string OSCADDRESS {
      private set { oscAddress = value; }
      get { return oscAddress; }
    }
    private static void SetOSCADDRESS() {
      if(settings["OSCADDRESS"] != null) {
        Globals.OSCADDRESS = settings["OSCADDRESS"];
      }
      Logger.Msg($"Set OSC Address: {Globals.OSCADDRESS}");
    }

    private static int oscPort = 57120;
    public static int OSCPORT {
      private set { oscPort = value; }
      get { return oscPort; }
    }
    private static void SetOSCPORT() {
      if(settings["OSCPORT"] != null) {
        try {
          Globals.OSCPORT = int.Parse(settings["OSCPORT"]);
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
    }

    private static int hkRate = 200;  // mS
    public static int HKRATE {
      private set { hkRate = value; }
      get { return hkRate; }
    }
    private static void SetHKRATE() {
      if(settings["HKRATE"] != null) {
        try {
          var tmp = int.Parse(settings["HKRATE"]);
          if(tmp > 0) { Globals.HKRATE = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set HKRATE = {HKRATE}");
    }

    private static string tpsPort = "";
    public static string TPSPORT {
      private set { tpsPort = value; }
      get { return tpsPort; }
    }
    private static void SetTPSPORT() {
      if(settings["TPSPORT"] != null) {
        Globals.TPSPORT = settings["TPSPORT"];
      }
      Logger.Msg($"Set Device Connection String: {Globals.TPSPORT}");
    }

    // initialize  device's channel map
    public static Dictionary<string, int> CHNLMAP = null;
    private static void SetCHNLMAP() {
      CHNLMAP = new Dictionary<string, int>();
      foreach(string index in settings.AllKeys) {
        var parts = index.Split('_');
        if(parts[0] == "CHNL") {
          AddChnl(parts[1], int.Parse(settings[index]));
        }
      }

    }
    private static void AddChnl(string ind, int val) {
      if(CHNLMAP.ContainsValue(val)) {
        Logger.Err(102);
        Logger.Msg($"Duplicate channel value for: index {ind}; val {val}");
      } else if(CHNLMAP.ContainsKey(ind)) {
        Logger.Err(103);
        Logger.Msg($"Duplicate channel index for: index {ind}; val {val}");
      } else {
        CHNLMAP.Add(ind, val);
        Logger.Msg($"Added channel {ind} : {val}");
      }
    }

    // Scanner's section

    private static int trialDrtnMS = 4000;
    public static int TRIALDRTNMS {
      private set { trialDrtnMS = value; }
      get { return trialDrtnMS; }
    }
    private static void SetTRIALDRTNMS() {
      if(settings["TRIALDRTNMS"] != null) {
        try {
          var tmp = int.Parse(settings["TRIALDRTNMS"]);
          if(tmp > 0) { Globals.TRIALDRTNMS = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set TRIALDRTNMS = {TRIALDRTNMS}");
    }

    private static int scanRateBVP = 300;
    public static int SCANRATEBVP {
      private set { scanRateBVP = value; }
      get { return scanRateBVP; }
    }
    private static void SetSCANRATEBVP() {
      if(settings["SCANRATEBVP"] != null) {
        try {
          var tmp = int.Parse(settings["SCANRATEBVP"]);
          if(tmp > 0) { Globals.SCANRATEBVP = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set SCANRATEBVP = {SCANRATEBVP}");
    }

    private static int scanRateEDA = 15;
    public static int SCANRATEEDA {
      private set { scanRateEDA = value; }
      get { return scanRateEDA; }
    }
    private static void SetSCANRATEEDA() {
      if(settings["SCANRATEEDA"] != null) {
        try {
          var tmp = int.Parse(settings["SCANRATEEDA"]);
          if(tmp > 0) { Globals.SCANRATEEDA = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set SCANRATEEDA = {SCANRATEEDA}");
    }

    private static int scanRateTMP = 15;
    public static int SCANRATETMP {
      private set { scanRateTMP = value; }
      get { return scanRateTMP; }
    }
    private static void SetSCANRATETMP() {
      if(settings["SCANRATETMP"] != null) {
        try {
          var tmp = int.Parse(settings["SCANRATETMP"]);
          if(tmp > 0) { Globals.SCANRATETMP = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set SCANRATETMP = {SCANRATETMP}");
    }

    private static int scanRate = 10;
    public static int SCANRATE {
      private set { scanRate = value; }
      get { return scanRate; }
    }
    private static void SetSCANRATE() {
      if(settings["SCANRATE"] != null) {
        try {
          var tmp = int.Parse(settings["SCANRATE"]);
          if(tmp > 0) { Globals.SCANRATE = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set SCANRATE = {SCANRATE}");
    }

    private static double zeroTempThrhld = 0.001;
    public static double ZEROTEMPTHRHLD {
      get { return zeroTempThrhld; }
    }

    private static double slopeTempThrhld = 0.001;
    public static double SLOPETEMPTHRHLD {
      get { return slopeTempThrhld; }
    }

    private static double noSgnlTrhshld = 0.001;
    public static double NOSGNLTRHSHLD {
      private set { noSgnlTrhshld = value; }
      get { return noSgnlTrhshld; }
    }
    private static void SetNOSGNLTRHSHLD() {
      if(settings["NOSGNLTRHSHLD"] != null) {
        try {
          var tmp = Double.Parse(settings["NOSGNLTRHSHLD"]);
          if(tmp > 0) { Globals.NOSGNLTRHSHLD = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set NOSGNLTRHSHLD = {NOSGNLTRHSHLD}");
    }

    private static int noSgnlCntr = 10;
    public static int NOSGNLCNTR {
      private set { noSgnlCntr = value; }
      get { return noSgnlCntr; }
    }
    private static void SetNOSGNLCNTR() {
      if(settings["NOSGNLCNTR"] != null) {
        try {
          var tmp = int.Parse(settings["NOSGNLCNTR"]);
          if(tmp > 0) { Globals.NOSGNLCNTR = tmp; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set NOSGNLCNTR = {NOSGNLCNTR}");
    }

    private static bool holdEnd = false;
    public static bool HOLDEND {
      private set { holdEnd = value; }
      get { return holdEnd; }
    }
    private static void SetHOLDEND() {
      if(settings["HOLDEND"] != null) {
        try {
          if(settings["HOLDEND"].ToUpper() == "YES") {
            Globals.HOLDEND = true;
          }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set HOLDEND = {HOLDEND}");
    }

    private static int dataLength;
    public static int DATALENGTH {
      private set { dataLength = value; }
      get { return dataLength; }
    }

    private static int dataLengthBVP;
    public static int DATALENGTHBVP {
      private set { dataLengthBVP = value; }
      get { return dataLengthBVP; }
    }

    private static int cycleDrtnMS;
    public static int CYCLEDRTNMS {
      private set { cycleDrtnMS = value; }
      get { return cycleDrtnMS; }
    }

    private static bool deleteData = false;
    public static bool DELETEDATA {
      set { deleteData = value; }
      get { return deleteData; }
    }
    private static void SetDELETEDATA() {
      Globals.DELETEDATA = false;
    }

    private static int SetSCANNER() {
      SetTRIALDRTNMS();
      SetSCANRATEBVP();
      SetSCANRATEEDA();
      SetSCANRATETMP();
      SetSCANRATE();
      SetSCANRATEBVP();
      SetNOSGNLCNTR();
      SetNOSGNLTRHSHLD();

      dataLengthBVP = (int)(2 * scanRateBVP / Math.Min(scanRateTMP, scanRateEDA));
      Logger.Msg($"Set DATALENGTHBVP = {DATALENGTHBVP}");

      cycleDrtnMS = (int)(1000 / scanRate);
      Logger.Msg($"Set CYCLEDRTNMS = {CYCLEDRTNMS}");

      dataLength = (int)(trialDrtnMS * 1.2 / cycleDrtnMS);
      Logger.Msg($"Set DATALENGTH = {DATALENGTH}");

      return Globals.OK;
    }

    public static Dictionary<int, double> DRTNVIDEO = null;
    private static void SetDRTNVIDEO() {
      DRTNVIDEO = new Dictionary<int, double>();
      foreach(string index in settings.AllKeys) {
        var parts = index.Split('_');
        if(parts[0] == "DRTNVIDEO") {
          AddDrtnVideo(int.Parse(parts[1]), double.Parse(settings[index]) * 1000);
        }
      }
    }
    private static void AddDrtnVideo(int ind, double val) {
      if(DRTNVIDEO.ContainsKey(ind)) {
        Logger.Err(106);
        Logger.Msg($"Duplicate video duration index for: index {ind}; val {val}");
      } else {
        DRTNVIDEO.Add(ind, val);
        Logger.Msg($"Added video duration {ind} : {val}");
      }
    }

    private static double fadeOutLag = 2500;
    public static double FADEOUTLAG {
      private set { fadeOutLag = value; }
      get { return fadeOutLag; }
    }
    private static void SetFADEOUTLAG() {
      if(settings["FADEOUTLAG"] != null) {
        try {
          var tmp = double.Parse(settings["FADEOUTLAG"]);
          if(tmp > 0) { Globals.FADEOUTLAG = tmp * 1000; }
        } catch(Exception e) {
          // just ignore and use dfault value
        }
      }
      Logger.Msg($"Set FADEOUTLAG = {FADEOUTLAG}");
    }

    private static double prsntChkTimeout = 15000;
    public static double PRSNTCHKTIMEOUT {
      private set { prsntChkTimeout = value; }
      get { return prsntChkTimeout; }
    }
    private static void SetPRSNTCHKTIMEOUT() {
      try {
        PRSNTCHKTIMEOUT = double.Parse(settings["PRSNTCHKTIMEOUT"]);
      } catch(Exception e) {

      }
      Logger.Msg($"Set PRSNTCHKTIMEOUT = {PRSNTCHKTIMEOUT}");
    }

    private static int prsntChkCls = 500;
    public static int PRSNTCHKCLS {
      private set { prsntChkCls = value; }
      get { return prsntChkCls; }
    }
    private static void SetPRSNTCHKCLS() {
      try {
        PRSNTCHKCLS = int.Parse(settings["PRSNTCHKCLS"]);
      } catch(Exception e) {

      }
      Logger.Msg($"Set PRSNTCHKCLS = {PRSNTCHKCLS}");
    }

    public static int Init() {

      try {
        // get settings from the "bm_hw.exe.conf" file 
        settings = ConfigurationManager.AppSettings;
        if(settings == null) {
          Environment.Exit(101);
        }
      } catch(Exception e) {
        Environment.Exit(101);
      }

      SetHOLDEND();
      SetOSCADDRESS();
      SetOSCPORT();
      SetTPSPORT();
      SetCHNLMAP();
      SetSCANNER();
      SetDELETEDATA();

      return Globals.OK;
    }

    public static Dictionary<int, string> Errors = new Dictionary<int, string> {
      // 1xx - errors for Globals
      {101, "Can't process config file" },
      {102, "Duplicate channel value" },
      {103, "Duplicate channel index" },
      {104, "Can't find the TESTER" },
      {105, "Can't find the SuperCollider" },
      {106, "Duplicate video duration value" },
      {107, "Duplicate video duration index" },
      // 2xx - errors for Messanger
      {201, "Can't create JSON Parser" },
      {202, "WS URL is empty" },
      {203, "Can't create WS socket" },
      {204, "Can't connect to WS server" },
      // 3xx - errors for TPS device/driver
      {301, "Can't instatiate a control library for TPS device" },
      {302, "No value for the device's port provided" },
      {303, "Can't connect to TPS device" },
      // 4xx - errors for the scanner
      {401, "Can't create a data structure" },
      {402, "Can't create a timer" },
      // 5xx - errors for the scanner
      {501, "Can't create dirctory for the subject's data" },
      {502, "Can't write data unit into the file" },
      {503, "Can't create a file for the resulting data" },
      {504, "Can't create logs directory" },
      {505, "Can't reate a log file" },
      // 6xx - errors for the audio generator
      {601, "Can't create UdpWriter" },
      {602, "Can't connect to audio generator" },
      {603, "Can't start sound generator" },
    };

  }
}
