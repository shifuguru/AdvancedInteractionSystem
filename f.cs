using GTA;
using GTA.Native;
using System;
using System.Windows.Forms;

public class ModName : Script
{
  private bool engineoff = false;
  private bool engineon = true;
  public static Keys MenuKey;
  public static ScriptSettings iniSettings;
  private Ped playerPed = Game.Player.Character;
  private Player player = Game.Player;

  public ModName()
  {
    SetupScript();
    Aborted += new EventHandler(OnAbort);
    Tick += new EventHandler(OnTick);
    KeyDown += new KeyEventHandler(OnKeyDown);
    KeyUp += new KeyEventHandler(OnKeyUp);
  }

  private void OnAbort(object sender, EventArgs e)
  {
  }

  private void SetupScript()
  {
    ModName.iniSettings = ScriptSettings.Load("scripts\\enginehotkey.ini");
    Keys result;
    Enum.TryParse<Keys>(ModName.iniSettings.GetValue("KEYS", "MAIN", "N"), out result);
    ModName.MenuKey = result;
  }

  private void OnTick(object sender, EventArgs e)
  {
    bool flag = Function.Call<bool>(Hash.IS_USING_KEYBOARD_AND_MOUSE, InputArgument.op_Implicit(2));
    if (Game.IsControlJustPressed(2, (Control) 233) && Game.Player.Character.IsInVehicle() && Game.Player.Character.CurrentVehicle.EngineRunning && !flag)
    {
      Function.Call(Hash.REQUEST_ANIM_DICT, "veh@std@ds@base");
      Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character, "veh@std@ds@base", "change_station", 8f, 1f, 600, 48, 0.1f, 0, 0, 0);
      engineoff = true;
      engineon = false;
    }
    if (Game.IsControlJustPressed(2, (Control) 233) && Game.Player.Character.IsInVehicle() && !Game.Player.Character.CurrentVehicle.EngineRunning && !flag)
    {
      Function.Call(Hash.REQUEST_ANIM_DICT, "veh@std@ds@base");
      Function.Call(Hash.TASK_PLAY_ANIM, InputArgument.op_Implicit(Game.Player.Character), "veh@std@ds@base", InputArgument.op_Implicit("change_station"), InputArgument.op_Implicit(8f), InputArgument.op_Implicit(1f), InputArgument.op_Implicit(650), InputArgument.op_Implicit(48), InputArgument.op_Implicit(0.1f), InputArgument.op_Implicit(0), InputArgument.op_Implicit(0), InputArgument.op_Implicit(0));
      engineon = true;
      engineoff = false;
    }
    if (engineoff)
    {
      Function.Call(Hash.SET_VEHICLE_ENGINE_ON, InputArgument.op_Implicit(Game.Player.Character.CurrentVehicle), InputArgument.op_Implicit(false), InputArgument.op_Implicit(false), InputArgument.op_Implicit(true));
      engineon = false;
    }
    if (engineon)
    {
      Function.Call(Hash.SET_VEHICLE_ENGINE_ON, InputArgument.op_Implicit(Game.Player.Character.CurrentVehicle), InputArgument.op_Implicit(true), InputArgument.op_Implicit(false), InputArgument.op_Implicit(true));
      engineoff = false;
    }
    if (!Game.Player.Character.IsOnFoot || !engineon)
      return;
    Function.Call(Hash.SET_VEHICLE_ENGINE_ON, InputArgument.op_Implicit(Game.Player.Character.LastVehicle), InputArgument.op_Implicit(true), InputArgument.op_Implicit(true), InputArgument.op_Implicit(false));
  }

  private void OnKeyDown(object sender, KeyEventArgs e)
  {
  }

  private void OnKeyUp(object sender, KeyEventArgs e)
  {
    if (e.KeyCode == ModName.MenuKey && Game.Player.Character.IsInVehicle() && Game.Player.Character.CurrentVehicle.EngineRunning)
    {
      Function.Call(Hash.REQUEST_ANIM_DICT, "veh@std@ds@base");
      Function.Call(Hash.TASK_PLAY_ANIM, InputArgument.op_Implicit(Game.Player.Character), "veh@std@ds@base", InputArgument.op_Implicit("change_station"), InputArgument.op_Implicit(8f), InputArgument.op_Implicit(1f), InputArgument.op_Implicit(600), InputArgument.op_Implicit(48), InputArgument.op_Implicit(0.1f), InputArgument.op_Implicit(0), InputArgument.op_Implicit(0), InputArgument.op_Implicit(0));
      engineoff = true;
      engineon = false;
    }
    if (e.KeyCode != ModName.MenuKey || !Game.Player.Character.IsInVehicle() || Game.Player.Character.CurrentVehicle.EngineRunning)
      return;
    Function.Call(Hash.REQUEST_ANIM_DICT, "veh@std@ds@base");
    Function.Call(Hash.TASK_PLAY_ANIM, InputArgument.op_Implicit(Game.Player.Character), "veh@std@ds@base", InputArgument.op_Implicit("change_station"), InputArgument.op_Implicit(8f), InputArgument.op_Implicit(1f), InputArgument.op_Implicit(650), InputArgument.op_Implicit(48), InputArgument.op_Implicit(0.1f), InputArgument.op_Implicit(0), InputArgument.op_Implicit(0), InputArgument.op_Implicit(0));
    engineon = true;
    engineoff = false;
  }

  private void DisplayHelpTextThisFrame(string text)
  {
      Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, InputArgument.op_Implicit("STRING"));
    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, InputArgument.op_Implicit(text));
    Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, InputArgument.op_Implicit(0), InputArgument.op_Implicit(0), InputArgument.op_Implicit(1), InputArgument.op_Implicit(-1));
  }
}
