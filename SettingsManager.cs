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
        public static string settingsFilePath = Path.Combine(settingsDirectory, "settings.ini");
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
        public static bool ignitionByThrottleEnabled = true; // Enable starting the vehicle by accelerator. 
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
        public static float persistenceDistance = 15f;
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
                writer.WriteLine($"Menu Key = {menuToggleKey}");
                writer.WriteLine($"Mod Enabled = {modEnabled}");
                writer.WriteLine($"Debug Enabled = {debugEnabled}");
                writer.WriteLine($"Handler Debug Enabled = {handler_debugEnabled}");
                writer.WriteLine($"Fuel Debug Enabled = {fuel_debugEnabled}");
                writer.WriteLine($"Repairs Debug Enabled = {repairs_debugEnabled}");
                writer.WriteLine($"Cleaning Debug Enabled = {cleaning_debugEnabled}");
                writer.WriteLine($"Ignition Control Enabled = {ignitionControlEnabled}");
                writer.WriteLine($"Fuel Enabled = {fuelEnabled}");
                writer.WriteLine($"Cleaning Enabled = {cleaningEnabled}");
                writer.WriteLine($"Repairs Enabled = {repairsEnabled}");
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
                    menuToggleKey = settings.GetValue<Keys>("Options", "Menu Key", menuToggleKey);
                    modEnabled = settings.GetValue<bool>("Options", "Mod Enabled", modEnabled);
                    debugEnabled = settings.GetValue<bool>("Options", "Debug Enabled", debugEnabled);
                    handler_debugEnabled = settings.GetValue<bool>("Options", "Handler Debug Enabled", handler_debugEnabled);
                    fuel_debugEnabled = settings.GetValue<bool>("Options", "Fuel Debug Enabled", fuel_debugEnabled);
                    repairs_debugEnabled = settings.GetValue<bool>("Options", "Repairs Debug Enabled", repairs_debugEnabled);
                    cleaning_debugEnabled = settings.GetValue<bool>("Options", "Cleaning Debug Enabled", cleaning_debugEnabled);
                    ignitionControlEnabled = settings.GetValue<bool>("Options", "Ignition Control Enabled", ignitionControlEnabled);
                    fuelEnabled = settings.GetValue<bool>("Options", "Fuel Enabled", fuelEnabled);
                    repairsEnabled = settings.GetValue<bool>("Options", "Repairs Enabled", repairsEnabled);
                    cleaningEnabled = settings.GetValue<bool>("Options", "Cleaning Enabled", cleaningEnabled);
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
                    settings.SetValue<bool>("Options", "Mod Enabled", modEnabled);
                    settings.SetValue<bool>("Options", "Debug Enabled", debugEnabled);
                    settings.SetValue<bool>("Options", "Handler Debug Enabled", handler_debugEnabled);
                    settings.SetValue<bool>("Options", "Fuel Debug Enabled", fuel_debugEnabled);
                    settings.SetValue<bool>("Options", "Repairs Debug Enabled", repairs_debugEnabled);
                    settings.SetValue<bool>("Options", "Cleaning Debug Enabled", cleaning_debugEnabled);
                    settings.SetValue<bool>("Options", "Ignition Control Enabled", ignitionControlEnabled);
                    settings.SetValue<bool>("Options", "Fuel Enabled", fuelEnabled);
                    settings.SetValue<bool>("Options", "Repairs Enabled", repairsEnabled);
                    settings.SetValue<bool>("Options", "Cleaning Enabled", cleaningEnabled);
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
