using GTA;
using LemonUI;
using LemonUI.Menus;
using System;
using Screen = GTA.UI.Screen;

namespace AdvancedInteractionSystem
{
    public class LemonMenu : Script
    {
        public static bool modEnabled = SettingsManager.modEnabled;
        public static bool debugEnabled = SettingsManager.debugEnabled;
        public static bool ignitionControlEnabled = SettingsManager.ignitionControlEnabled;
        public static bool cleaningEnabled = SettingsManager.cleaningEnabled;
        public static bool flipEnabled = SettingsManager.flipEnabled;
        public static bool fuelEnabled = SettingsManager.fuelEnabled;
        public static bool repairsEnabled = SettingsManager.repairsEnabled;

        public static readonly ObjectPool pool = new ObjectPool();
        public static readonly NativeMenu menu = new NativeMenu("Advanced Interactions", "Main Menu", "");
        public static readonly NativeMenu fuelmenu = new NativeMenu("Fuel Module", "Fuel Module", "Fuel Settings:");
        public static readonly NativeMenu repairmenu = new NativeMenu("Repair Module", "Repair Module", "Repair Settings:");
        public static readonly NativeMenu persistencemenu = new NativeMenu("Persistence Module", "Persistence Module", "Vehicle Saving:");
        public static readonly NativeMenu cleaningmenu = new NativeMenu("Cleaning Module", "Cleaning Module", "Cleaning Settings:");
        // Toggles: 
        private static readonly NativeCheckboxItem modEnabledToggle = new NativeCheckboxItem("Mod Enabled: ", "Enables/Disables the Mod", SettingsManager.modEnabled);
        private static readonly NativeCheckboxItem debugEnabledToggle = new NativeCheckboxItem("Debug Enabled: ", "Enables Debug Notifications. Recommended: False", SettingsManager.debugEnabled);
        private static readonly NativeCheckboxItem handler_debugEnabledToggle = new NativeCheckboxItem("Vehicle Handler Debug Enabled: ", "Enables Debug Notifications. Recommended: False", SettingsManager.handler_debugEnabled);
        private static readonly NativeCheckboxItem fuelEnabledToggle = new NativeCheckboxItem("Fuel Module enabled:", "Realtime Fuel calculation based on your vehicle's handling and driving behaviour", SettingsManager.fuelEnabled);
        private static readonly NativeCheckboxItem fuel_debugEnabledToggle = new NativeCheckboxItem("Fuel Debug Enabled: ", "Enables Debug Notifications. Recommended: False", SettingsManager.fuel_debugEnabled);
        private static readonly NativeCheckboxItem persistenceEnabledToggle = new NativeCheckboxItem("Persistence Module Enabled: ", "Enables Vehicle Saving and Persistence", SettingsManager.persistenceEnabled);
        private static readonly NativeCheckboxItem persistence_debugEnabledToggle = new NativeCheckboxItem("Persistence Debug Enabled: ", "Enables Debug Notifications. Recommended: False", SettingsManager.persistence_debugEnabled);
        private static readonly NativeCheckboxItem repairsEnabledToggle = new NativeCheckboxItem("Repair Module enabled:", "Repair vehicles when standing close", SettingsManager.repairsEnabled);
        private static readonly NativeCheckboxItem repairs_debugEnabledToggle = new NativeCheckboxItem("Repairs Debug Enabled: ", "Enables Debug Notifications. Recommended: False", SettingsManager.repairs_debugEnabled);
        private static readonly NativeCheckboxItem cleaningEnabledToggle = new NativeCheckboxItem("Cleaning Module enabled:", "Press ~INPUT_~ Wash your vehicles while standing close", SettingsManager.cleaningEnabled);
        private static readonly NativeCheckboxItem cleaning_debugEnabledToggle = new NativeCheckboxItem("Cleaning Debug Enabled: ", "Enables Debug Notifications. Recommended: False", SettingsManager.cleaning_debugEnabled);
        private static readonly NativeCheckboxItem flipEnabledToggle = new NativeCheckboxItem("Flip Vehicle Enabled: ", "Enables ability to flip vehicle when upside down. Recommended: True", SettingsManager.flipEnabled);
        private static readonly NativeCheckboxItem ignitionControlEnabledToggle = new NativeCheckboxItem("Engine Toggle enabled:", "Ability to turn engine on/off using E, or D-Pad Right", SettingsManager.ignitionControlEnabled);
        private static readonly NativeDynamicItem<float> refuelSpeedMultiplierItem = new NativeDynamicItem<float>("Refuel Speed Multiplier", "Refuelling speed multiplier", SettingsManager.refuelSpeedMultiplier);
        private static readonly NativeDynamicItem<int> fuel_levelItem = new NativeDynamicItem<int>("Set Fuel Level: ", "Set the Current Fuel Level. Useful for Debugging ", (int)Fuel.CurrentFuel);

