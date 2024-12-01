using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    public class Repairs : Script
    {
        public static Entity repairProp = null;
        public static int prop_tool_spanner01 = -2050576199;
        public static Vector3 repairObjectPosition = new Vector3(0.0700006f, 0.0100001f, -0.0100001f);
        public static Vector3 repairObjectRotation = new Vector3(112.32f, 5.76f, -15.84f);
        public static DateTime actionStartTime;

        public const float maxHealth = 1000f;
        public static float totalHealth = 1000f;
        private static float bodyHealth;
        public static float engineHealth = 1000f;
        public static bool isRepairing = false;
        public static float repairAmount = Math.Min(maxHealth, engineHealth + 0.1f);

        // Attach Repair Object:
        public static Entity AttachRepairObject()
        {
            try
            {
                Ped GPC = Game.Player.Character;
                if (GPC.Bones["IK_R_Hand"].Index != -1)
                {
                    int handle = Function.Call<int>(
                        Hash.CREATE_OBJECT,
                        prop_tool_spanner01,
                        GPC.Position.X, GPC.Position.Y, GPC.Position.Z,
                        true, true, true
                        );

                    Function.Call(
                        Hash.ATTACH_ENTITY_TO_ENTITY,
                        handle,
                        GPC, GPC.Bones["IK_R_Hand"].Index,
                        repairObjectPosition.X, repairObjectPosition.Y, repairObjectPosition.Z,
                        repairObjectRotation.X, repairObjectRotation.Y, repairObjectRotation.Z,
                        false, false, false, false, 2, true
                        );

                    return Entity.FromHandle(handle);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("AttachRepairObject", ex);
                return null;
            }
        }
        // Play Repair Animation:
        public static void PlayRepairAnimation()
        {
            try
            {
                Game.Player.Character.Task.PlayAnimation("mini@repair", "fixing_a_ped", 8f, -8f, -1, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 0.0f);
            }
            catch (Exception ex)
            {
                AIS.LogException("Repairs.PlayRepairAnimation", ex);
            }
            
        }


        public Repairs()
        {
            Tick += OnTick;
            Aborted += OnAborted;
            Interval = 10;
        }

        public static void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (!isRepairing) return;

                Vehicle vehicle = InteractionManager.closestVehicle;

                if (vehicle == null || !vehicle.Exists() || vehicle.IsDead || !InteractionHandler.IsPlayerFacingVehicle(vehicle) || !CanVehicleBeRepaired(vehicle))
                {
                    RemoveRepairProp();
                    InteractionManager.CompleteActions();
                    return;
                }

                int repairTime = Game.GameTime - actionStartTime.Millisecond;
                if (repairTime < 500)
                {
                    return;
                }

                engineHealth = vehicle.EngineHealth;
                totalHealth = vehicle.HealthFloat;
                bodyHealth = vehicle.BodyHealth;

                if (SettingsManager.repairs_debugEnabled)
                {
                    N.ShowSubtitle($"Body Health: {vehicle.BodyHealth} Engine Health: {vehicle.EngineHealth}", 200);
                }

                if (totalHealth < 1000f)
                {
                    vehicle.EngineHealth += repairAmount;
                    
                }
                if (bodyHealth < 1000f)
                {
                    vehicle.BodyHealth += repairAmount;
                }
                else
                {
                    CompleteRepair(vehicle);
                }

            }
            catch (Exception ex)
            {
                AIS.LogException("Repairs.OnTick()", ex);
            }
        }

        public static void OnAborted(object sender, EventArgs e)
        {
            RemoveRepairProp();
            isRepairing = false;
        }

        // Start Repair Process:

        private void RepairBody(Vehicle vehicle)
        {
            vehicle.BodyHealth = 1000f;
        }
        private void RepairEngine(Vehicle vehicle)
        {
            vehicle.EngineHealth = 1000f;
        }
        private void RepairWindow(Vehicle vehicle)
        {
            vehicle.Doors[VehicleDoorIndex.BackLeftDoor].Close();
        }
        private void RepairAllWindows(Vehicle vehicle)
        {
            vehicle.Windows[0].Repair();
        }
        private void RepairWheel(Vehicle vehicle)
        {

        }
        private void RepairAllWheels(Vehicle vehicle)
        {

        }

        public static void StartRepairProcess(Vehicle vehicle)
        {
            if (CanVehicleBeRepaired(vehicle))
            {
                InteractionHandler.LookAtVehicle(vehicle, 2000);
                repairProp = AttachRepairObject();
                PlayRepairAnimation();
                actionStartTime = DateTime.Now;
                isRepairing = true;
            }
        }

        public static bool CanVehicleBeRepaired(Vehicle vehicle)
        {
            try
            {
                // If vehicle's health is too low we cannot repair it. 
                if (vehicle.EngineHealth <= 100f)
                {
                    N.DisplayNotification("~r~Vehicle Engine is beyond repair~s~", false);
                    return false;
                }

                // If the engine is running and repairs require engine off (realistic)
                if (SettingsManager.repairRequiresEngineOff && vehicle.IsEngineRunning)
                {
                    N.DisplayNotification($"~o~Engine must be off prior to repair~s~", false);
                    return false;
                }

                // If the engine cover is closed
                if (!vehicle.Doors[VehicleDoorIndex.Hood].IsOpen)
                {
                    N.ShowSubtitle("Open the hood to repair the engine", 1500);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AIS.LogException("Repairs.StartRepairProcess", ex);
                return false;
            }
        }

        private static void RemoveRepairProp()
        {
            if (repairProp == null || !repairProp.Exists()) return;

            repairProp.Detach();
            repairProp.Delete();
            repairProp = null;
        }

        public static void CompleteRepair(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null) return;

                InteractionManager.CompleteActions();
                Game.Player.Character.Task.ClearAnimation("mini@repair", "fixing_a_ped");
                
                if (SettingsManager.repairBodyDamage)
                {
                    vehicle.Repair();
                }

                RemoveRepairProp();

                isRepairing = false;

                if (SettingsManager.repairs_debugEnabled)
                {
                    N.ShowSubtitle("~g~Repair Complete~s~", 300);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("Repairs.CompleteRepairProcess", ex);
            }
        }
    }
}
