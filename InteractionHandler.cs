using System;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;
using LemonUI;
using System.Threading;
using LemonUI.Elements;

namespace AdvancedInteractionSystem
{
    public class InteractionHandler : Script
    {
        public static bool handler_debugEnabled = SettingsManager.handler_debugEnabled;
        // CONTROLS: 
        private static Control flipControl;
        //private static Control studyControl; // No longer used, car is studied while interacting. 
        private static Control cleanControl;
        private static Control repairControl;
        private static Control doorControl;
        private static Control ignitionControl;

        public static float interactionDistance = 8f;
        public static DateTime ignitionHeldStartTime; // READONLY
        public static bool ignitionHeld; // READONLY
        public static bool isEngineOff = false;
        public static bool toggleInProgress = false;
        public static float ignitionHoldDuration = 1.5f;
        // handle locked vehicle: 
        public static bool locked = false;

        public InteractionHandler()
        {
            flipControl = SettingsManager.flipControl;
            // studyControl = SettingsManager.studyControl;
            cleanControl = SettingsManager.cleanControl;
            repairControl = SettingsManager.repairControl;
            doorControl = SettingsManager.doorControl;
            ignitionControl = SettingsManager.ignitionControl;
        }

        // audio - BARRY3C_IGNITION_FAIL

        public static void HandleInVehicle(Vehicle currentVehicle)
        {
            try
            {
                if (currentVehicle == null || !currentVehicle.Exists()) return;
                
                bool engineRunning = currentVehicle.IsEngineRunning;
                float engineHealth = currentVehicle.EngineHealth;
                double engineTemp = Math.Round(currentVehicle.EngineTemperature, 1);
                bool isTempSafe = engineTemp >= 5f && engineTemp <= 110f;
                bool isRefueling = Fuel.isRefueling;

                if (!isRefueling)
                {
                    HandleIgnition();

                    if (engineRunning)
                    {
                        if (!isTempSafe)
                        {
                            if (engineTemp < 5f)
                            {
                                N.DisplayNotification("Engine Temperature: ~b~Low~s~", false);
                            }
                            else if (engineTemp > 110f)
                            {
                                N.DisplayNotification("Engine Temperature: ~r~High~s~", false);
                            }
                        }
                        if (engineHealth < 400)
                        {
                            N.ShowHelpText("~r~Warning: Engine condition critical. Service immediately.~s~");
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.HandleInVehicle()", ex);
            }
        }

        public static void HandleOnFoot(Vehicle closestVehicle)
        {
            try
            {
                if (closestVehicle == null || !closestVehicle.Exists()) return;

                Ped GPC = Game.Player.Character;
                float distance = GPC.Position.DistanceTo(closestVehicle.Position);
                bool vehicleClose = distance < interactionDistance;
                bool facingVehicle = IsPlayerFacingVehicle(closestVehicle);
                bool controlHeld = Game.IsControlPressed(Control.Aim);

                if (facingVehicle && vehicleClose)
                {
                    if (controlHeld)
                    {
                        if (GPC.Weapons.Current.LocalizedName == "Unarmed")
                        {
                            ShowInteractionOptions(closestVehicle);
                        }
                        else
                        {
                            if (SettingsManager.debugEnabled)
                            {
                                N.ShowSubtitle("Stow your weapon to interact with this vehicle", 200);
                            }
                        }
                    }
                }
                else
                {
                    if (Repairs.isRepairing || Cleaning.cleaning)
                    {
                        InteractionManager.CancelActions();
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.HandleOnFoot()", ex);
            }
        }

        public static bool IsPlayerFacingVehicle(Vehicle vehicle)
        {
            try
            {
                Ped GPC = Game.Player.Character;
                if (vehicle == null || GPC == null) return false;
                Vector3 position = GPC.Position;
                Vector3 forwardVector = GPC.ForwardVector;
                Vector3 directionToVehicle = vehicle.Position - position;
                directionToVehicle.Normalize();
                return Vector3.Dot(forwardVector, directionToVehicle) >= 0.5;
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.IsPlayerFacingVehicle", ex);
                return false;
            }
        }

        public static void ShowInteractionOptions(Vehicle closestVehicle)
        {
            try
            {
                InteractionManager.DisableControls();
                HandleInteractionControls(closestVehicle);
                HandleStudy(closestVehicle);
                
                HandlePrompt(closestVehicle);
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.ShowInteractionsOptions", ex);
            }
        }

        public static void HandlePrompt(Vehicle closestVehicle)
        {
            string clean_message = "Clean ~INPUT_MELEE_ATTACK_LIGHT~~n~";
            string repair_message = "Repair ~INPUT_SPRINT~~n~";
            string door_message = GetDoorMessage(closestVehicle);
            string flip_message = GetFlipMessage(closestVehicle);
            string message = $"{door_message}{repair_message}{clean_message}";
            N.ShowHelpText($"{message}");
        }

        public static void HandleInteractionControls(Vehicle closestVehicle)
        {
            try
            {
                // REPAIR:
                if (Game.IsControlJustPressed(repairControl))
                {
                    Repairs.StartRepairProcess(closestVehicle);
                }

                // CLEAN: 
                if (Game.IsControlJustPressed(cleanControl))
                {
                    HandleCleaning();
                }

                // FLIP: 
                if (Game.IsControlJustPressed(flipControl))
                {
                    HandleFlipping(closestVehicle);
                }

                // DOORS: 
                HandleDoorInteraction(closestVehicle, doorControl);
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.HandleInteractionControls", ex);
            }
        }

        // INTERACTIONS: 

        public static void LookAtVehicle(Vehicle vehicle, int duration)
        {
            try
            {
                Game.Player.Character.Task.LookAt(vehicle?.Position ?? Vector3.Zero, duration);
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.LookAtVehicle", ex);
            }

        }
        public static void OpenVehicleDoors(Vehicle vehicle)
        {
            try
            {
                vehicle?.Doors[VehicleDoorIndex.Hood]?.Open(false, false);
                vehicle?.Doors[VehicleDoorIndex.Trunk]?.Open(false, false);
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.OpenVehicleDoors", ex);
            }

        }
        public static void CloseVehicleDoors(Vehicle vehicle)
        {
            try
            {
                if (vehicle.Doors[VehicleDoorIndex.Trunk].IsOpen)
                {
                    vehicle?.Doors[VehicleDoorIndex.Trunk]?.Close(false);
                }
                if (vehicle.Doors[VehicleDoorIndex.Hood].IsOpen)
                {
                    vehicle?.Doors[VehicleDoorIndex.Hood]?.Close(false);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.CloseVehicleDoors", ex);
            }

        }

        public static void LeaveEngineRunning(Vehicle vehicle)
        {
            if (!vehicle.IsEngineRunning)
            {
                N.SetVehicleEngineOn(vehicle, true, true, false);
            }
        }

        public static void DisableAutoStart(Vehicle vehicle)
        {
            if (vehicle.IsEngineRunning)
            {
                N.SetVehicleEngineOn(vehicle, false, true, false);
            }
        }



        public static void HandleStudy(Vehicle vehicle)
        {
            try
            {
                Model model = vehicle.Model;
                string makeName = N.GetMakeNameFromModel(vehicle.Model);
                string brand = N.GetGxtName(makeName);
                string name = vehicle.LocalizedName;
                float topSpeed = Function.Call<float>(Hash.GET_VEHICLE_ESTIMATED_MAX_SPEED, vehicle);
                int topSpeedMPH = (int)Math.Round(topSpeed * 2.23694f);
                int engineHealth = (int)Math.Round(100 * (vehicle.EngineHealth / 1000));
                NotificationIcon icon = NotificationIcon.Carsite;
                string sender = $"{brand} {name}";
                string subject = vehicle.ClassLocalizedName;
                string message = $"Top Speed: {topSpeedMPH} mph ~n~Engine Health: {engineHealth} %";
                bool displayMessage = false;

                if (displayMessage)
                {
                    N.DisplayNotificationSMS(
                    icon,
                    sender,
                    subject,
                    message,
                    false,
                    false
                    );
                }



                

            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.GetStudyMessage", ex);
            }
        }
        public static void HandleCleaning()
        {
            try
            {
                if (!Cleaning.cleaning && !Repairs.isRepairing)
                {
                    Cleaning.StartCleaning();
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.HandleCleaning", ex);
            }
        }

        // FLIPPING: 
        public static string GetFlipMessage(Vehicle vehicle)
        {
            try
            {
                if (vehicle.IsUpsideDown)
                {
                    string flip_message = "Flip Vehicle Hold ~INPUT_JUMP~ ~n~";
                    return flip_message;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.GetFlipMessage", ex);
                return string.Empty;
            }
        }
        public static void HandleFlipping(Vehicle vehicle)
        {
            try
            {
                if (vehicle.IsUpsideDown)
                {
                    vehicle.Rotation = Vector3.Zero;
                    vehicle.Velocity = Vector3.Zero;
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.HandleFlipping", ex);
            }
        }
        
        // DOORS: 
        public static string GetDoorMessage(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null) return string.Empty;
                if (N.IsVehicleLocked(vehicle))
                {
                    locked = true;
                    return "~INPUT_ENTER~ Unlock Vehicle";
                }

                string doorMessage = string.Empty;
                var closestBone = BoneHelper.GetClosestBone(vehicle, Game.Player.Character.Position);
                float distance = World.GetDistance(Game.Player.Character.Position, closestBone.Item2);

                if (closestBone == null || distance > 5f) return string.Empty;

                string boneName = closestBone.Item1;
                switch (boneName)
                {
                    case "door_dside_f":
                        doorMessage += $"Door ~INPUT_ENTER~~n~";
                        break;
                    case "door_dside_r":
                    case "door_pside_f":
                    case "door_pside_r":
                        doorMessage += $"Door ~INPUT_ENTER~~n~";
                        break;
                    case "boot":
                        doorMessage += $"Trunk ~INPUT_ENTER~~n~";
                        break;
                    case "bonnet":
                        doorMessage += $"Hood ~INPUT_ENTER~~n~";
                        break;
                    case "petroltank":
                        doorMessage += $"Fuel Tank ~INPUT_ENTER~~n~";
                        break;
                }
                return doorMessage;
            }
            catch (Exception ex)
            {
                AIS.LogException("GetDoorMessage", ex);
                return string.Empty;
            }
        }
        public static void HandleDoorInteraction(Vehicle vehicle, Control doorControl)
        {
            try
            {
                var closestBone = BoneHelper.GetClosestBone(vehicle, Game.Player.Character.Position);
                if (closestBone == null || vehicle.LockStatus != VehicleLockStatus.Unlocked) return;

                string boneName = closestBone.Item1;
                VehicleDoor door = null;

                switch (boneName)
                {
                    case "door_dside_f":
                        door = vehicle.Doors[VehicleDoorIndex.FrontLeftDoor];
                        break;
                    case "door_dside_r":
                        door = vehicle.Doors[VehicleDoorIndex.BackLeftDoor];
                        break;
                    case "door_pside_f":
                        door = vehicle.Doors[VehicleDoorIndex.FrontRightDoor];
                        break;
                    case "door_pside_r":
                        door = vehicle.Doors[VehicleDoorIndex.BackRightDoor];
                        break;
                    case "boot":
                        door = vehicle.Doors[VehicleDoorIndex.Trunk];
                        break;
                    case "bonnet":
                        door = vehicle.Doors[VehicleDoorIndex.Hood];
                        break;
                }
                
                if (handler_debugEnabled)
                {
                    N.DrawMarker(3, closestBone.Item2, 0.3f, 120, 120, 120, 80, false, true);
                }

                if (Game.IsControlJustPressed(doorControl) && door != null && !door.IsBroken)
                {
                    if (door.IsOpen)
                    {
                        door.Close(false);
                    }
                    else
                    {
                        door.Open(false, false);
                        vehicle.LockStatus = VehicleLockStatus.Unlocked;
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.HandleDoorInteraction", ex);
            }
        }

        // TOGGLE IGNITION: 
        private static readonly object toggleLock = new object();

        public static void ToggleIgnition()
        {
            try
            {
                lock (toggleLock)
                {
                    if (LemonMenu.pool.AreAnyVisible) return;
                    Vehicle currentVehicle = Game.Player.Character.CurrentVehicle;
                    if (currentVehicle == null || toggleInProgress) return;

                    toggleInProgress = true;

                    bool engineRunning = currentVehicle.IsEngineRunning;
                    float engineHealth = currentVehicle.EngineHealth;
                    float engineTemp = currentVehicle.EngineTemperature;
                    bool isTempSafe = engineTemp >= 5f && engineTemp <= 110f;

                    // if the engine is currently running we need to turn it off: 
                    if (engineRunning)
                    {
                        // Disable Ignition: 
                        N.SetVehicleEngineOn(Game.Player.Character.CurrentVehicle, false, false, true);
                        isEngineOff = true;
                    }
                    // engine is not running -> turn it on, as long as the engine is healthy: 
                    else if (!engineRunning && engineHealth > 0f && isTempSafe)
                    {
                        // Enable Ignition: 
                        N.SetVehicleEngineOn(Game.Player.Character.CurrentVehicle, true, false, true);
                        isEngineOff = false;
                    }
                    if (handler_debugEnabled)
                    {
                        N.ShowSubtitle("Turning key in vehicle's ignition.", 1000);
                    }
                }
                
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.ToggleIgnition", ex);
                isEngineOff = false;
                isEngineOff = false;
                toggleInProgress = false;
            }
            finally
            {
                toggleInProgress = false;
            }
        }
        public static void HandleIgnition()
        {
            try
            {
                // If Control is Held
                if (Game.IsControlPressed(ignitionControl))
                {
                    if (!ignitionHeld)
                    {
                        ignitionHeld = true;
                        ignitionHeldStartTime = DateTime.Now;
                    }
                    else
                    {
                        double heldDuration = (DateTime.Now - ignitionHeldStartTime).TotalSeconds;
                        if (heldDuration >= ignitionHoldDuration && !toggleInProgress)
                        {
                            // Toggle Ignition State: 
                            ToggleIgnition();
                            ignitionHeld = false;
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
                    if (Game.IsControlPressed(Control.VehicleAccelerate) && isEngineOff)
                    {
                        N.SetVehicleEngineOn(Game.Player.Character.CurrentVehicle, true, false, true);
                        isEngineOff = false;
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.HandleIgnition", ex);
                ignitionHeld = false;
                isEngineOff = false;
                toggleInProgress = false;
            }
        }
        
        public static void TyrePressureMonitoringSystem(Vehicle vehicle)
        {
            try
            {
                // TYRES:
                bool lf_burst = false, rf_burst = false, lr_burst = false, rr_burst = false;
                bool lf_leak = false, rf_leak = false, lr_leak = false, rr_leak = false;
                // Check Tyre status:
                bool tyreIssue = IsTyreBurstOrLeaking(vehicle, 0, out lf_burst, out lf_leak)
                    || IsTyreBurstOrLeaking(vehicle, 1, out rf_burst, out rf_leak)
                    || IsTyreBurstOrLeaking(vehicle, 4, out lr_burst, out lr_leak)
                    || IsTyreBurstOrLeaking(vehicle, 5, out rr_burst, out rr_leak);

                string tyrePressure = $"Tyre Pressure Warning: ";

                if (lf_leak)
                {
                    tyrePressure += "~n~Left Front: ";
                    tyrePressure += lf_burst ? "~r~(Burst)~s~ " : "~y~Low!~s~";
                }
                if (rf_leak)
                {
                    tyrePressure += "~n~Right Front: ";
                    tyrePressure += rf_burst ? "~r~(Burst)~s~ " : "~y~Low!~s~";
                }
                if (lr_leak)
                {
                    tyrePressure += "~n~Left Rear: ";
                    tyrePressure += lr_burst ? "~r~(Burst)~s~ " : "~y~Low!~s~";
                }
                if (rr_leak)
                {
                    tyrePressure += "~n~Right Rear: ";
                    tyrePressure += rr_burst ? "~r~(Burst)~s~ " : "~y~Low!~s~";
                }

                if (tyreIssue)
                {
                    Notification.Show(tyrePressure, false);
                }
                else
                {
                    Notification.Show("All Tyres are in perfect condition.", false);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionHandler.TyrePressureCheck", ex);
            }
        }

        public static bool IsTyreBurstOrLeaking(Vehicle vehicle, int tyreIndex, out bool isBurst, out bool isLeaking)
        {
            try
            {
                isBurst = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, tyreIndex, true);
                isLeaking = Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, tyreIndex, false);
                return isBurst || isLeaking;
            }
            catch (Exception ex)
            {
                isBurst = false;
                isLeaking = false;
                AIS.LogException("InteractionHandler.IsTyreBurstOrLeaking", ex);
                return isBurst || isLeaking;
            }
        }
    }
}
