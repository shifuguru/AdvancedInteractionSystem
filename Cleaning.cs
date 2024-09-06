using System;
using System.IO;
using System.Media;
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
using System.Diagnostics.Eventing.Reader;
using System.Threading;

namespace AdvancedInteractionSystem
{
    public class Cleaning : Script
    {
        public static Ped GPC = InteractionManager.GPC;
        public static bool cleaning_debugEnabled = SettingsManager.cleaning_debugEnabled;
        public static Vehicle closestVehicle = InteractionManager.closestVehicle;
        public static Vehicle currentVehicle = InteractionManager.currentVehicle;
        public static bool cleaningRequiresEngineOff = SettingsManager.cleaningRequiresEngineOff;
        public static int washDuration = SettingsManager.washDuration;
        public static bool cleaningSoundEnabled = SettingsManager.cleaningSoundEnabled;
        //
        public static float washIntensity = 1.0f;
        public static Entity cleaningProp;
        public static int prop_rag_01 = 679927467;
        public static Vector3 cleaningObjectPosition = new Vector3(0.0700006f, 0.0100001f, -0.0100001f);
        public static Vector3 cleaningObjectRotation = new Vector3(112.32f, 5.76f, -15.84f);
        public static DateTime actionStartTime;
        public static bool cleaning = false;
        public static Thread cleaningSoundThread;
        public static bool cleaningSoundPlaying;
        // CLEANING: 

        public static void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (!cleaning || closestVehicle == null || !closestVehicle.Exists())
                    return;

                Vehicle vehicle = InteractionManager.closestVehicle;
                if (vehicle == null || !vehicle.Exists() || vehicle.IsDead)
                {
                    InteractionManager.CancelActions();
                    return;
                }

                if ((DateTime.Now - actionStartTime).TotalSeconds >= washDuration)
                {
                    CleanVehicle(vehicle);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("Cleaning.OnTick", ex);
            }
        }
        public static void StartCleaning()
        {
            try
            {
                if (closestVehicle == null || closestVehicle.IsDead || (cleaningRequiresEngineOff && closestVehicle.IsEngineRunning))
                    return;
                if (!cleaningRequiresEngineOff || !closestVehicle.IsEngineRunning)
                {
                    cleaning = true;
                    actionStartTime = DateTime.Now;
                    cleaningProp = AttachCleaningObject();
                    PlayCleaningAnimation(closestVehicle, washDuration);
                    StartCleaningSoundLoop();
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("StartCleaningProcess", ex);
            }
        }
        public static Entity AttachCleaningObject()
        {
            try
            {
                if (Game.Player.Character.Bones["IK_R_Hand"].Index != -1)
                {
                    int handle = Function.Call<int>(Hash.CREATE_OBJECT, prop_rag_01, GPC.Position.X, GPC.Position.Y, GPC.Position.Z, true, true, true);
                    Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, handle, GPC, GPC.Bones["IK_R_Hand"].Index, cleaningObjectPosition.X, cleaningObjectPosition.Y, cleaningObjectPosition.Z, cleaningObjectRotation.X, cleaningObjectRotation.Y, cleaningObjectRotation.Z, false, false, false, false, 2, true);
                    return Entity.FromHandle(handle);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("AttachCleaningObject", ex);
            }
            return null;
        }
        public static void PlayCleaningAnimation(Vehicle vehicle, int duration)
        {
            try
            {
                if (vehicle == null)
                    return;
                Game.Player.Character.Task.PlayAnimation("timetable@maid@cleaning_surface@base", "base", 8f, -8f, -1, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, duration);
                Game.Player.Character.Task.LookAt(vehicle.Position, duration);
            }
            catch (Exception ex)
            {
                AIS.LogException("PlayCleaningAnimation", ex);
            }
        }
        public static void StartCleaningSoundLoop()
        {
            try
            {
                if (!cleaningSoundEnabled)
                    return;

                cleaningSoundPlaying = true;
                cleaningSoundThread = new Thread((() =>
                {
                    try
                    {
                        string str = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VehicleMaintenance", "Audio", "Cleaning.wav");
                        if (File.Exists(str))
                        {
                            using (SoundPlayer soundPlayer = new SoundPlayer(str))
                            {
                                while (cleaning && cleaningSoundPlaying)
                                {
                                    soundPlayer.Play();
                                    soundPlayer.PlaySync();
                                    Thread.Sleep(300);
                                }
                            }
                        }
                        else 
                        {
                            if (cleaning_debugEnabled)
                            {
                                Screen.ShowSubtitle("~r~Audio File not found in directory. Verify installation.~s~", 2500);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AIS.LogException("CleaningSoundThread", ex);
                    }
                }));
                cleaningSoundThread.Start();
            }
            catch (Exception ex)
            {
                AIS.LogException("StartCleaningSoundLoop", ex);
                cleaningSoundThread.Abort();
            }
        }

        public static void CleanVehicle(Vehicle vehicle)
        {
            try
            {
                vehicle.DirtLevel = 0;
                Function.Call(Hash.WASH_DECALS_FROM_VEHICLE, washIntensity, vehicle);
                InteractionManager.CompleteActions();
            }
            catch
            {

            }
        }
    }
}
