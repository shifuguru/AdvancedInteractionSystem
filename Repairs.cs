using System;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Screen = GTA.UI.Screen;

namespace AdvancedInteractionSystem
{
    public class Repairs : Script
    {
        public static bool repairs_debugEnabled = SettingsManager.repairs_debugEnabled;
        public static int repairDuration = SettingsManager.repairDuration;
        public static bool repairBodyDamage = SettingsManager.repairBodyDamage;
        public static bool repairRequiresEngineOff = SettingsManager.repairRequiresEngineOff;
        public static Entity repairProp = null;
        public static int prop_tool_spanner01 = -2050576199;
        public static Vector3 repairObjectPosition = new Vector3(0.0700006f, 0.0100001f, -0.0100001f);
        public static Vector3 repairObjectRotation = new Vector3(112.32f, 5.76f, -15.84f);
        public static DateTime actionStartTime;
        public static bool isRepairing = false;

        public Repairs()
        {
            Tick += OnTick;
            Interval = 10;
        }

        public static void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (isRepairing && !Cleaning.cleaning)
                {
                    Vehicle vehicle = InteractionManager.closestVehicle;
                    if (vehicle == null || vehicle.IsDead)
                    {
                        InteractionManager.CancelActions();
                        return;
                    }

                    float engineHealth = vehicle.EngineHealth;

                    if (engineHealth < vehicle.MaxHealth)
                    {
                        vehicle.EngineHealth = Math.Min(vehicle.MaxHealth, engineHealth + 1f);
                    }
                    else
                    {
                        if (repairBodyDamage)
                        {
                            vehicle.Repair();
                        }
                        CompleteRepair(vehicle);
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("Repairs.OnTick()", ex);
            }
            
        }


        // Start Repair Process: 
        public static void StartRepairProcess(Vehicle vehicle)
        {
            try
            {
                if (!isRepairing)
                {
                    // this method runs each time the repair control is pressed. 
                    if (vehicle == null)
                    {
                        if (repairs_debugEnabled)
                        {
                            N.ShowSubtitle("~r~Repair Failed: No vehicle found~s~", 300);
                        }
                        return;
                    }
                    if (vehicle.IsDead)
                    {
                        if (repairs_debugEnabled)
                        {
                            N.ShowSubtitle("~r~Vehicle is beyond repair~s~", 300);
                        }
                        return;
                    }
                    if (repairRequiresEngineOff && vehicle.IsEngineRunning)
                    {
                        N.DisplayNotification($"~o~Engine must be off prior to repair.~s~", false);
                        return;
                    }
                    if (repairs_debugEnabled)
                    {
                        if (repairs_debugEnabled)
                        {
                            N.ShowSubtitle("Starting Repairing Process...", 1000);
                        }                        
                    }
                    
                    InteractionHandler.LookAtVehicle(vehicle, 2000);
                    InteractionHandler.OpenVehicleDoors(vehicle);
                    repairProp = AttachRepairObject();
                    actionStartTime = DateTime.Now;
                    PlayRepairAnimation();                    
                    isRepairing = true;
                }                
            }
            catch (Exception ex)
            {
                AIS.LogException("Repairs.StartRepairProcess", ex);
                InteractionManager.CancelActions();
            }
        }
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
       
        public static void CompleteRepair(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null) return;

                InteractionHandler.CloseVehicleDoors(vehicle);
                InteractionManager.CompleteActions();
                Game.Player.Character.Task.ClearAnimation("mini@repair", "fixing_a_ped");
                
                if (repairProp != null && repairProp.Exists())
                {
                    repairProp.Detach();
                    repairProp.Delete();
                    repairProp = null;
                }

                isRepairing = false;

                if (repairs_debugEnabled)
                {
                    N.ShowSubtitle("Repair Complete", 300);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("Repairs.CompleteRepairProcess", ex);
            }
        }
    }
}
