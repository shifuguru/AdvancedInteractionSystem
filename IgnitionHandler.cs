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
        public static int exitHeldTime;
        public static bool toggleInProgress;
        public static bool bypass;

        // IGNTION CONTROL: 

        public static void IVExit(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null || !vehicle.Exists())
                {
                    exitHeldTime = 0;
                    return;
                }

                if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.Player.Character)
                {
                    bool exitHeld = Game.IsControlPressed(Control.VehicleExit);

                    if (bypass || exitHeld && vehicle.LockStatus.Equals(1))
                    {
                        if (!exitHeld)
                        {
                            Game.Player.Character.Task.LeaveVehicle();
                        }
                        ++exitHeldTime;
                        // Game.Player.Character.Task.LeaveVehicle(vehicle, true);
                        if (exitHeldTime < 11)
                        {
                            bypass = true;
                            vehicle.IsEngineRunning = true;
                            // LeaveEngineRunning(vehicle);
                            return;
                        }
                        if (exitHeldTime < 211) return;
                        vehicle.IsEngineRunning = !exitHeld;
                        if (exitHeldTime < 1211 && !vehicle.IsEngineRunning) return;
                        vehicle.IsEngineRunning = false;
                    }
                }
                bypass = false;
                exitHeldTime = 0;
            }
            catch (Exception ex)
            {
                AIS.LogException("IgnitionHandler.IVExit", ex);
                engineToggled = false;
            }
        }
        public static void ToggleIgnition(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null || !vehicle.Exists() || LemonMenu.pool.AreAnyVisible) 
                    return;

                toggleInProgress = true;

                float engineHealth = vehicle.EngineHealth;
                float engineTemp = vehicle.EngineTemperature;
                bool isTempSafe = engineTemp >= 5f && engineTemp <= 110f;

                // TURN ENGINE OFF: 
                if (vehicle.IsEngineRunning)
                {
                    Game.Player.Character.Task.PlayAnimation("veh@std@ds@base", "start_engine", 0, 0, 0, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 1);
                    N.SetVehicleEngineOn(vehicle, false, false, true);
                }
                // TURN ENGINE ON: 
                else if (!vehicle.IsEngineRunning)
                {
                    Game.Player.Character.Task.PlayAnimation("veh@std@ds@base", "start_engine", 0, 0, 0, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 1);
                    IgnitionStartupChecks(vehicle);
                    N.SetVehicleEngineOn(vehicle, true, false, true);
                }
                if (SettingsManager.ignition_debugEnabled)
                {
                    N.ShowSubtitle("Turning key in vehicle's ignition.", 1000);
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
                if (vehicle == null || !vehicle.Exists())
                    return;

                // VEHICLE HEALTH:
                if (vehicle.IsEngineRunning)
                {
                    // ENGINE HEALTH:
                    if (vehicle.EngineHealth <= (vehicle.EngineHealth * 0.2f))
                    {
                        Notification.Show($"~y~Warning!~s~ Engine Health at Critical Level: ~r~{vehicle.EngineHealth}~s~.", false);
                    }
                    // PETROL TANK HEALTH:
                    if (vehicle.PetrolTankHealth <= (vehicle.PetrolTankHealth * 0.9f))
                    {
                        Notification.Show($"~y~Warning!~s~ Fuel Tank damaged: ~r~{vehicle.PetrolTankHealth}~s~.", false);
                    }
                    // TYRE PRESSURE: 
                    InteractionHandler.TyrePressureMonitoringSystem(vehicle);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("IgnitionHandler.IgnitionStartupChecks", ex);
            }
        }
        
        public static void HandleIgnition(Vehicle vehicle)
        {
            try
            {
                // While the Control is held:
                if (Game.IsControlPressed(ignitionControl))
                {
                    if (!ignitionHeld && !toggleInProgress)
                    {
                        ignitionHeld = true;
                        toggleInProgress = false;
                        ignitionHeldStartTime = DateTime.Now;
                    }
                    else
                    {
                        double heldDuration = (DateTime.Now - ignitionHeldStartTime).TotalSeconds;
                        if (heldDuration >= ignitionHoldDuration && !toggleInProgress)
                        {
                            // Toggle Ignition State:
                            ToggleIgnition(vehicle);
                            ignitionHeld = false;
                            // toggleInProgress = true;
                        }
                    }
                }
                else
                {
                    ignitionHeld = false;
                    toggleInProgress = false;
                }
                
                if (SettingsManager.ignitionByThrottleEnabled)
                {
                    if (Game.IsControlPressed(Control.VehicleAccelerate) && !vehicle.IsEngineRunning)
                    {
                        N.SetVehicleEngineOn(Game.Player.Character.CurrentVehicle, true, false, true);
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.HandleIgnition", ex);
                ignitionHeld = false;
                toggleInProgress = false;
            }
        }
    }
}