        public LemonMenu()
        {
            LoadMenu();
            Tick += OnTick;
        }

        public void OnTick(object sender, EventArgs e)
        {
            pool.Process();
        }

        public static void LoadMenu()
        {
            pool.Add(menu);
            pool.Add(persistencemenu);
            pool.Add(fuelmenu);
            pool.Add(repairmenu);
            pool.Add(cleaningmenu);
            //
            menu.Add(modEnabledToggle);
            menu.Add(debugEnabledToggle);
            menu.Add(fuelmenu);
            menu.Add(repairmenu);
            menu.Add(cleaningmenu);
            menu.Add(persistencemenu);
            menu.Add(handler_debugEnabledToggle);
            menu.Add(ignitionControlEnabledToggle);
            menu.Add(flipEnabledToggle);

            persistencemenu.Add(persistenceEnabledToggle);
            persistencemenu.Add(persistence_debugEnabledToggle);

            fuelmenu.Add(fuelEnabledToggle);
            fuelmenu.Add(fuel_debugEnabledToggle);
            fuelmenu.Add(fuel_levelItem);
            fuelmenu.Add(refuelSpeedMultiplierItem);

            repairmenu.Add(repairsEnabledToggle);
            repairmenu.Add(repairs_debugEnabledToggle);

            cleaningmenu.Add(cleaningEnabledToggle);
            cleaningmenu.Add(cleaning_debugEnabledToggle);

            // 
            modEnabledToggle.Activated += ToggleMod;
            debugEnabledToggle.Activated += ToggleDebug;
            handler_debugEnabledToggle.Activated += ToggleHandlerDebug;
            ignitionControlEnabledToggle.Activated += ToggleEngineControl;
            fuelEnabledToggle.Activated += ToggleFuel;
            fuel_debugEnabledToggle.Activated += ToggleFuelDebug;
            fuel_levelItem.ItemChanged += SetFuelLevel;
            refuelSpeedMultiplierItem.ItemChanged += SetRefuelMultiplier;
            persistenceEnabledToggle.Activated += TogglePersistence;
            persistence_debugEnabledToggle.Activated += TogglePersistenceDebug;
            repairsEnabledToggle.Activated += ToggleRepairs;
            repairs_debugEnabledToggle.Activated += ToggleRepairsDebug;
            cleaningEnabledToggle.Activated += ToggleCleaning;
            cleaning_debugEnabledToggle.Activated += ToggleCleaningDebug;
            flipEnabledToggle.Activated += ToggleFlip;
            //
            modEnabledToggle.Checked = modEnabledToggle.Checked;
            debugEnabledToggle.Checked = debugEnabledToggle.Checked;
            handler_debugEnabledToggle.Checked = handler_debugEnabledToggle.Checked;
            fuelEnabledToggle.Checked = fuelEnabledToggle.Checked;
            fuel_debugEnabledToggle.Checked = fuel_debugEnabledToggle.Checked;
            refuelSpeedMultiplierItem.SelectedItem = SettingsManager.refuelSpeedMultiplier;
            fuel_levelItem.SelectedItem = (int)Fuel.CurrentFuel;
            persistenceEnabledToggle.Checked = persistenceEnabledToggle.Checked;
            persistence_debugEnabledToggle.Checked = persistence_debugEnabledToggle.Checked;
            repairs_debugEnabledToggle.Checked = repairs_debugEnabledToggle.Checked;
            cleaning_debugEnabledToggle.Checked = cleaning_debugEnabledToggle.Checked;
            flipEnabledToggle.Checked = flipEnabledToggle.Checked;
            repairsEnabledToggle.Checked = repairsEnabledToggle.Checked;
            cleaningEnabledToggle.Checked = cleaningEnabledToggle.Checked;
            ignitionControlEnabledToggle.Checked = ignitionControlEnabledToggle.Checked; 
        }
        public static void OpenMenu()
        {
            menu.Visible = true;
        }
        public static void CloseMenu()
        {
            menu.Visible = false;
            SettingsManager.SaveSettings();
        }
        public static void ToggleMod(object sender, EventArgs e)
        {
            SettingsManager.modEnabled = !SettingsManager.modEnabled;
            modEnabledToggle.Checked = SettingsManager.modEnabled;
            Screen.ShowSubtitle($"Advanced Interaction System Enabled: {SettingsManager.modEnabled}", 1500);
            SettingsManager.SaveSettings();
        }
        public static void ToggleDebug(object sender, EventArgs e)
        {
            SettingsManager.debugEnabled = !SettingsManager.debugEnabled;
            debugEnabledToggle.Checked = SettingsManager.debugEnabled;
            Screen.ShowSubtitle($"Debug Enabled: {SettingsManager.debugEnabled}", 1500);
            SettingsManager.SaveSettings();
        }
        public static void ToggleHandlerDebug(object sender, EventArgs e)
        {
            SettingsManager.handler_debugEnabled = !SettingsManager.handler_debugEnabled;
            handler_debugEnabledToggle.Checked = SettingsManager.handler_debugEnabled;
            Screen.ShowSubtitle($"Handler Debug Enabled: {SettingsManager.handler_debugEnabled}", 1500);
            SettingsManager.SaveSettings();
        }
        public static void ToggleFuel(object sender, EventArgs e)
        {
            SettingsManager.fuelEnabled = !SettingsManager.fuelEnabled;
            fuelEnabledToggle.Checked = SettingsManager.fuelEnabled;
            if (SettingsManager.debugEnabled)
            {
                Screen.ShowSubtitle($"Fuel Module Enabled: {SettingsManager.fuelEnabled}", 1500);
            }
            SettingsManager.SaveSettings();
        }
        public static void ToggleFuelDebug(object sender, EventArgs e)
        {
            SettingsManager.fuel_debugEnabled = !SettingsManager.fuel_debugEnabled;
            fuel_debugEnabledToggle.Checked = SettingsManager.fuel_debugEnabled;
            Screen.ShowSubtitle($"Fuel Debug Enabled: {SettingsManager.fuel_debugEnabled}", 1500);
            SettingsManager.SaveSettings();
        }

