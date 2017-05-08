using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using AxTTLLiveCtrlLib;
using TPSbvp_cl;
using TTLLiveCtrlLib;
using Ventuz.OSC;

namespace bmosc {
  public static class Driver {

    // handlers
    //private AxTTLLiveCtrlLib.AxTTLLive mTTLLive = null;
    private static TTLLiveCtrlLib.ITTLLive mTTLLive = null;

    private static int mDev = -1;
    private static int mEDA = -1;
    private static int mBVP = -1;
    private static int mTMP = -1;



    // heart rate algorithm
    private static TPSbvp mHRAlg = null;
    private static justbpf mBpf1, mBpf2;

    private static int mCounter = 0;

    private static bool noSignal = false;
    private static int noSignalCntr = 0;

    private static int rslt = 0;
    private static double oldEDA = 0;
    private static double oldSMEDA = 0;
    private static double oldTMP = 0;
    private static int gapEDACntr = 0;
    private static int gapTempCntr = 0;

    private static double tempSlope = 0;
    private static double tempIntercept = 0;
    private static int tempFrstPnt = 0;
    private static bool notCalulatedSlope = true;

    public static int Init() {
      // initialize control library
      try {
        mTTLLive = new TTLLiveCtrlLib.TTLLive();
      } catch(Exception e) {
        return Logger.Err(301);
      }
      if(mTTLLive == null) {
        return Logger.Err(301);
      }

      // check for device's connection string
      if(Globals.TPSPORT == "") {
        return Logger.Err(302);
      }

      // connect to device
      mDev = mTTLLive.OpenConnection(Globals.TPSPORT, 1000);
      if(mDev >= 0) {
        Logger.Msg($"Connected to device at port: {Globals.TPSPORT}");
      } else {
        return Logger.Err(303);
      }

      // setup channels
      mTTLLive.AddChannel(mDev, Globals.CHNLMAP["EDA"], ref mEDA);
      mTTLLive.SensorType[mEDA] = mTTLLive.SensorID[mEDA];
      mTTLLive.UnitType[mEDA] = 16; // harcoded from example
      Logger.Msg($"EDA channel is:   {mEDA}\ttype: {mTTLLive.SensorType[mEDA]}\tunits: {mTTLLive.UnitType[mEDA]}");

      mTTLLive.AddChannel(mDev, Globals.CHNLMAP["BVP"], ref mBVP);
      mTTLLive.SensorType[mBVP] = mTTLLive.SensorID[mBVP];
      Logger.Msg($"BVP channel is:  {mBVP}\ttype: {mTTLLive.SensorType[mBVP]}\tunits: {mTTLLive.UnitType[mBVP]}");

      mTTLLive.AddChannel(mDev, Globals.CHNLMAP["TMP"], ref mTMP);
      mTTLLive.SensorType[mTMP] = mTTLLive.SensorID[mTMP];
      Logger.Msg($"TMP channel is: {mTMP}\ttype: {mTTLLive.SensorType[mTMP]}\tunits: {mTTLLive.UnitType[mTMP]}");

      return Globals.OK;
    }

    public static void Run() {
      // initialize HBR algorithm's objects
      mHRAlg = new TPSbvp();
      mHRAlg.Setup(Globals.SCANRATEBVP);
      mBpf1 = new justbpf();
      mBpf1.Setup(10.0f, 0.01f, 10.0f);
      mBpf2 = new justbpf();
      mBpf2.Setup(10.0f, 0.01f, 10.0f);

      // reset counters
      mCounter = 0;
      noSignal = false;
      noSignalCntr = 0;

      rslt = 0;
      oldEDA = 0;
      oldTMP = 0;
      gapEDACntr = 0;
      gapTempCntr = 0;

      tempSlope = 0;
      tempIntercept = 0;
      tempFrstPnt = 0;
      notCalulatedSlope = true;

      // start reading data
      mTTLLive.StartChannels();

      Console.WriteLine("************ Scanner: device started");
    }

