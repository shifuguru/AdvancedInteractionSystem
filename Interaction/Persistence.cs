using System;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Linq;
using System.Drawing;
using System.Xml;
using System.Diagnostics;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using Control = GTA.Control;
using Notification = GTA.UI.Notification;
using Screen = GTA.UI.Screen;

namespace AdvancedInteractionSystem
{
    public enum PlayerCharacter
    {
        Michael,
        Franklin,
        Trevor,
        Other,
        None
    }

    public class VehicleData
    {
        public int ModelHash { get; set; }
        public string LicensePlate { get; set; }
        public PlayerCharacter Owner { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 LastPosition { get; set; }
        public float Heading { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
        public float FuelLevel { get; set; }
        public float MaxFuel { get; set; }
        public float TripFuelLevel { get; set; }
        public float LastFuelLevel { get; set; }
        public float Odometer { get; set; }
        public float Tripometer { get; set; }
        public float FuelEfficiency { get; set; }
        // public VehicleModCollection Mods { get; set; }
        // public Dictionary<int, int> Modifications { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is VehicleData data)
            {
                return LicensePlate == data.LicensePlate &&
                    ModelHash == data.ModelHash &&
                    Owner == data.Owner;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return LicensePlate.GetHashCode() ^ ModelHash.GetHashCode() ^ Owner.GetHashCode();
        }
    }

    public class Persistence : Script
    {
        public static Model carKeyModel = new Model("lr_prop_carkey_fob");
        public static string path = "scripts\\AIS\\vehicles";
        public static bool IsVehicleLoaded = false;
        public static bool IsVehicleLoading = false;
        public static bool showBlips = true;
        public static Control saveKey = (Control)51;
        public static bool dispVehName = false;
        public static bool saveDamage = true;
        public static PlayerCharacter owner;

        // PROP: Carkey Fob ~
        public static Entity carkeyProp;
        public static int lr_prop_carkey_fob = -1341933582;
        public static Vector3 carkeyObjectPosition = new Vector3(0.0700006f, 0.0100001f, -0.0100001f);
        public static Vector3 carkeyObjectRotation = new Vector3(112.32f, 5.76f, -15.84f);

        public static List<Blip> carBlips = new List<Blip>();
        private Dictionary<string, VehicleData> ownedVehicles = new Dictionary<string, VehicleData>();
        public static Dictionary<PlayerCharacter, List<VehicleData>> playerVehicleRegistry = new Dictionary<PlayerCharacter, List<VehicleData>>();

        public Persistence()
        {
            Tick += OnTick;
            Aborted += OnAborted;
            LoadPlayerVehicleRegistry();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Game.IsLoading || Game.IsPaused || !SettingsManager.modEnabled)
                return;

            Ped GPC = Game.Player.Character;
            if (GPC == null) return;

            // OWNER:
            owner = GetPlayerCharacter(GPC);

            // PERSISTENCE ENABLED?:
            if (!SettingsManager.persistenceEnabled) return;

            // ON FOOT:
            if (GPC.IsOnFoot && InteractionManager.closestVehicle != null)
            {
                RaycastResult raycastResult = World.Raycast(GPC.Position, InteractionManager.closestVehicle.Position, IntersectFlags.Vehicles);

                Color markerColor = Color.FromArgb(100, 100, 100, 80);
                bool canSeeVehicle = false;

                if (raycastResult.HitEntity != null && raycastResult.HitEntity.Model.IsCar)
                {
                    canSeeVehicle = InteractionHandler.IsPlayerFacingVehicle(InteractionManager.closestVehicle) && raycastResult.HitEntity == InteractionManager.closestVehicle;

                    if (canSeeVehicle && !Game.IsControlPressed(Control.Aim) && Game.Player.Character.Weapons.Current.LocalizedName != "Jerry Can")
                    {
                        // Save vehicle on foot after locking the vehicle
                        // SaveVehicle(InteractionManager.currentVehicle);
                        InteractWithClosestVehicle(InteractionManager.closestVehicle);
                    }
                }

            }

            // IN VEHICLE:
            if (GPC.IsInVehicle() && InteractionManager.currentVehicle != null)
            {
                Vehicle vehicle = InteractionManager.currentVehicle;
                // Persistence behaviour:
                VehicleData data = GetVehicleData(vehicle);
                if (data == null) return;

                UpdateVehicleData(vehicle, data, Game.LastFrameTime);

                if (!IsVehicleOwned(data))
                {
                    AddToRegistry(data);
                }
                // Player can save vehicle by using keys to lock the vehicle
            }
        }

        public void DisplayVehicleStats(VehicleData data)
        {
            if (data == null) return;

            float fuelConsumed = data.TripFuelLevel / 1000f;
            float distanceKm = data.Tripometer / 1000f;
            float fuelEfficiencyL100Km = distanceKm > 0 ? (fuelConsumed / distanceKm) * 100 : 0;
            float steeringAngle = InteractionManager.currentVehicle.SteeringAngle;

            // FUEL LEVEL:
            if (SettingsManager.fuel_debugEnabled)
            {
                new TextElement(
                    string.Format(
                        $"Weather: {World.Weather.ToString()}\n"
                        + $"Health: {Math.Round(InteractionManager.currentVehicle.HealthFloat)}\n"
                        + $"Temp: {InteractionManager.currentVehicle.EngineTemperature:F2} °C\n"
                        + $"Speed: {Math.Abs(InteractionManager.currentVehicle.Speed * 3.6f):F0} km/h\n"
                        + $"Rate: {Fuel.CalculateFuelConsumptionRate(InteractionManager.currentVehicle):F3}\n"
                        + $"Fuel = {Fuel.CurrentFuel / 1000f:F2} Liters\n"
                        + $"RPM: {InteractionManager.currentVehicle.CurrentRPM * 10000:F0}\n"),
                    new PointF(250.0f, 620f), 0.25f, Color.LightBlue).Draw();
            }
            // TRIPOMETER:
            if (SettingsManager.trip_debugEnabled)
            {
                new TextElement(
                    string.Format(
                        $"{data.TripFuelLevel / 1000f:F2} L / {data.Tripometer / 1000f:F3} km\n"
                        + $"Efficiency: {fuelEfficiencyL100Km:F2} L / 100 km\n"
                        + $"Trip: {distanceKm:F2} km "
                        + $"Odo: {data.Odometer / 1000f:F2} km"),
                    new PointF(65.0f, 635f), 0.20f, Color.LightBlue).Draw();
            }
        }

        public void UpdateVehicleData(Vehicle vehicle, VehicleData data, float deltaTime)
        {
            if (vehicle == null) return;

            Vector3 currentPosition = vehicle.Position;

            float distanceTraveled = data.LastPosition != Vector3.Zero
                ? data.LastPosition.DistanceTo(currentPosition)
                : 0f;

            float fuelLevelDifference = data.LastFuelLevel - vehicle.FuelLevel;

            data.TripFuelLevel += Fuel.CalculateFuelConsumptionRate(vehicle) / 1000f;
            data.Odometer += distanceTraveled;
            data.Tripometer += distanceTraveled;
            data.FuelEfficiency = data.Tripometer / data.TripFuelLevel;

            data.LastPosition = currentPosition;
            data.FuelLevel = vehicle.FuelLevel;
            DisplayVehicleStats(data);
        }
        public static VehicleData GetVehicleData(Vehicle vehicle)
        {
            if (vehicle == null) return null;

            if (TryGetVehicleData(vehicle, out var existingData)) return existingData;

            return new VehicleData
            {
                Owner = owner,
                LicensePlate = vehicle.Mods.LicensePlate,
                ModelHash = vehicle.Model.Hash,
                Brand = Game.GetLocalizedString(N.GetMakeNameFromModel(vehicle.Model)),
                Name = vehicle.LocalizedName,
                FullName = $"{N.GetVehicleFullName(vehicle)}",
                Position = vehicle.Position,
                Heading = vehicle.Heading,
                // Mods = vehicle.Mods,
                PrimaryColor = (int)vehicle.Mods.PrimaryColor,
                SecondaryColor = (int)vehicle.Mods.SecondaryColor,
                FuelLevel = Fuel.CurrentFuel,
                MaxFuel = Fuel.MaxFuel,
                Odometer = Function.Call<float>(Hash.GET_RANDOM_FLOAT_IN_RANGE, 0, 299999999.999f),
                Tripometer = 0f,
                FuelEfficiency = 0f,
                LastPosition = vehicle.Position,
            };
        }
        public static bool TryGetVehicleData(Vehicle vehicle, out VehicleData vehicleData)
        {
            vehicleData = null;
            if (vehicle == null || playerVehicleRegistry == null) return false;

            foreach (var playerEntry in playerVehicleRegistry)
            {
                foreach (var data in playerEntry.Value)
                {
                    if (data.LicensePlate == vehicle.Mods.LicensePlate && data.ModelHash == vehicle.Model.Hash)
                    {
                        vehicleData = data;
                        return true;
                    }
                }
            }
            return false;
        }

        public static void ResetTripometer(Vehicle vehicle)
        {
            VehicleData data = GetVehicleData(vehicle);
            if (data == null) return;
            data.Tripometer = 0f;
            data.TripFuelLevel = data.FuelLevel;
        }


        #region LOCKING / UNLOCKING:
        // INTERACTION:
        private void InteractWithClosestVehicle(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists()) return;

            VehicleLockStatus lockStatus = vehicle.LockStatus;
            VehicleData data = GetVehicleData(vehicle);

            bool isOwned = IsVehicleOwned(data);

            if (isOwned)
            {
                N.ShowHelpText($"Press ~INPUT_CONTEXT~ to {(lockStatus == VehicleLockStatus.CannotEnter ? "unlock" : "lock")} the {data.Name} or ~INPUT_CONTEXT_SECONDARY~ to abandon it");

                if (Game.IsControlJustReleased(Control.ContextSecondary))
                {
                    AbandonVehicle(vehicle);
                }
                else if (Game.IsControlJustReleased(Control.Context))
                {
                    if (lockStatus == VehicleLockStatus.CannotEnter)
                    {
                        // N.ShowHelpText($"Press ~INPUT_CONTEXT~ to unlock the {data.LocalizedName} or ~INPUT_CONTEXT_SECONDARY~ to abandon it");
                        UnlockVehicle(vehicle);
                    }
                    else
                    {
                        LockVehicle(vehicle);
                        SaveVehicle(vehicle);
                    }
                }
            }


        }
        private void SaveVehicle(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null || !vehicle.Exists()) return;
                VehicleData data = GetVehicleData(vehicle);
                if (data == null) return;

