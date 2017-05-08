using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmosc {
  public static class Util {

    //public static int CrtRsltDataDirectory() {
    //  Globals.subjDirPath = Path.Combine(Globals.DATAROOTPATH, Globals.fileNameTS);
    //  try {
    //    if(!Directory.Exists(Globals.subjDirPath)) {
    //      Directory.CreateDirectory(Globals.subjDirPath);
    //      Logger.Msg($"Created subject's data directory: {Globals.subjDirPath}");
    //    }
    //  } catch(Exception e) {
    //    return Logger.Err(501);
    //  }
    //  return Globals.OK;
    //}

    public static int CrtRsltAudioFileName() {
      //Globals.rsltAudioFileName = Path.Combine(Globals.subjDirPath, "audio_"+ Globals.fileNameTS);
      //Globals.rsltAudioFileName = Globals.rsltAudioFileName.Replace('\\', '/');
      Globals.rsltAudioFileName = Globals.fileNameTS + "/audio_" + Globals.fileNameTS;
      return Globals.OK;
    }

    private static int DataUnitToFile(Dictionary<string, object> du, int index, double level,
      System.IO.StreamWriter file, System.IO.StreamWriter raw) {

      string resLine = $"{index},";
      string resRawLine = $"{index},";

      // process scalar
      var scalar = (Dictionary<int, double>)du["sclr"];
      foreach(KeyValuePair<string, int> channel in Globals.CHNLMAP) {
        if(channel.Key != "BVP") {
          if(channel.Key != "DSMEDA") {
            resLine += scalar[channel.Value].ToString("F3") + ",";
            resRawLine += scalar[channel.Value].ToString("F3") + ",";
          } else {
            resLine += scalar[channel.Value].ToString("e3") + ",";
            resRawLine += scalar[channel.Value].ToString("e3") + ",";
          }

        } else {
          resLine += ("{0},");
        }
      }
      //if(scalar[Globals.CHNLMAP["MRK"]] >= 3 && scalar[Globals.CHNLMAP["MRK"]] < 5 ||
      //  scalar[Globals.CHNLMAP["MRK"]] == 2.0) {
      if(scalar[Globals.CHNLMAP["MRK"]] >= 3 && scalar[Globals.CHNLMAP["MRK"]] < 5) {
        raw.WriteLine(resRawLine);
      }

      // now let's take care of the verctor portion of the data
      var vector = (double[])du["vctr"];
      for(int j = 0; j < Globals.DATALENGTHBVP; j++) {
        try {
          if(vector[j] != 0) {
            if(level == 0) {
              file.WriteLine(resLine, vector[j].ToString("F3"));
            } else {
              //Console.WriteLine(resLine, vector[j].ToString("F3"));
              if(scalar[Globals.CHNLMAP["MRK"]] == level) {
                file.WriteLine(resLine, vector[j].ToString("F3"));
              }
            }
          }
        } catch(Exception e) {
          return Logger.Err(502);
        }
      }
      return Globals.OK;
    }

    public static string SaveData(List<object> data, double level = 0) {

      string fileName = "data_";
      string raw = "data_";
      if(level == 0) {
        fileName += "all_";
        raw += "raw_";
      } else {
        fileName += $"test_{level.ToString("F1")}_";
        fileName.Replace('.', '_');
      }
      fileName += $"{Globals.fileNameTS}.csv";
      raw += $"{Globals.fileNameTS}.csv";
      string fullFilename = Path.Combine(Globals.subjDirPath, fileName);
      string rawFilename = Path.Combine(Globals.subjDirPath, raw);
      System.IO.StreamWriter file = null;
      System.IO.StreamWriter rawFile = null;
      //try {
      file = new System.IO.StreamWriter(fullFilename);
      rawFile = new System.IO.StreamWriter(rawFilename);
      //}catch(Exception e) {
      //  return Logger.Err(503);
      //}
      // create header
      string resLine = "counter,";
      string resLineRaw = "counter,";
      foreach(KeyValuePair<string, int> channel in Globals.CHNLMAP) {
        resLine += channel.Key + ',';
        if(channel.Key != "BVP") {
          resLineRaw += channel.Key + ',';
        }
      }
      file.WriteLine(resLine);
      rawFile.WriteLine(resLineRaw);

      for(int indx = 0; indx < Globals.DATALENGTH; indx++) {
        DataUnitToFile((Dictionary<string, object>)data[indx], indx, level, file, rawFile);
      }


      file.Close();
      rawFile.Close();

      return fullFilename;
    }

    public static void SaveRslt() {
      string fileName = $"dmgr_{Globals.fileNameTS}.json";
      string fullFilename = Path.Combine(Globals.subjDirPath, fileName);
      System.IO.StreamWriter file = null;
      file = new System.IO.StreamWriter(fullFilename);
      string rslt = $"{{\"sex\":{Globals.DMGR["sex"]},\"age\":{Globals.DMGR["age"]}";
      for(int i = 0; i < 4; i++) {
        var expr = (Dictionary<string, string>)Globals.SBJEXPERIENCE[i];
        rslt += $",\"VIDEO_{i + 1}\":{Globals.VIDEOSEQUENCE[i]},\"EXPR_CTGR_{i + 1}\":{expr["ctgr"]},\"EXPR_VAL_{i + 1}\":{expr["exp"]}";
      }
      rslt += "}}";
      file.WriteLine(rslt);
      file.Close();
    }

    public static void OpenLogFile() {
      string path;
      try {
        path = Path.GetDirectoryName(Globals.settings["DATAROOTPATH"] + '\\');
        if(!Directory.Exists(path)) {
          Directory.CreateDirectory(path);
        }
        Console.WriteLine($"+++++> root path: {path}");
      } catch(Exception e) {
        // get working directory
        path = System.Reflection.Assembly.GetExecutingAssembly().Location;
        path = Path.GetDirectoryName(path);
      }

      string logDir = Path.Combine(path, "logs");
      try {
        if(!Directory.Exists(logDir)) {
          Directory.CreateDirectory(logDir);
          Logger.Msg($"Created logs directory: {logDir}");
        }
      } catch(Exception e) {
        Logger.Err(504);
      }
      //return Globals.OK;

      string logFileName = Path.Combine(logDir, $"log_{DateTime.Now.ToString("yyMMdd_HHmmss")}.txt");

      try {
        Globals.logFile = new System.IO.StreamWriter(logFileName);
        Logger.Msg($"Created log file: {logFileName}");
      } catch(Exception e) {
        Logger.Err(505);
        //Logger.Msg(e.Data)
      }

    }
    public static void DeleteData() {
      string[] files = Directory.GetFiles(Globals.subjDirPath);
      foreach(string file in files) {
        File.Delete(file);
      }
      Directory.Delete(Globals.subjDirPath);
    }
  }
}