        public static void TogglePersistence(object sender, EventArgs e)
        {
            SettingsManager.persistenceEnabled = !SettingsManager.persistenceEnabled;
            persistenceEnabledToggle.Checked = SettingsManager.persistenceEnabled;
            Screen.ShowSubtitle($"Persistence Module Enabled: {SettingsManager.persistenceEnabled}", 1500);
            SettingsManager.SaveSettings();
        }
        public static void TogglePersistenceDebug(object sender, EventArgs e)
        {
            SettingsManager.persistence_debugEnabled = !SettingsManager.persistence_debugEnabled;
            persistence_debugEnabledToggle.Checked = SettingsManager.persistence_debugEnabled;
            Screen.ShowSubtitle($"Persistence Debug Enabled: {SettingsManager.persistence_debugEnabled}", 1500);
            SettingsManager.SaveSettings();
        }

        public static void SetRefuelMultiplier(object sender, ItemChangedEventArgs<float> e)
        {
            int minLimit = 1;
            int maxLimit = 10;

            int increment = 1;

            if (e.Direction == Direction.Left)
            {
                increment = -increment;
            }

            e.Object = (e.Object + increment - minLimit + (maxLimit - minLimit + 1)) % (maxLimit - minLimit + 1) + minLimit;
            SettingsManager.refuelSpeedMultiplier = (int)e.Object;
            SettingsManager.SaveSettings();
        }

