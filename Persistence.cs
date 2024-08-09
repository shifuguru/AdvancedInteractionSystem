using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    public enum PlayerCharacter
    {
        Michael, 
        Franklin, 
        Trevor, 
        Other
    }
    public class VehicleData
    {
        public PlayerCharacter Owner { get; set; }
        public string LicensePlate {  get; set; }
        public int ModelHash { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public string ModelName { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
        // public float FuelLevel { get; set; }
        // public Dictionary<int, int> Modifications { get; set; }
    }
    
    public class Persistence : Script
    {
        public static bool persistence_debugEnabled = SettingsManager.persistence_debugEnabled;
        public static Model carKeyModel = new Model("lr_prop_carkey_fob");
        public static string xmlPath = "scripts\\AIS\\vehicles";
        public static bool IsVehicleLoaded = false;
        public static bool IsVehicleLoading = false;
        public static bool showBlips = true;
        public static Control saveKey = (Control)51;
        public static bool dispVehName = false;
        public static bool saveDamage = true;

        public static List<Blip> carBlips = new List<Blip>();

        private Dictionary<PlayerCharacter, List<VehicleData>> playerVehicles = new Dictionary<PlayerCharacter, List<VehicleData>>();
       
        public List<VehicleData> GetVehiclesForPlayer(PlayerCharacter player)
        {
            return playerVehicles.ContainsKey(player) ? playerVehicles[player] : new List<VehicleData>();
        }
        
        private void DeleteAllBlips()
        {
            foreach (Blip blip in carBlips)
            {
                blip.Delete();
            }
            carBlips.Clear();
        }
        private void CreateBlip(Vehicle vehicle)
        {
            Blip blip = World.CreateBlip(vehicle.Position);
            blip.Name = Function.Call<string>(Hash.GET_MAKE_NAME_FROM_VEHICLE_MODEL, vehicle.Model) + " " + vehicle.LocalizedName;
            blip.Color = BlipColor.White;
            blip.Sprite = BlipSprite.PersonalVehicleCar;
            blip.IsShortRange = false;
            carBlips.Add(blip);
        }

        private void RunPersistence(Vehicle lastVehicle)
        {
            if (lastVehicle == null) return;
            Vehicle closestVehicle = World.GetClosestVehicle(Game.Player.Character.Position, 15f);
            if (closestVehicle == null) return;
            
            if (closestVehicle != lastVehicle) return;

            VehicleLockStatus lockStatus = lastVehicle.LockStatus;

            if (lockStatus == VehicleLockStatus.CannotEnter)
            {
                N.ShowHelpText($"Press ~INPUT_CONTEXT~ to unlock the {lastVehicle.LocalizedName} or ~INPUT_CONTEXT_SECONDARY~ to abandon it");

                if (Game.IsControlJustReleased(Control.Context))
                {
                    lastVehicle.LockStatus = VehicleLockStatus.Unlocked;
                }
                if (Game.IsControlJustReleased(Control.ContextSecondary))
                {
                    RemoveVehicle(lastVehicle);
                }
            }
            else
            {
                N.ShowHelpText($"Press ~INPUT_CONTEXT~ to lock the {lastVehicle.LocalizedName}");

                if (Game.IsControlJustReleased(Control.Context))
                {
                    lastVehicle.LockStatus = VehicleLockStatus.CannotEnter;
                    SaveVehicle(lastVehicle, GetPlayerCharacter(Game.Player.Character));
                }
            }
        }

        public Persistence()
        {
            // Initialise Player Vehicles Dictionary: 
            foreach (PlayerCharacter player in Enum.GetValues(typeof(PlayerCharacter)))
            {
                playerVehicles[player] = new List<VehicleData>();
            }

            Tick += OnTick;
            Aborted += OnAborted;
            LoadAllVehicles();
        }

        private PlayerCharacter GetPlayerCharacter(Ped player)
        {
            string playerName = Function.Call<string>(Hash.GET_PLAYER_NAME, Game.Player.Character);

            switch (playerName)
            {
                case "Michael":
                    return PlayerCharacter.Michael;
                case "Franklin":
                    return PlayerCharacter.Franklin;
                case "Trevor":
                    return PlayerCharacter.Trevor;
                default:
                    return PlayerCharacter.Other;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!Game.IsLoading || !Game.IsPaused)
            {
                if (!SettingsManager.modEnabled || !SettingsManager.persistenceEnabled) 
                    return;

                Ped GPC = Game.Player.Character; 
                if (GPC == null) return;
                if (!GPC.IsOnFoot || Game.IsControlPressed(Control.Aim)) return;

                Vehicle lastVehicle = GPC.LastVehicle;

                if (lastVehicle != null)
                {
                    RunPersistence(lastVehicle);
                }
            }
        }

        private void SaveVehicleToXml(VehicleData data)
        {
            string fileName = $"{data.LicensePlate}.xml";
            string fullPath = Path.Combine(xmlPath, fileName);

            XmlSerializer serializer = new XmlSerializer(typeof(VehicleData));

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                serializer.Serialize(stream, data);
            }

            N.ShowSubtitle($"Vehicle {data.ModelName} saved to XML!", 2500);
        }

        private VehicleData LoadVehicleFromXml(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(xmlPath, fileName);

                if (File.Exists(fullPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VehicleData));

                    using (FileStream stream = new FileStream(xmlPath, FileMode.Open))
                    {
                        return (VehicleData)serializer.Deserialize(stream);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                AIS.LogException("LoadVehicleFromXml", ex);
                return null;
            }
        }

        private void LoadAllVehicles()
        {
            DeleteAllBlips();

            if (!Directory.Exists(xmlPath))
            {
                Directory.CreateDirectory(xmlPath);
            }
            string[] files = Directory.GetFiles(xmlPath, "*.xml");

            foreach (string file in files)
            {
                VehicleData data = LoadVehicleFromXml(Path.GetFileName(file));

                if (data != null)
                {
                    if (!playerVehicles.ContainsKey(data.Owner))
                    {
                        playerVehicles[data.Owner] = new List<VehicleData>();
                    }

                    playerVehicles[data.Owner].Add(data);
                }
            }

            if (persistence_debugEnabled)
            {
                N.ShowSubtitle($"Loaded {files.Length} vehicle(s) from XML.", 2500);
            }
        }

        private void SaveVehicle(Vehicle vehicle, PlayerCharacter owner)
        {
            try
            {
                if (vehicle != null)
                {
                    CreateBlip(vehicle);

                    VehicleData data = new VehicleData
                    {
                        Owner = owner,
                        LicensePlate = vehicle.Mods.LicensePlate,
                        ModelHash = vehicle.Model.Hash,
                        ModelName = vehicle.LocalizedName,
                        Position = vehicle.Position,
                        Heading = vehicle.Heading,
                        PrimaryColor = (int)vehicle.Mods.PrimaryColor,
                        SecondaryColor = (int)vehicle.Mods.SecondaryColor,
                        // FuelLevel = Fuel.currentFuel
                    };
                    playerVehicles[owner].Add(data);
                    SaveVehicleToXml(data);

                    Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, vehicle, true, true);
                    
                    if (persistence_debugEnabled)
                    {
                        N.ShowSubtitle($"Vehicle {vehicle.LocalizedName} saved for Player {owner}!", 2500);
                    }
                }
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
        private void RemoveVehicle(Vehicle vehicle)
        {
            if (vehicle == null) return;
            if (vehicle.AttachedBlip != null)
            {
                Blip blip = vehicle.AttachedBlip;
                if (carBlips.Contains(blip))
                {
                    carBlips.Remove(blip);
                }
                blip.Delete();
            }
            
            Function.Call(Hash.SET_ENTITY_AS_NO_LONGER_NEEDED, vehicle);

            string fileName = $"{vehicle.Mods.LicensePlate}.xml";
            string fullPath = Path.Combine(xmlPath, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                N.ShowSubtitle($"Vehicle {vehicle.LocalizedName} removed. File {fileName} deleted", 2500);
            }
        }
        
        private void OnAborted(object sender, EventArgs e)
        {
            DeleteAllBlips();
        }
    }
}
