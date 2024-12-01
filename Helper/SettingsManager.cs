using System;
using System.IO;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using Control = GTA.Control;

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
        public static bool disableAutoStart = true;
        public static bool IVExit = true;
        // Ignition:
        public static bool ignition_debugEnabled = true;
        public static bool ignitionControlEnabled = true; // Enable the Vehicle Engine Button/Key. 
        public static bool ignitionByThrottleEnabled = true; // Enable starting the vehicle by accelerator. 
        // Fuel: 
        public static bool fuel_debugEnabled = true;
        public static bool fuelEnabled = true; // Enable the Vehicle Fuel System Module. 
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
        public static bool persistence_debugEnabled = false;
        // Toggle Modules: 
        public static bool flipEnabled = true; // Enable the Vehicle Flipping Module. 

        // Other Settings: 
        public static Keys menuToggleKey = Keys.F8; // Use this Key to open the Mod's Menu. 
        public static Keys refuelKey = Keys.Space; // User changeable refuel key - Not currently implemented
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
        public static float interactionDistance = 3.5f;
        public static float persistenceDistance = 10f;
        public static int repairDuration = 8;
        public static int washDuration = 6;
        public static int refuelSpeedMultiplier = 1;
        public static float ignitionHoldDuration = 1.5f;
        public static bool showFlipNotification = true;
        public static bool cleaningSoundEnabled = true;
        public static bool shortRangeBlips = true;
        
        public static void CreateIni(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("[Options]");
                // 
                writer.WriteLine($"Mod Enabled = {modEnabled}");
                writer.WriteLine($"Debug Enabled = {debugEnabled}");
                writer.WriteLine($"Menu Key = {menuToggleKey}");
                // writer.WriteLine($"");
                // INTERACTION HANDLER:
                writer.WriteLine($"Interaction Distance = {interactionDistance}");
                writer.WriteLine($"Handler Debug Enabled = {handler_debugEnabled}");
                // IGNITION: 
                writer.WriteLine("Ignition Options", $"Disable Auto Start Enabled = {disableAutoStart}");
                writer.WriteLine("Ignition Options", $"IV Exit Enabled = {IVExit}");
                writer.WriteLine("Ignition Options", $"Ignition Control Enabled = {ignitionControlEnabled}");
                writer.WriteLine("Ignition Options", $"Ignition Debug Enabled = {ignition_debugEnabled}");
                writer.WriteLine("Ignition Options", $"Ignition By Throttle Enabled = {ignitionByThrottleEnabled}");
                // FUEL: 
                writer.WriteLine("Fuel Options", $"Fuel Enabled = {fuelEnabled}");
                writer.WriteLine("Fuel Options", $"Fuel Debug Enabled = {fuel_debugEnabled}");
                // REPAIRS:
                writer.WriteLine("Repairs Options", $"Repairs Enabled = {repairsEnabled}");
                writer.WriteLine("Repairs Options", $"Repairs Debug Enabled = {repairs_debugEnabled}");
                writer.WriteLine("Repairs Options", $"Repairs Body Damage Enabled = {repairBodyDamage}");
                writer.WriteLine("Repairs Options", $"Repairs Refuel on Repair Enabled = {refuelOnRepair}");
                writer.WriteLine("Repairs Options", $"Repairs Duration = {repairDuration}");
                // CLEANING:
                writer.WriteLine("Cleaning Options", $"Cleaning Enabled = {cleaningEnabled}");
                writer.WriteLine("Cleaning Options", $"Cleaning Debug Enabled = {cleaning_debugEnabled}");
                writer.WriteLine("Cleaning Options", $"Cleaning Wash Duration = {washDuration}");
                writer.WriteLine("Cleaning Options", $"Cleaning Sound Enabled = {cleaningSoundEnabled}");
                // PERSISTENCE:
                writer.WriteLine("Persistence Options", $"Persistence Enabled = {persistenceEnabled}");
                writer.WriteLine("Persistence Options", $"Persistence Debug Enabled = {persistence_debugEnabled}");
                writer.WriteLine("Persistence Options", $"Persistence Distance = {persistenceDistance}");
                writer.WriteLine("Persistence Options", $"Persistence Short Range Blips = {shortRangeBlips}");
                // FLIP:
                writer.WriteLine("Options", $"Show Flip Notification = {showFlipNotification}");
                writer.WriteLine("Options", $"Vehicle Flip Enum Control = {flipControl}");
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
                    modEnabled = settings.GetValue<bool>("Options", "Mod Enabled", modEnabled);
                    debugEnabled = settings.GetValue<bool>("Options", "Debug Enabled", debugEnabled);
                    menuToggleKey = settings.GetValue<Keys>("Options", "Menu Key", menuToggleKey);

                    interactionDistance = settings.GetValue<float>("Options", "Interaction Distance", interactionDistance);
                    handler_debugEnabled = settings.GetValue<bool>("Options", "Handler Debug Enabled", handler_debugEnabled);
                    
                    disableAutoStart = settings.GetValue<bool>("Ignition Options", "Disable Auto Start Enabled", disableAutoStart);
                    ignitionControlEnabled = settings.GetValue<bool>("Ignition Options", "Ignition Control Enabled", ignitionControlEnabled);
                    ignition_debugEnabled = settings.GetValue<bool>("Ignition Options", "Ignition Debug Enabled", ignition_debugEnabled);
                    ignitionByThrottleEnabled = settings.GetValue<bool>("Ignition Options", "Ignition By Throttle Enabled", ignitionByThrottleEnabled);

                    fuelEnabled = settings.GetValue<bool>("Fuel Options", "Fuel Enabled", fuelEnabled);
                    fuel_debugEnabled = settings.GetValue<bool>("Fuel Options", "Fuel Debug Enabled", fuel_debugEnabled);

                    repairsEnabled = settings.GetValue<bool>("Repair Options", "Repairs Enabled", repairsEnabled);
                    repairs_debugEnabled = settings.GetValue<bool>("Repair Options", "Repairs Debug Enabled", repairs_debugEnabled);
                    repairBodyDamage = settings.GetValue<bool>("Repair Options", "Repairs Body Damage Enabled", repairBodyDamage);
                    refuelOnRepair = settings.GetValue<bool>("Repair Options", "Repairs Refuel on Repair Enabled", refuelOnRepair);
                    repairDuration = settings.GetValue<int>("Repair Options", "Repairs Duration", repairDuration);

                    cleaningEnabled = settings.GetValue<bool>("Cleaning Options", "Cleaning Enabled", cleaningEnabled);
                    cleaning_debugEnabled = settings.GetValue<bool>("Cleaning Options", "Cleaning Debug Enabled", cleaning_debugEnabled);
                    cleaningSoundEnabled = settings.GetValue<bool>("Cleaning Options", "Cleaning Sound Enabled", cleaningSoundEnabled);
                    washDuration = settings.GetValue<int>("Cleaning Options", "Cleaning Wash Duration", washDuration);

                    persistenceEnabled = settings.GetValue<bool>("Persistence Options", "Persistence Enabled", persistenceEnabled);
                    persistence_debugEnabled = settings.GetValue<bool>("Persistence Options", "Persistence Debug Enabled", persistence_debugEnabled);
                    shortRangeBlips = settings.GetValue<bool>("Persistence Options", "Persistence Short Range Blips", shortRangeBlips);
                    persistenceDistance = settings.GetValue<float>("Persistence Options", "Persistence Distance", persistenceDistance);

                    showFlipNotification = settings.GetValue<bool>("Options", "Show Flip Notification", showFlipNotification);
                    flipControl = settings.GetValue<Control>("Options", "Vehicle Flip Enum Control", flipControl);

                    SaveSettings();
                    
                    if (debugEnabled)
                        N.DisplayNotification($"Loaded Advanced Interaction System settings", true);
                }
                else
                {
                    // Loading Failed! 
                    if (debugEnabled)
                    {
                        N.ShowSubtitle($"~r~Warning!: Loading Advanced Interaction System Settings failed.~s~", 500);
                    }
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
                    settings.SetValue<bool>("Options", "Mod Enabled", modEnabled);
                    settings.SetValue<bool>("Options", "Debug Enabled", debugEnabled);
                    settings.SetValue<Keys>("Options", "Menu Key", menuToggleKey);

                    settings.SetValue<bool>("Options", "Disable Auto Start Enabled", disableAutoStart);
                    settings.SetValue<float>("Options", "Interaction Distance", interactionDistance);
                    settings.SetValue<bool>("Options", "Handler Debug Enabled", handler_debugEnabled);

                    settings.SetValue<bool>("Ignition Options", "Ignition Control Enabled", ignitionControlEnabled);
                    settings.SetValue<bool>("Ignition Options", "Ignition Debug Enabled", ignition_debugEnabled);
                    settings.SetValue<bool>("Ignition Options", "Ignition By Throttle Enabled", ignitionByThrottleEnabled);

                    settings.SetValue<bool>("Fuel Options", "Fuel Enabled", fuelEnabled);
                    settings.SetValue<bool>("Fuel Options", "Fuel Debug Enabled", fuel_debugEnabled);

                    settings.SetValue<bool>("Repair Options", "Repairs Enabled", repairsEnabled);
                    settings.SetValue<bool>("Repair Options", "Repairs Debug Enabled", repairs_debugEnabled);
                    settings.SetValue<bool>("Repair Options", "Repairs Body Damage Enabled", repairBodyDamage);
                    settings.SetValue<bool>("Repair Options", "Repairs Refuel on Repair Enabled", refuelOnRepair);
                    settings.SetValue<int>("Repair Options", "Repairs Duration", repairDuration);

                    settings.SetValue<bool>("Cleaning Options", "Cleaning Enabled", cleaningEnabled);
                    settings.SetValue<bool>("Cleaning Options", "Cleaning Debug Enabled", cleaning_debugEnabled);
                    settings.SetValue<bool>("Cleaning Options", "Cleaning Sound Enabled", cleaningSoundEnabled);
                    settings.SetValue<int>("Cleaning Options", "Cleaning Wash Duration", washDuration);

                    settings.SetValue<bool>("Persistence Options", "Persistence Enabled", persistenceEnabled);
                    settings.SetValue<bool>("Persistence Options", "Persistence Debug Enabled", persistence_debugEnabled);
                    settings.SetValue<bool>("Persistence Options", "Persistence Short Range Blips", shortRangeBlips);
                    settings.SetValue<float>("Persistence Options", "Persistence Distance", persistenceDistance);

                    settings.SetValue<bool>("Options", "Show Flip Notification", showFlipNotification);
                    settings.SetValue<Control>("Options", "Vehicle Flip Enum Control", flipControl);

                    settings.Save();

                    if (debugEnabled)
                    {
                        Notification.Show($"Saved Advanced Interaction System settings", true);
                    }
                }
                else
                { 
                    // Saving Failed!
                    if (debugEnabled)
                    {
                        N.ShowSubtitle($"~r~Warning!: Saving Advanced Interaction System Settings failed.~s~", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("SettingsManager.LoadSettings", ex);
            }
            
        }

    }
}
