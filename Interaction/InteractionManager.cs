using System;
using GTA;
using GTA.Native;
using Control = GTA.Control;
using System.Threading;

namespace AdvancedInteractionSystem
{
    public class InteractionManager : Script
    {
        public static float interactionDistance = SettingsManager.interactionDistance;
        public static float persistenceDistance = SettingsManager.persistenceDistance;
        
        // VEHICLES: 
        public static Vehicle currentVehicle = null; // Player's Current Vehicle. 
        public static Vehicle lastVehicle = null; // Player's Current Vehicle. 
        public static Vehicle closestVehicle = null; // Closest Vehicle when player is on foot 

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
            Aborted += OnAborted;
            Interval = 10;
            Function.Call(Hash.REQUEST_ANIM_DICT, "veh@std@ds@base");
        }
        private void OnAborted(object sender, EventArgs e)
        {
            // CancelActions();
        }

        // Logic Inside Vehicle:
        private void UpdateCurrentVehicle()
        {
            currentVehicle = Game.Player.Character.CurrentVehicle;
        }

        // Logic On Foot:
        private void UpdateClosestVehicle()
        {
            if (GPC.IsOnFoot)
            {
                closestVehicle = World.GetClosestVehicle(GPC.Position, persistenceDistance);
            }
            else
            {
                closestVehicle = null;
            }
        }

        private void OnTick(object o, EventArgs e)
        {
            try
            {
                // The Interaction Manager class manages the main Interaction methods.
                // We update the Player's current and closest vehicle 
                // Then pass to the Interaction Handler class

                if (Game.IsLoading || Game.IsPaused || !SettingsManager.modEnabled)
                    return;
                
                GPC = Game.Player.Character;
                if (GPC == null) return;

                UpdateCurrentVehicle();
                UpdateClosestVehicle();

                IgnitionHandler.DisableAutoStart();
                IgnitionHandler.IVExit(currentVehicle);

                if (currentVehicle != null && currentVehicle.Exists())
                {
                    
                    InteractionHandler.HandleInVehicle(currentVehicle);
                }

                if (closestVehicle != null && closestVehicle.Exists())
                {
                    InteractionHandler.HandleOnFoot(closestVehicle);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.OnTick()", ex);
            }
        }

        // END ACTIONS: 
        public static void CompleteActions()
        {
            try
            {
                if (soundStopEvent != null)
                {
                    soundStopEvent.Set();
                }

                // Clear animations
                if (Game.Player.Character != null)
                {
                    Game.Player.Character.Task.ClearAll();
                }

                if (Repairs.repairProp != null && Repairs.repairProp.Exists())
                {
                    Repairs.repairProp.Detach();
                    Repairs.repairProp.Delete();
                }

                if (Cleaning.cleaningProp != null && Cleaning.cleaningProp.Exists())
                {
                    Cleaning.cleaningProp.Detach();
                    Cleaning.cleaningProp.Delete();
                }

                Repairs.isRepairing = false;
                Cleaning.cleaning = false;
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.CancelActions", ex);
            }
        }

        // DISABLE CONTROLS: 
        public static void DisableControls()
        {
            try
            {
                // This doesn't work, causes player to loop in-out of aiming lol 
                // Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Aim, false);
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
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.MeleeBlock, true);
            }
            catch (Exception ex)
            {
                AIS.LogException("InteractionManager.DisableControls", ex);
            }
        }
    }
}