                vehicle.IsPersistent = true;
                AIS.AttachBlip(vehicle, BlipSprite.PersonalVehicleCar, 0.8f, BlipColor.White, data.FullName, carBlips);

                SaveVehicleToXml(data);
            }
            catch (UnauthorizedAccessException ex)
            {
                N.ShowSubtitle($"Access denied: {ex.Message}", 2500);
            }
            catch (Exception ex)
            {
                N.ShowSubtitle($"Error saving vehicle: {ex.Message}", 2500);
            }
        }
        private void AbandonVehicle(Vehicle vehicle)
        {

            if (vehicle == null) return;

            vehicle.IsPersistent = false;
            vehicle.MarkAsNoLongerNeeded();

            Blip blip = vehicle.AttachedBlip;

            if (blip != null)
            {
                if (blip.Exists())
                {
                    blip.Delete();
                }
                if (carBlips.Contains(blip))
                {
                    carBlips.Remove(blip);
                }
            }

            VehicleData data = GetVehicleData(vehicle);

            if (data != null)
            {
                try
                {
                    if (SettingsManager.persistence_debugEnabled)
                    {
                        N.ShowSubtitle($"Abandoning Vehicle: {data.FullName}", 2500);
                    }

                    string plate = vehicle.Mods.LicensePlate;
                    string fileName = $"{plate}.xml";
                    string fullPath = Path.Combine(path, owner.ToString(), fileName);

                    if (File.Exists(fullPath))
                    {
                        File.SetAttributes(fullPath, FileAttributes.Normal);
                        File.Delete(fullPath);

                        if (SettingsManager.persistence_debugEnabled)
                        {
                            N.ShowSubtitle($"{vehicle.LocalizedName} : {plate} - File deleted", 2500);
                        }
                    }

                    playerVehicleRegistry[owner].Remove(data);
                }
                catch (UnauthorizedAccessException ex)
                {
                    AIS.LogException("AbandonVehicle - Access Denied", ex);
                }
                catch (IOException ex)
                {
                    AIS.LogException("AbandonVehicle - File I/O Error", ex);
                }
                catch (Exception ex)
                {
                    AIS.LogException("AbandonVehicle", ex);
                }
            }

        }
        public static void UnlockVehicle(Vehicle vehicle)
        {
            if (SettingsManager.persistence_debugEnabled)
            {
                N.ShowSubtitle($"{vehicle.LocalizedName} unlocked", 2500);
            }

            PlayLockingAnimation(vehicle, 1500);
            PlayUnlockSound();

            vehicle.IsAlarmSet = false;
            vehicle.LockStatus = VehicleLockStatus.Unlocked;
        }
        public static void LockVehicle(Vehicle vehicle)
        {
            if (SettingsManager.persistence_debugEnabled)
            {
                N.ShowSubtitle($"{vehicle.LocalizedName} locked", 2500);
            }

            PlayLockingAnimation(vehicle, 1500);
            PlayLockSound();

            vehicle.LockStatus = VehicleLockStatus.CannotEnter;
            vehicle.IsAlarmSet = true;
        }
        // KEY PROP:
        public static Entity AttachKey(Ped ped)
        {
            try
            {
                if (Game.Player.Character.Bones["IK_R_Hand"].Index != -1)
                {
                    HideWeapon(ped);
                    int handle = Function.Call<int>(Hash.CREATE_OBJECT, lr_prop_carkey_fob, ped.Position.X, ped.Position.Y, ped.Position.Z, true, true, true);
                    Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, handle, ped, ped.Bones["IK_R_Hand"].Index, carkeyObjectPosition.X, carkeyObjectPosition.Y, carkeyObjectPosition.Z, carkeyObjectPosition.X, carkeyObjectPosition.Y, carkeyObjectPosition.Z, false, false, false, false, 2, true);
                    return Entity.FromHandle(handle);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("AIS.AttachKeyFobObject", ex);
            }
            return null;
        }
        // SOUND:
        public static void PlayLockingAnimation(Vehicle vehicle, int duration)
        {
            try
            {
                if (vehicle == null) return;

                carkeyProp = AttachKey(Game.Player.Character);

                Game.Player.Character.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10f, 1500, (AnimationFlags)49);
                Game.Player.Character.Task.LookAt(vehicle.Position, duration);

                Script.Wait(500);
                carkeyProp.Detach();
                carkeyProp.Delete();
                Script.Wait(500);

                Function.Call(Hash.SET_PED_CURRENT_WEAPON_VISIBLE, Game.Player.Character, true);
            }
            catch (Exception ex)
            {
                AIS.LogException("AIS.PlayLockingAnimation", ex);
            }
        }
        public static void PlayLockSound()
        {
            AudioManager.PlaySound("lock.wav");
        }
        public static void PlayUnlockSound()
        {
            AudioManager.PlaySound("unlock.wav");
        }
        private static void HideWeapon(Ped ped)
        {
            Function.Call(Hash.SET_PED_CURRENT_WEAPON_VISIBLE, ped, false, true, true, true);
        }
        #endregion

        private PlayerCharacter GetPlayerCharacter(Ped player)
        {
            Model model = player.Model;
            PlayerCharacter character;

            if (model == PedHash.Michael)
            {
                character = PlayerCharacter.Michael;
            }
            else if (model == PedHash.Franklin)
            {
                character = PlayerCharacter.Franklin;
            }
            else if (model == PedHash.Trevor)
            {
                character = PlayerCharacter.Trevor;
            }
            else
            {
                character = PlayerCharacter.Other;
            }

            if (SettingsManager.persistence_debugEnabled)
            {
                // N.ShowSubtitle($"Owner: {character}", 500);
            }

            return character;
        }

        // FILE CHECK: 
        public static bool CheckDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                    return true;
                }
                catch (Exception ex)
                {
                    AIS.LogException("LoadPlayerVehicleRegistry()", ex);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // REGISTRY:
        private bool IsVehicleOwned(VehicleData data)
        {
            return playerVehicleRegistry[owner].Contains(data);
        }
        private void AddToRegistry(VehicleData data)
        {
            playerVehicleRegistry[owner].Add(data);
            N.DisplayNotification($"{data.Owner} found the keys to the ~b~{data.FullName}~s~", false);
        }
        private void RemoveFromRegistry(Vehicle vehicle)
        {
            VehicleData data = GetVehicleData(vehicle);
            playerVehicleRegistry[owner].Remove(data);
            vehicle.IsPersistent = false;
            N.DisplayNotification($"{data.Owner} abandoned the keys to the ~b~{data.FullName}~s~", false);
        }
        private void ClearRegistry()
        {
            playerVehicleRegistry.Clear();
        }
        // SAVING:
        private void SavePlayerVehicleRegistry()
        {
            foreach (var entry in playerVehicleRegistry)
            {
                PlayerCharacter player = entry.Key;
                List<VehicleData> vehicles = entry.Value;

                string directoryPath = Path.Combine(path, player.ToString());

                if (CheckDirectoryExists(directoryPath))
                {
                    foreach (VehicleData data in vehicles)
                    {
                        try
                        {
                            string fileName = $"{data.LicensePlate}.xml";
                            string filePath = Path.Combine(directoryPath, fileName);

                            SaveVehicleToXml(data);
                        }
                        catch (Exception ex)
                        {
                            AIS.LogException("SavePlayerVehicleRegistry", ex);
                        }
                    }
                }
            }
        }
        public static void SaveVehicleToXml(VehicleData data)
        {
            try
            {
                string directoryPath = Path.Combine(path, data.Owner.ToString());
                string fileName = $"{data.LicensePlate}.xml";
                string filePath = Path.Combine(directoryPath, fileName);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);

                    if (SettingsManager.persistence_debugEnabled)
                    {
                        N.ShowSubtitle($"Created directory: {directoryPath}", 1000);
                    }
                }
                if (!File.Exists(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VehicleData));
                    FileSecurity fileSecurity = new FileSecurity();
                    using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        serializer.Serialize(stream, data);
                    }
                }

                N.DisplayNotificationSMS(NotificationIcon.MpMorsMutual, "Mors Mutual Insurance", "", $"{data.FullName} added to {owner}'s Insurance Policy", false, false);

                if (SettingsManager.persistence_debugEnabled)
                {
                    N.ShowSubtitle($"{data.Owner}'s {data.FullName} - {data.LicensePlate} saved to {filePath}", 2500);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                AIS.LogException("SaveVehicleToXml()", ex);
            }
        }

        // LOADING
        public static void LoadPlayerVehicleRegistry()
        {
            playerVehicleRegistry.Clear();

            foreach (PlayerCharacter player in Enum.GetValues(typeof(PlayerCharacter)))
            {
                if (SettingsManager.persistence_debugEnabled)
                {
                    N.DisplayNotification($"Loading Registry for: ~b~{player}~s~", true);
                }

                playerVehicleRegistry[player] = new List<VehicleData>();
                string directoryPath = Path.Combine(path, player.ToString());

                if (CheckDirectoryExists(directoryPath))
                {
                    string[] files = Directory.GetFiles(directoryPath, "*.xml");

                    foreach (string file in files)
                    {
                        try
                        {
                            VehicleData data = LoadVehicleFromXml(file);

                            if (SettingsManager.persistence_debugEnabled)
                            {
                                N.DisplayNotification($"Loading Registry - {player} : {file}", true);
                            }

                            if (data != null)
                            {
                                playerVehicleRegistry[player].Add(data);
                                // LoadVehicle();
                                Vehicle[] nearVehicles = World.GetNearbyVehicles(data.Position, 5f);
                                foreach (Vehicle vehicle in nearVehicles)
                                {
                                    if (vehicle == null)
                                    {
                                        Vehicle newVehicle = World.CreateVehicle(data.ModelHash, data.Position, data.Heading);
                                        newVehicle.Mods.LicensePlate = data.LicensePlate;
                                        newVehicle.Mods.PrimaryColor = (VehicleColor)data.PrimaryColor;
                                        newVehicle.Mods.SecondaryColor = (VehicleColor)data.SecondaryColor;
                                    }
                                    else if (vehicle.Model == data.ModelHash)
                                    {
                                        vehicle.Mods.LicensePlate = data.LicensePlate;
                                        vehicle.Mods.PrimaryColor = (VehicleColor)data.PrimaryColor;
                                        vehicle.Mods.SecondaryColor = (VehicleColor)data.SecondaryColor;
                                    }
                                    else
                                    {
                                        if (SettingsManager.persistence_debugEnabled)
                                        {
                                            N.ShowSubtitle($"Could not load vehicle: {data.FullName} ~n~Position occupied", 2500);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (SettingsManager.persistence_debugEnabled)
                                {
                                    N.DisplayNotification($"Failed to load vehicle data: {file}", true);

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AIS.LogException($"LoadPlayerVehicleRegistry() - Failed to load data from: {player} - {file}", ex);
                        }
                    }
                }
                else
                {
                    if (SettingsManager.persistence_debugEnabled)
                    {
                        N.DisplayNotification($"Directory not found for: {player} : {directoryPath}", true);
                    }
                }
            }
        }
        public static VehicleData LoadVehicleFromXml(string fileName)
        {
            try
            {
                AIS.LogDebug($"Attempting to load XML file from {fileName}");

                if (File.Exists(fileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VehicleData));

                    using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        VehicleData vehicleData = (VehicleData)serializer.Deserialize(stream);
                        AIS.LogDebug("Deserialization completed successfully");
                        return vehicleData;
                    }
                }
                else
                {
                    AIS.LogDebug($"File not found: {fileName}");
                    return null;
                }
            }
            catch (FileNotFoundException fnfEx)
            {
                AIS.LogException($"File not found: {fileName}", fnfEx);
                return null;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                AIS.LogException($"Access denied to file: {fileName}", uaEx);
                return null;
            }
            catch (IOException ioEx)
            {
                AIS.LogException($"IO error when loading file: {fileName}", ioEx);
                return null;
            }
            catch (Exception ex)
            {
                AIS.LogException("An unexpected error occurred in LoadVehicleFromXml", ex);
                return null;
            }
        }


        private void OnAborted(object sender, EventArgs e)
        {
            // SavePlayerVehicleRegistry();
            AIS.DeleteAllBlips(carBlips);
            playerVehicleRegistry.Clear();
        }
    }
}
