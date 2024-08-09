using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LemonUI;
using LemonUI.Menus;
using iFruitAddon2;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;
using System.Threading;

namespace AdvancedInteractionSystem
{
    public class IgnitionHandler : Script
    {
        public static bool ignitionHeld; // READONLY
        public static bool engineToggled; // READONLY
        public static DateTime ignitionHeldStartTime; // READONLY
        public const float ignitionHoldDuration = 2.5f;
        public static Control ignitionControl = Control.Context;
        public static int tickCount;
        public static bool bypass;

        // IGNTION CONTROL: 
        public static void IVExit(Ped player, Vehicle vehicle, bool debugEnabled)
        {
            try
            {
                if (vehicle == null || player == null)
                {
                    ignitionHeld = false;
                    engineToggled = false;
                    return;
                }
                if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == player)
                {
                    bool exitHeld = Game.IsControlPressed(Control.VehicleExit);
                    if (bypass || exitHeld && vehicle.LockStatus.Equals(1))
                    {
                        if (!exitHeld)
                        {
                            player.Task.LeaveVehicle(vehicle, true);
                        }
                        ++tickCount;
                        if (tickCount < 11)
                        {
                            bypass = true;
                            vehicle.IsEngineRunning = true;
                            return;
                        }
                        if (tickCount < 211) return;
                        vehicle.IsEngineRunning = !exitHeld;
                        if (tickCount < 1211 && !vehicle.IsEngineRunning) return;
                        vehicle.IsEngineRunning = false;
                    }
                }
                bypass = false;
                tickCount = 0;
            }
            catch (Exception ex)
            {
                AIS.LogException("IgnitionHandler.IVExit", ex);
                ignitionHeld = false;
                engineToggled = false;
            }
        }
        public static void ToggleIgnition(Vehicle vehicle, bool debugEnabled)
        {
            try
            {
                if (vehicle == null) return;
                if (vehicle.Exists())
                {
                    // TURN ENGINE OFF: 
                    if (vehicle.IsEngineRunning)
                    {
                        Function.Call(Hash.SET_VEHICLE_ENGINE_ON, vehicle, false, false, true);
                        if (debugEnabled)
                        {
                            Screen.ShowSubtitle("Engine ~r~Off~s~.", 2000);
                        }
                    }
                    // TURN ENGINE ON: 
                    if (!vehicle.IsEngineRunning)
                    {
                        Game.Player.Character.Task.PlayAnimation("veh@std@ds@base", "start_engine", 0, 0, 0, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 1);
                        Function.Call(Hash.SET_VEHICLE_ENGINE_ON, vehicle, true, false, true);
                        if (debugEnabled)
                        {
                            Screen.ShowSubtitle("Engine ~g~On~s~.", 2000);
                        }
                    }

                    IgnitionStartupChecks(vehicle);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("IgnitionHandler.ToggleIgnition", ex);
            }
        }
        public static void IgnitionStartupChecks(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null)
                    return;
                if (vehicle != null && vehicle.Exists())
                {
                    // VEHICLE START UP CHECKS: 
                    // ENGINE HEALTH:
                    if (vehicle.IsEngineRunning)
                    {
                        if (vehicle.EngineHealth <= (vehicle.EngineHealth * 0.2f))
                        {
                            Notification.Show($"~y~Warning!~s~ Engine Health at Critical Level: ~r~{vehicle.EngineHealth}~s~.", false);
                        }
                        // TYRES:
                        bool lf_burst = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 0, true); // Left-Front Tyre Burst Completely
                        bool rf_burst = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 1, true); // Right-Front Tyre Burst Completely
                        bool lr_burst = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 4, true); // Left-Rear Tyre Burst Completely
                        bool rr_burst = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 5, true); // Right-Rear Tyre Burst Completely
                        bool lf_leak = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 0, false); // Left-Front Tyre Burst 
                        bool rf_leak = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 1, false); // Right-Front Tyre Burst 
                        bool lr_leak = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 4, false); // Left-Rear Tyre Burst 
                        bool rr_leak = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 5, false); // Right-Rear Tyre Burst 

                        string tyrePressure = $"Tyre Pressure: ";

                        if (lf_leak || rf_leak || lr_leak || rr_leak)
                        {
                            tyrePressure += $"~r~Severe Warning!~s~";
                            // at least 1 tyre is leaking. Warning! ~y~
                        }
                        if (lf_burst || rf_burst || lr_burst || rr_burst)
                        {
                            // at least 1 tyre burst. Severe warning! ~r~
                        }

                        // PETROL TANK HEALTH:
                        if (vehicle.PetrolTankHealth <= (vehicle.PetrolTankHealth * 0.9f))
                        {
                            Notification.Show($"~y~Warning!~s~ Fuel Tank damaged: ~r~{vehicle.PetrolTankHealth}~s~.", false);
                        }
                        // FUEL:     
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("IgnitionHandler.IgnitionStartupChecks", ex);
            }
        }
    }
}
