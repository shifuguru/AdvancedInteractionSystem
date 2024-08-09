// Decompiled with JetBrains decompiler
// Type: IVStyleExit
// Assembly: IVStyleExit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 108BE1E2-D7D3-404E-B154-70D79620BE0E
// Assembly location: C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts\IVStyleExit.dll

using GTA;
using System;
using System.IO;

#nullable disable
public class IVStyleExit : Script
{
  public bool closeDoorOnExit;
  private bool bypass;
  private int tickCount;
  private Ped player;
  private Vehicle veh;
  private readonly string defaultContent = "; 1 = enable; 0 = disable" + "?closeDoorOnExit=1";

  public IVStyleExit()
  {
    readConfig();
    Tick += new EventHandler(OnTick);
    Interval = 1;
  }

  public void OnTick(object sender, EventArgs e)
  {
    player = Game.Player.Character;
    veh = player.CurrentVehicle;

    if (veh != null && veh.GetPedOnSeat((VehicleSeat) -1) == player))
    {
      bool flag = Game.IsControlPressed((Control) 75);
      if (bypass | flag && veh.LockStatus == 1)
      {
        if (!flag)
          player.Task.LeaveVehicle(veh, closeDoorOnExit);
        ++tickCount;
        if (tickCount < 11)
        {
          bypass = true;
          veh.IsEngineRunning = true;
          return;
        }
        if (tickCount < 211)
          return;
        veh.IsEngineRunning = !flag;
        if (tickCount < 1211 && !veh.IsEngineRunning)
          return;
        veh.IsEngineRunning = false;
      }
    }
    bypass = false;
    tickCount = 0;
  }

  private void readConfig()
  {
    string path = BaseDirectory + "\\IVStyleExit.custom";
    if (!File.Exists(path))
    {
      createConfig();
    }
    else
    {
      try
      {
        string[] strArray = File.ReadAllLines(path);
        int num = 0;
        foreach (string str1 in strArray)
        {
          ++num;
          string str2 = str1.Trim();
          if (str2[0] != ';' && !string.IsNullOrEmpty(str2) && str2[0] == '?' && str2.StartsWith("?closeDoorOnExit=") && str2.Substring("?closeDoorOnExit=".Length)[0] == '1')
          {
            closeDoorOnExit = true;
            break;
          }
        }
      }
      catch
      {
      }
    }
  }

  private void createConfig()
  {
    string path = BaseDirectory + "\\IVStyleExit.custom";
    try
    {
      File.Create(path).Close();
      File.WriteAllText(path, defaultContent);
    }
    catch
    {
    }
  }
}