    public static void Stop() {
      // stop reading data
      mTTLLive.StopChannels();

      // destroy HBR algorithm's objects
      mHRAlg = null;
      mBpf1 = null;
      mBpf2 = null;
    }

    public static void GetDataOSC(Dictionary<string, double> data) {
      double mValueEDA = 0;
      double mValueTMP = 0;
      double mValueHBR = 0;

      Array buff;
      // re-enterance 

      // getting TEMP channel
      buff = (Array)mTTLLive.ReadChannelDataVT(mTMP, 1000);
      if(buff.GetLength(0) > 0) mValueTMP = ((float[])buff).Average();
      //Console.WriteLine($"mValueTMP = {mValueTMP}");
      data["temp"] = mValueTMP;

      // getting BVP channel and converting it to HR value
      buff = (Array)mTTLLive.ReadChannelDataVT(mBVP, 1000);
      int length = buff.GetUpperBound(0);
      //      Console.WriteLine($"Length of raw BVP buff is: {length}");
      for(int i = 0; i <= length; i++) {
        mHRAlg.Process((float)buff.GetValue(i));
      }
      data["hbr"] = mHRAlg.HR();

      // getting EDA channel
      buff = (Array)mTTLLive.ReadChannelDataVT(mEDA, 1000);
      if(buff.GetLength(0) > 0) mValueEDA = ((float[])buff).Average();
      data["eda"] = mValueEDA;
    }

