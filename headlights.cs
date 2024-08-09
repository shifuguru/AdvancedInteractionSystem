// Decompiled with JetBrains decompiler
// Type: DaytimeHeadlights
// Assembly: DaytimeHeadlights, Version=1.2.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 60A51E4C-5B90-46D5-A508-43292AEFD776
// Assembly location: C:\Users\Admin\Downloads\-DaytimeHeadlights_1.3.1\DaytimeHeadlights.dll

using GTA;
using GTA.Native;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

#nullable disable
public class DaytimeHeadlights : Script
{
  private bool enabled_mod = true;
  private bool modEnabled;
  private MenuPool _menuPool;
  private UIMenu mainMenu;
  private bool OtherVehiclesActive;
  private string toolkey;
  private string tgkey;
  private Keys toggleKey;
  private Keys toggle_menu;
  private string int_ensity;
  private int intensity;
  private bool oth_vehs_xenon;
  private string copy_menu;
  private string copy_head;
  private string toggle_head_to_show;
  private string toggle_menu_to_show;

  public DaytimeHeadlights()
  {
    ScriptSettings scriptSettings = ScriptSettings.Load("scripts\\DaytimeHeadlights.config");
    modEnabled = scriptSettings.GetValue<bool>("SETTINGS", "Start", true);
    OtherVehiclesActive = scriptSettings.GetValue<bool>("SETTINGS", "OtherVehicles", true);
    toggleKey = scriptSettings.GetValue<Keys>("SETTINGS", "ToggleKey", Keys.G);
    intensity = scriptSettings.GetValue<int>("SETTINGS", "Lights", 1);

    KeyDown += OnKeyDown;
    KeyUp += OnKeyUp;
    Tick += OnTick;
    Interval = 10;
  }

  private void OnKeyDown(object sender, KeyEventArgs e)
  {
    toggleKey = ScriptSettings.Load("scripts\\DaytimeHeadlights.config").GetValue<Keys>("SETTINGS", "ToggleKey", Keys.G);
    if (e.KeyCode != toggleKey || !modEnabled) return;

    if (Game.Player.Character.IsInVehicle() && enabled_mod)
    {
      enabled_mod = false;
      Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.Character.CurrentVehicle, 1);
      UI.ShowSubtitle("Headlights disabled");
    }
    else if (Game.Player.Character.IsInVehicle() && !enabled_mod)
    {
      enabled_mod = true;
      Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.Character.CurrentVehicle, 2);
      UI.ShowSubtitle("Headlights enabled");
    }
  }

  private void OnKeyUp(object sender, KeyEventArgs e)
  {
    toggle_menu = ScriptSettings.Load("scripts\\DaytimeHeadlights.config").GetValue<Keys>("SETTINGS", "Toggle_Menu", Keys.F5);
    if (e.KeyCode != toggle_menu || _menuPool.IsAnyMenuOpen()) return;
    StartUpMenu();
  }

  private void OnTick(object sender, EventArgs e)
  {
    _menuPool.ProcessMenus();

    if (OtherVehiclesActive && modEnabled && !oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
          Function.Call(Hash.SET_VEHICLE_LIGHTS, nearbyVehicle, 3);
      }
    }
    if (OtherVehiclesActive && modEnabled && oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
        {
          Function.Call(Hash.TOGGLE_​VEHICLE_​MOD, nearbyVehicle, 22, true);
          Function.Call(Hash.SET_VEHICLE_LIGHTS, nearbyVehicle, 3);
        }
      }
    }

    if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, Game.Player.LastVehicle) && enabled_mod && modEnabled)
      Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.LastVehicle, 2);

    else if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, Game.Player.LastVehicle) && !enabled_mod && modEnabled)
      Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.LastVehicle, 1);

    else if (!Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, Game.Player.LastVehicle) && enabled_mod && modEnabled)
      Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.LastVehicle, 1);

    if (!modEnabled)
    {
      Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.LastVehicle, 0);
      Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.Character.CurrentVehicle, 0);
    }
    if (intensity == 1 && modEnabled && !oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.2);
      }
    }
    else if (intensity == 2 && modEnabled && !oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.4);
      }
    }
    else if (intensity == 3 && modEnabled && !oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.6);
      }
    }
    else if (intensity == 4 && modEnabled && !oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.8);
      }
    }
    else if (intensity == 5 && modEnabled && !oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 1.0);
      }
    }
    else if (intensity == 1 && modEnabled && oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
        {
          Function.Call(Hash.TOGGLE_​VEHICLE_​MOD, nearbyVehicle, 22, true);
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.2);
        }
      }
    }
    else if (intensity == 2 && modEnabled && oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
        {
          Function.Call(Hash.TOGGLE_​VEHICLE_​MOD, nearbyVehicle, 22, true);
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.4);
        }
      }
    }
    else if (intensity == 3 && modEnabled && oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
        {
          Function.Call(Hash.TOGGLE_​VEHICLE_​MOD, nearbyVehicle, 22, true);
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.6);
        }
      }
    }
    else if (intensity == 4 && modEnabled && oth_vehs_xenon)
    {
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
        {
          Function.Call(Hash.TOGGLE_​VEHICLE_​MOD, nearbyVehicle, 22, true);
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 0.8);
        }
      }
    }
    else
    {
      if (intensity != 5 || !modEnabled || !oth_vehs_xenon)
        return;
      foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
      {
        if (Function.Call<bool>(Hash.GET_​IS_​VEHICLE_​ENGINE_​RUNNING, nearbyVehicle))
        {
          Function.Call(Hash.TOGGLE_​VEHICLE_​MOD, nearbyVehicle, 22, true);
          Function.Call(Hash.SET_​VEHICLE_​LIGHT_​MULTIPLIER, nearbyVehicle, 1.0);
        }
      }
    }
  }
