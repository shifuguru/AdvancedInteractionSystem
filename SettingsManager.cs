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

namespace AdvancedInteractionSystem
{
    internal class SettingsManager : Script
    {
        public static string settingsDirectory = "scripts\\AIS";
        public static string settingsFilePath = Path.Combine(settingsDirectory, "AdvancedInteractionSystem.ini");
        public static ScriptSettings settings = ScriptSettings.Load(settingsFilePath);
        // Settings: 
        public static bool modEnabled = true;
        public static bool debugEnabled = false; // Set back to false on Release. 
        // Handler: 
        public static bool handler_debugEnabled = true;
        // Fuel: 
        public static bool fuel_debugEnabled = true;
        public static bool fuelEnabled = true; // Enable the Vehicle Fuel System Module. 
        // Ignition:
        public static bool ignitionControlEnabled = true; // Enable the Vehicle Engine Button/Key. 
        // Repairs:
        public static bool repairsEnabled = true; // Enable the Repairing Module. 
        public static bool repairs_debugEnabled = true;
        public static bool repairBodyDamage = true; // 
        public static bool refuelOnRepair = false; // Enable Refuelling on Repair. 
        // Cleaning:
        public static bool cleaningEnabled = true; // Enable the Cleaning Module. 
        public static bool cleaning_debugEnabled = true;
        // Persistence: 
        public static bool persistenceEnabled = true;
        public static bool persistence_debugEnabled = true;
        // Toggle Modules: 
        public static bool flipEnabled = true; // Enable the Vehicle Flipping Module. 

        // Other Settings: 
        public static Keys menuToggleKey = Keys.F8; // Use this Key to open the Mod's Menu. 
        // CONTROLS: 
        public const Control studyControl = Control.Cover;
        public const Control cleanControl = Control.MeleeAttackLight;
        public const Control repairControl = Control.Sprint;
        public const Control doorControl = Control.Enter;
        public const Control ignitionControl = Control.VehicleHeadlight;

        // User Settings:
        public static Control flipControl = Control.Enter;
        public static bool repairRequiresEngineOff = true;
        public static bool cleaningRequiresEngineOff = false;
        public static float interactionDistance = 3f;
        public static int repairDuration = 8;
        public static int washDuration = 6;
        public static float ignitionHoldDuration = 1.5f;
        public static bool showFlipNotification = true;
        public static bool cleaningSoundEnabled = true;
        public const bool shortRangeBlips = true;
        