    public static int GetData(List<object> data, int counter) {
      double mValueEDA = 0;
      double mValueTMP = 0;
      double mValueHBR = 0;

      Array buff;
      // re-enterance 

      var rnd = new Random();

      var du = (Dictionary<string, object>)data[counter]; ;
      // scalar portion
      var scalar = (Dictionary<int, double>)du["sclr"];
      // getting TEMP channel
      buff = (Array)mTTLLive.ReadChannelDataVT(mTMP, 1000);
      if(buff.GetLength(0) > 0) mValueTMP = ((float[])buff).Average();
      scalar[Globals.CHNLMAP["TMP"]] = mValueTMP;

      // vector portion
      var vector = (double[])du["vctr"];

      // getting BVP channel and converting it to HR value
      buff = (Array)mTTLLive.ReadChannelDataVT(mBVP, 1000);
      int length = buff.GetUpperBound(0);
      //      Console.WriteLine($"Length of raw BVP buff is: {length}");
      for(int i = 0; i <= length; i++) {
        mHRAlg.Process((float)buff.GetValue(i));
        if(i < vector.Length) {
          vector[i] = Convert.ToDouble(buff.GetValue(i));
        }
      }
      mValueHBR = mHRAlg.HR();
      // and one more scalar value
      scalar[Globals.CHNLMAP["HBR"]] = mValueHBR;

      // getting EDA channel
      try {
        rslt = mTTLLive.Version;
      } catch(Exception e) {
        return Globals.ERR;
      }
      buff = (Array)mTTLLive.ReadChannelDataVT(mEDA, 1000);
      //} catch (Exception e) {

      //}
      if(buff.GetLength(0) > 0) mValueEDA = ((float[])buff).Average();
      scalar[Globals.CHNLMAP["EDA"]] = mValueEDA;

      // data is collected
      // data pre-processing section is next

      // check EDA channel
      if(mValueEDA < Globals.NOSGNLTRHSHLD) {
        // basic no signal check
        if(!noSignal && noSignalCntr++ >= Globals.NOSGNLCNTR) {
          if(scalar[Globals.CHNLMAP["MRK"]] > 1 && scalar[Globals.CHNLMAP["MRK"]] < 5) {
            //Messanger.SendNoSignal();
            noSignal = true;
          }
        }
        // fill gaps in data if any
        if(gapEDACntr++ <= 3) {
          scalar[Globals.CHNLMAP["EDA"]] = oldEDA;
        }
      } else {
        noSignalCntr = 0;
        gapEDACntr = 0;
        oldEDA = mValueEDA;
      }
      // get smoothed EDA
      if(counter > 10) {
        for(int n = 0; n < 10; n++) {

          var dataUnit = (Dictionary<string, object>)data[counter - n]; ;
          var sclrSet = (Dictionary<int, double>)dataUnit["sclr"];
          scalar[Globals.CHNLMAP["SMEDA"]] += 0.1 * sclrSet[Globals.CHNLMAP["EDA"]];
        }
        if(scalar[Globals.CHNLMAP["DT"]] != 0) {
          scalar[Globals.CHNLMAP["DSMEDA"]] = (scalar[Globals.CHNLMAP["SMEDA"]] - oldSMEDA) / scalar[Globals.CHNLMAP["DT"]];
        } else {
          scalar[Globals.CHNLMAP["DSMEDA"]] = 0;
        }
        oldSMEDA = scalar[Globals.CHNLMAP["SMEDA"]];
      }

      // check TMP channel
      if(mValueTMP < Globals.ZEROTEMPTHRHLD) {
        if(gapTempCntr++ <= 3) {
          scalar[Globals.CHNLMAP["TMP"]] = oldTMP;
        }
      } else {
        gapTempCntr = 0;
        oldTMP = mValueTMP;
      }

      // assume no changes in the temp slope
      scalar[Globals.CHNLMAP["DTMP"]] = 0;
      // check for slope change
      if(counter >= 4) {
        // set an intial slope and intercept point if needed
        if(notCalulatedSlope) {
          SetSlopeAndIntercept(data, 0);
          notCalulatedSlope = false;
        }
        // calculate deviations
        double sumSqrs = 0;
        for(int i = tempFrstPnt; i <= counter; i++) {
          double diff = GetTemp(data, i) - (tempSlope * i + tempIntercept);
          sumSqrs += diff * diff;
        }
        sumSqrs /= (counter - tempFrstPnt + 1);
        // check deviations again the threshold
        if(sumSqrs > Globals.SLOPETEMPTHRHLD) {
          var oldSlope = tempSlope;
          SetSlopeAndIntercept(data, counter - 4);
          if(oldSlope < tempSlope) {
            scalar[Globals.CHNLMAP["DTMP"]] = 1;
          } else {
            scalar[Globals.CHNLMAP["DTMP"]] = -1;
          }
        }

      }

      // test output
      //Console.WriteLine($"----> EDA:{mValueEDA.ToString("F3")}\tSMEDA:{scalar[Globals.CHNLMAP["SMEDA"]].ToString("F3")}\tTMP:{mValueTMP.ToString("F2")}\tdT:{scalar[Globals.CHNLMAP["DTMP"]].ToString("F0")}\tHBR:{mValueHBR.ToString("F2")}\tTSP:{ scalar[Globals.CHNLMAP["TSP"]].ToString("F3")}\tMRK:{ scalar[Globals.CHNLMAP["MRK"]].ToString("F2")}");


      return Globals.OK;
    }
    private static double GetTemp(List<object> data, int indx) {
      var dataUnit = (Dictionary<string, object>)data[indx]; ;
      var scalar = (Dictionary<int, double>)dataUnit["sclr"];
      return scalar[Globals.CHNLMAP["TMP"]];
    }

    private static void SetSlopeAndIntercept(List<object> data, int startIndx) {
      double Sx = 0;
      double Sy = 0;
      double Sxy = 0;
      double Sx2 = 0;
      for(int i = 0; i < 5; i++) {
        double temp = GetTemp(data, startIndx + i);
        Sx += startIndx + i;
        Sy += temp;
        Sxy += temp * (startIndx + i);
        Sx2 += (startIndx + i) * (startIndx + i);
      }
      tempSlope = (5 * Sxy - Sx * Sy) / (5 * Sx2 - Sx * Sx);
      tempIntercept = (Sy - tempSlope * Sx) / 5;
      tempFrstPnt = startIndx;
    }

    public static void Quit() {
      if(mTTLLive != null) {
        mTTLLive.CloseConnections();
        mTTLLive = null;
      }
    }
  }
}