        public static void SetFuelLevel(object sender, ItemChangedEventArgs<int> e)
        {
            if (Game.Player.Character.CurrentVehicle == null || InteractionManager.currentVehicle == null) 
            {
                e.Object = 0;
                return;
            }

            int maxLimit = (int)Fuel.MaxFuel;
            int minLimit = 0;

            int increment = 500; // Default increment 

            // Determine the increment based on the menu direction
            if (e.Direction == Direction.Left)
            {
                increment = -increment;
            }

            // Adjust the show duration within the specified range 
            e.Object = (e.Object + increment - minLimit + (maxLimit - minLimit + 1)) % (maxLimit - minLimit + 1) + minLimit;
            Fuel.UpdateVehicleFuel(InteractionManager.currentVehicle.Mods.LicensePlate, -e.Object);
            
            // SettingsManager.SaveSettings();
        }

        public static void ToggleRepairsDebug(object sender, EventArgs e)
        {
            SettingsManager.repairs_debugEnabled = !SettingsManager.repairs_debugEnabled;
            repairs_debugEnabledToggle.Checked = SettingsManager.repairs_debugEnabled;
            Screen.ShowSubtitle($"Repairs Debug Enabled: {SettingsManager.repairs_debugEnabled}", 1500);
            SettingsManager.SaveSettings();
        }
        public static void ToggleCleaningDebug(object sender, EventArgs e)
        {
            SettingsManager.cleaning_debugEnabled = !SettingsManager.cleaning_debugEnabled;
            cleaning_debugEnabledToggle.Checked = SettingsManager.cleaning_debugEnabled;
            Screen.ShowSubtitle($"Cleaning Debug Enabled: {SettingsManager.cleaning_debugEnabled}", 1500);
            SettingsManager.SaveSettings();
        }
        public static void ToggleFlip(object sender, EventArgs e)
        {
            SettingsManager.flipEnabled = !SettingsManager.flipEnabled;
            flipEnabledToggle.Checked = SettingsManager.flipEnabled;
            if (SettingsManager.debugEnabled)
            {
                Screen.ShowSubtitle($"Vehicle Flip Enabled: {SettingsManager.flipEnabled}", 1500);
            }
            SettingsManager.SaveSettings();
        }
        public static void ToggleRepairs(object sender, EventArgs e)
        {
            SettingsManager.repairsEnabled = !SettingsManager.repairsEnabled;
            repairsEnabledToggle.Checked = SettingsManager.repairsEnabled;
            if (SettingsManager.debugEnabled)
            {
                Screen.ShowSubtitle($"Repairs Module Enabled: {repairsEnabled}", 1500);
            }
            SettingsManager.SaveSettings();
        }
        public static void ToggleCleaning(object sender, EventArgs e)
        {
            SettingsManager.cleaningEnabled = !SettingsManager.cleaningEnabled;
            cleaningEnabledToggle.Checked = SettingsManager.cleaningEnabled;
            if (SettingsManager.debugEnabled)
            {
                Screen.ShowSubtitle($"Cleaning Module Enabled: {cleaningEnabled}", 1500);
            }
            SettingsManager.SaveSettings();
        }
        public static void ToggleEngineControl(object sender, EventArgs e)
        {
            SettingsManager.ignitionControlEnabled = !SettingsManager.ignitionControlEnabled;
            ignitionControlEnabledToggle.Checked = SettingsManager.ignitionControlEnabled;
            if (SettingsManager.debugEnabled)
            {
                Screen.ShowSubtitle($"Ignition Key Enabled: {SettingsManager.ignitionControlEnabled}", 1500);
            }
            if (SettingsManager.ignitionControlEnabled)
            {
                Screen.ShowHelpText("You can now hold ~INPUT_CONTEXT~ for 3 seconds whenever you are in a vehicle to toggle the ignition", -1, false, false);
            }
            SettingsManager.SaveSettings();
        }
    }
}