        public static void CreateIni(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("[Options]");
                writer.WriteLine($"MENU TOGGLE KEY = {menuToggleKey}");
                writer.WriteLine($"MOD ENABLED = {modEnabled}");
                writer.WriteLine($"DEBUG ENABLED = {debugEnabled}");
                writer.WriteLine($"DEBUG ENABLED = {handler_debugEnabled}");
                writer.WriteLine($"DEBUG ENABLED = {fuel_debugEnabled}");
                writer.WriteLine($"DEBUG ENABLED = {repairs_debugEnabled}");
                writer.WriteLine($"DEBUG ENABLED = {cleaning_debugEnabled}");
                writer.WriteLine($"IGNITION CONTROL ENABLED = {ignitionControlEnabled}");
                writer.WriteLine($"FUEL ENABLED = {fuelEnabled}");
                writer.WriteLine($"CLEANING ENABLED = {cleaningEnabled}");
                writer.WriteLine($"REPAIRS ENABLED = {repairsEnabled}");
                writer.WriteLine($"Interaction Distance = {interactionDistance}");
                writer.WriteLine($"Show Flip Notification = {showFlipNotification}");
                writer.WriteLine($"Vehicle Flip Enum Control = {flipControl}");
            }
        }
        
        
        public static void LoadSettings()
        {
            try
            {
                if (!Directory.Exists(settingsDirectory))
                {
                    Directory.CreateDirectory(settingsDirectory);
                }
                if (!File.Exists(settingsFilePath))
                {
                    CreateIni(settingsFilePath);
                }
                
                settings = ScriptSettings.Load(settingsFilePath);

                if (settings != null)
                {
                    menuToggleKey = settings.GetValue<Keys>("Options", "MENU TOGGLE KEY", menuToggleKey);
                    modEnabled = settings.GetValue<bool>("Options", "MOD ENABLED", modEnabled);
                    debugEnabled = settings.GetValue<bool>("Options", "MAIN DEBUG ENABLED", debugEnabled);
                    handler_debugEnabled = settings.GetValue<bool>("Options", "HANDLER DEBUG ENABLED", handler_debugEnabled);
                    fuel_debugEnabled = settings.GetValue<bool>("Options", "FUEL DEBUG ENABLED", fuel_debugEnabled);
                    repairs_debugEnabled = settings.GetValue<bool>("Options", "REPAIRS DEBUG ENABLED", repairs_debugEnabled);
                    cleaning_debugEnabled = settings.GetValue<bool>("Options", "CLEANING DEBUG ENABLED", cleaning_debugEnabled);
                    ignitionControlEnabled = settings.GetValue<bool>("Options", "Engine Control ENABLED", ignitionControlEnabled);
                    fuelEnabled = settings.GetValue<bool>("Options", "FUEL ENABLED", fuelEnabled);
                    repairsEnabled = settings.GetValue<bool>("Options", "REPAIRS ENABLED", repairsEnabled);
                    cleaningEnabled = settings.GetValue<bool>("Options", "CLEANING ENABLED", cleaningEnabled);
                    showFlipNotification = settings.GetValue<bool>("Options", "Show Flip Notification", showFlipNotification);
                    interactionDistance = settings.GetValue<float>("Options", "Interaction Distance", interactionDistance);
                    flipControl = settings.GetValue<Control>("Options", "Vehicle Flip Enum Control", flipControl);
                    SaveSettings();
                    // Finished Loading 
                    if (debugEnabled)
                        N.DisplayNotification($"Loaded Advanced Interaction System settings.", true);
                }
                else
                {
                    // Loading Failed! 
                    if (debugEnabled)
                        N.ShowSubtitle($"~r~Warning!: Loading Advanced Interaction System Settings failed.~s~", 500);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("SettingsManager.LoadSettings", ex);
            }
            

            
        }
        public static void SaveSettings()
        {
            try
            {
                if (settings != null)
                {
                    settings.SetValue<Keys>("Options", "Menu Key", menuToggleKey);
                    settings.SetValue<bool>("Options", "MOD ENABLED", modEnabled);
                    settings.SetValue<bool>("Options", "MAIN DEBUG ENABLED", debugEnabled);
                    settings.SetValue<bool>("Options", "HANDLER DEBUG ENABLED", handler_debugEnabled);
                    settings.SetValue<bool>("Options", "FUEL DEBUG ENABLED", fuel_debugEnabled);
                    settings.SetValue<bool>("Options", "REPAIRS DEBUG ENABLED", repairs_debugEnabled);
                    settings.SetValue<bool>("Options", "CLEANING DEBUG ENABLED", cleaning_debugEnabled);
                    settings.SetValue<bool>("Options", "IGNITION CONTROL ENABLED", ignitionControlEnabled);
                    settings.SetValue<bool>("Options", "FUEL ENABLED", fuelEnabled);
                    settings.SetValue<bool>("Options", "REPAIRS ENABLED", repairsEnabled);
                    settings.SetValue<bool>("Options", "CLEANING ENABLED", cleaningEnabled);
                    settings.SetValue<bool>("Options", "Show Flip Notification", showFlipNotification);
                    settings.SetValue<float>("Options", "Interaction Distance", interactionDistance);
                    settings.SetValue<Control>("Options", "Vehicle Flip Enum Control", flipControl);
                    //settings.SetValue<bool>("Options", "Explosion Control", ExplosionControl);

                    // Finish Saving 
                    settings.Save();
                    if (debugEnabled)
                        Notification.Show($"Saved Advanced Interaction System settings.", true);
                }
                else
                {
                    // Saving Failed!
                    if (debugEnabled)
                        N.ShowSubtitle($"~r~Warning!: Saving Advanced Interaction System Settings failed.~s~", 500);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("SettingsManager.LoadSettings", ex);
            }
            
        }

    }
}
