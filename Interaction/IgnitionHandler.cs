using System;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Control = GTA.Control;

namespace AdvancedInteractionSystem
{
    public class IgnitionHandler : Script
    {
        public static bool closeDoorOnExit = true;
        // public static bool disableAutoStart = SettingsManager.disableAutoStart;
        public static Control ignitionControl = Control.VehicleHeadlight;


        public static bool ignitionHeld; // READONLY
        public const float ignitionHoldDuration = 2.5f; // Time ignition conrol must be held for, in seconds
        public static DateTime ignitionHeldStartTime; // READONLY
                
        public static int exitHeldTime = 0; // time exit button is held for
        public static int engineDelayTime = 0; // time delay of engine shutting off after holding Exit
        public static int exitDelayTime = 0; // time before exiting the vehicle
        public static int ignitionCooldown = 0; // time before ignition can be toggled again 

        public static bool toggleInProgress = false; 
        public static bool keepEngineRunning = false; // previously named 'bypass'. 
        public static bool eng = false;
        public static int enforce = 0;
        public static int delay = 0;
        public static int rest = 0;
        public static bool isPlayerDriving = false; 

        // IGNTION CONTROL: 
        public static void DisableAutoStart()
        {
            if (SettingsManager.disableAutoStart)
            {
                Vehicle vehicle = Function.Call<Vehicle>(Hash.GET_VEHICLE_PED_IS_TRYING_TO_ENTER, Game.Player.Character);
                if (vehicle != null && vehicle.Exists())
                {
                    N.SetVehicleEngineOn(vehicle, vehicle.IsEngineRunning, false, SettingsManager.disableAutoStart);
                }
            }
        }

        public static void IVExit(Vehicle vehicle)
        {
            if (!SettingsManager.IVExit) return;

            try
            {
                isPlayerDriving = vehicle != null && vehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.Player.Character;

                if (isPlayerDriving)
                {
                    bool exitHeld = Game.IsControlPressed(Control.VehicleExit);

                    if (keepEngineRunning || exitHeld && vehicle.LockStatus.Equals(1))
                    {
                        // Reset the Exit Held Time
                        if (!exitHeld)
                        {
                            Game.Player.Character.Task.LeaveVehicle(vehicle, true);
                        }

                        ++exitHeldTime;

                        if (exitHeldTime < 11)
                        {
                            keepEngineRunning = true;
                            vehicle.IsEngineRunning = true;
                            return;
                        }

                        if (exitHeldTime < 211) return;

                        vehicle.IsEngineRunning = !exitHeld;

                        if (exitHeldTime < 1211 && !vehicle.IsEngineRunning) return;

                        vehicle.IsEngineRunning = false;
                    }
                }

                keepEngineRunning = false;
                exitHeldTime = 0;
                rest = 0;
                delay = 0;
                enforce = 0;
            }
            catch (Exception ex)
            {
                AIS.LogException("IgnitionHandler.IVExit", ex);
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

                bool isEngineOn = vehicle.IsEngineRunning;

                Game.Player.Character.Task.PlayAnimation("veh@std@ds@base", "start_engine", 0, 0, 0, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 1);
                N.SetVehicleEngineOn(vehicle, !isEngineOn, false, SettingsManager.disableAutoStart);


                // TURN ENGINE OFF: 
                if (vehicle.IsEngineRunning)
                {
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
