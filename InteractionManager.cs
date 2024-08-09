using System;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;
using System.Threading;
using LemonUI.Elements;

namespace AdvancedInteractionSystem
{
    public class InteractionManager : Script
    {
        public static bool modEnabled = SettingsManager.modEnabled;
        public static bool debugEnabled = SettingsManager.debugEnabled;
        public static float interactionDistance = SettingsManager.interactionDistance;
        
        // VEHICLES: 
        public static Vehicle currentVehicle; // Player's Current Vehicle. 
        public static Vehicle closestVehicle; // Closest Vehicle when player is on foot 

        // PED: 
        public static Ped GPC = Game.Player.Character;
        // BOOLS: 
        public static ManualResetEvent soundStopEvent = new ManualResetEvent(false);
        public static int holdDuration = 3;
        // public static DateTime actionStartTime;
        // public static DateTime flipStartTime;
        // public static bool flipTimerRunning;

        public InteractionManager()
        {
            Tick += OnTick;
            Interval = 10;
            Function.Call(Hash.REQUEST_ANIM_DICT, "veh@std@ds@base");
        }

        private void OnTick(object o, EventArgs e)
        {
            try
            {
                modEnabled = SettingsManager.modEnabled;
                debugEnabled = SettingsManager.debugEnabled;
                if (!modEnabled) return;
                
                GPC = Game.Player.Character;
                if (GPC == null) return;
 
                if (GPC.IsInVehicle())
                {
                    currentVehicle = GPC.CurrentVehicle;
                    IgnitionHandler.IVExit(GPC, currentVehicle, debugEnabled);
                    InteractionHandler.HandleInVehicle(currentVehicle);
                }
                if (GPC.IsOnFoot)
                {
                    closestVehicle = World.GetClosestVehicle(GPC.Position, interactionDistance);
                    if (closestVehicle != null)
                    {
                        InteractionHandler.HandleOnFoot(closestVehicle);
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.OnTick()", ex);
            }
        }

        // END ACTIONS: 
        public static void CancelActions()
        {
            try
            {
                soundStopEvent.Set();
                
                Repairs.isRepairing = false;
                Cleaning.cleaning = false;
                // Clear animations
                Game.Player.Character.Task.ClearAnimation("mini@repair", "fixing_a_ped");
                Game.Player.Character.Task.ClearAnimation("timetable@maid@cleaning_surface@base", "base");
                Game.Player.Character.Task.ClearAll();

                if (Repairs.repairProp.Exists())
                {
                    Repairs.repairProp.Delete();
                }

                if (Cleaning.cleaningProp.Exists())
                {
                    Cleaning.cleaningProp.Delete();
                }
            }
            catch (Exception ex)
            {
                soundStopEvent.Set();
                Repairs.isRepairing = false;
                Cleaning.cleaning = false;
                AIS.LogException("InteractionManager.CancelActions", ex);
            }
        }
        public static void CompleteActions()
        {
            try
            {
                soundStopEvent.Set();
                Game.Player.Character.Task.ClearAll();
                Repairs.isRepairing = false;
                Cleaning.cleaning = false;

                if (Repairs.repairProp.Exists())
                {
                    Repairs.repairProp.Delete();
                }

                if (Cleaning.cleaningProp.Exists())
                {
                    Cleaning.cleaningProp.Delete();
                }
            }
            catch (Exception ex)
            {
                soundStopEvent.Set();
                Repairs.isRepairing = false;
                Cleaning.cleaning = false;
                AIS.LogException("InteractionManager.CompleteActions", ex);
            }
        }

        // DISABLE CONTROLS: 
        public static void DisableControls()
        {
            try
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Attack, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Cover, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Sprint, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Jump, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Enter, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.MeleeAttack1, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.MeleeAttack2, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Attack, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Attack2, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.MeleeAttackAlternate, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.MeleeAttackHeavy, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.MeleeAttackLight, true);
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.DisableControls", ex);
            }
        }
    }
}
