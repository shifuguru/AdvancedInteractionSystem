using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System.Linq;

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
        public string BrandName { get; set; }
        public string LocalizedName { get; set; }
        public string FullName { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
        // public float FuelLevel { get; set; }
        // public Dictionary<int, int> Modifications { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is VehicleData other)
            {
                return LicensePlate == other.LicensePlate &&
                    ModelHash == other.ModelHash &&
                    Owner == other.Owner;
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
        public static bool persistence_debugEnabled = SettingsManager.persistence_debugEnabled;
        public static float persistenceDistance = SettingsManager.persistenceDistance;
        public static Model carKeyModel = new Model("lr_prop_carkey_fob");
        public static string xmlPath = "scripts\\AIS\\vehicles";
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


        // Maps each player character to a list of vehicles they own: 
        public static Dictionary<PlayerCharacter, List<VehicleData>> playerVehicleRegistry = new Dictionary<PlayerCharacter, List<VehicleData>>();

        public Persistence()
        {
            Tick += OnTick;
            Aborted += OnAborted;

            LoadPlayerVehicleRegistry();
        }


        private void LoadPlayerVehicleRegistry()
        {
            playerVehicleRegistry.Clear();

            foreach (PlayerCharacter player in Enum.GetValues(typeof(PlayerCharacter)))
            {
                playerVehicleRegistry[player] = new List<VehicleData>();

                string directoryPath = Path.Combine(xmlPath, player.ToString());
                if (Directory.Exists(directoryPath))
                {
                    string[] files = Directory.GetFiles(directoryPath, "*.xml");
                    foreach (string file in files)
                    {
                        VehicleData data = LoadVehicleFromXml(file);
                        if (data != null)
                        {
                            playerVehicleRegistry[player].Add(data);
                        }
                    }
                }
            }
        }

        private void SavePlayerVehicleRegistry()
        {
            foreach (var entry in playerVehicleRegistry)
            {
                PlayerCharacter player = entry.Key;
                List<VehicleData> vehicles = entry.Value;

                string directoryPath = Path.Combine(xmlPath, player.ToString());
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                foreach (VehicleData data in vehicles)
                {
                    SaveVehicleToXml(data);
                }
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Game.IsLoading || Game.IsPaused || !SettingsManager.modEnabled || !SettingsManager.persistenceEnabled) 
                return;

            Ped GPC = Game.Player.Character;
            if (GPC == null || !GPC.IsOnFoot || Game.IsControlPressed(Control.Aim))
                return;
            owner = GetPlayerCharacter(GPC);

            Vehicle lastVehicle = GPC.LastVehicle;
            if (lastVehicle != null && lastVehicle.Exists() && !IsVehicleOwned(owner, lastVehicle))
            {
                SaveVehicle(owner, lastVehicle);
            }
            
            Vehicle closestVehicle = World.GetClosestVehicle(GPC.Position, persistenceDistance);
            if (closestVehicle != null && closestVehicle.Exists())
            {
                InteractWithClosestVehicle(closestVehicle);
            }
        }

        private void InteractWithClosestVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            VehicleData data = CreateVehicleData(vehicle, owner);

            bool isOwned = playerVehicleRegistry[owner].Any(v => v.Equals(data));
            VehicleLockStatus lockStatus = vehicle.LockStatus;

            if (isOwned)
            {
                N.ShowHelpText($"Press ~INPUT_CONTEXT~ to {(lockStatus == VehicleLockStatus.CannotEnter ? "unlock" : "lock")} the {data.FullName} or ~INPUT_CONTEXT_SECONDARY~ to abandon it");

                if (Game.IsControlJustReleased(Control.ContextSecondary))
                {
                    AbandonVehicle(vehicle);
                }
                else if (Game.IsControlJustReleased(Control.Context))
                {
                    if (lockStatus == VehicleLockStatus.CannotEnter)
                    {
                        N.ShowHelpText($"Press ~INPUT_CONTEXT~ to unlock the {data.FullName} or ~INPUT_CONTEXT_SECONDARY~ to abandon it");
                        UnlockVehicle(vehicle);
                    }
                    else
                    {
                        LockVehicle(vehicle);
                        // SaveVehicle(closestVehicle, GetPlayerCharacter(Game.Player.Character));
                    }
                }
            }
        }



        private PlayerCharacter GetPlayerCharacter(Ped player)
        {
            Player gamePlayer = Function.Call<Player>(Hash.GET_PLAYER_NAME, player);
            string playerName = Function.Call<string>(Hash.GET_PLAYER_PED, gamePlayer);

            if (persistence_debugEnabled && !string.IsNullOrEmpty(playerName))
            {
                N.ShowSubtitle($"Owner: {playerName}", 500);
            }

            switch (playerName)
            {
                case "MICHAEL":
                    return PlayerCharacter.Michael;
                case "FRANKLIN":
                    return PlayerCharacter.Franklin;
                case "TREVOR":
                    return PlayerCharacter.Trevor;
                default:
                    return PlayerCharacter.Other;
            }
        }

        private bool IsVehicleOwned(PlayerCharacter owner, Vehicle vehicle)
        {
            return playerVehicleRegistry[owner].Any(v => v.LicensePlate == vehicle.Mods.LicensePlate);
        }

        public static void UnlockVehicle(Vehicle vehicle)
        {
            if (persistence_debugEnabled)
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
            if (persistence_debugEnabled)
            {
                N.ShowSubtitle($"{vehicle.LocalizedName} locked", 2500);
            }

            PlayLockingAnimation(vehicle, 1500);
            PlayLockSound();

            vehicle.LockStatus = VehicleLockStatus.CannotEnter;
            vehicle.IsAlarmSet = true;
        }

        private void AbandonVehicle(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null) return;

                //PlayerCharacter owner = GetPlayerCharacter(Game.Player.Character);
                VehicleData data = CreateVehicleData(vehicle, owner);

                playerVehicleRegistry[owner].Remove(data);

                if (persistence_debugEnabled)
                {
                    N.ShowSubtitle($"Vehicle {data.FullName} abandoned", 2500);
                }

                vehicle.IsPersistent = false;
                vehicle.MarkAsNoLongerNeeded();
                
                Blip blip = vehicle.AttachedBlip;

                if (blip != null && carBlips.Contains(blip))
                {
                    if (blip.Exists())
                    {
                        blip.Delete();
                    }
                    carBlips.Remove(blip);
                }

                /*
                string plate = vehicle.Mods.LicensePlate;
                string fileName = $"{plate}.xml";
                string fullPath = Path.Combine(xmlPath, fileName);

                if (File.Exists(fullPath))
                {
                    if (persistence_debugEnabled)
                    {
                        N.ShowSubtitle($"Vehicle {vehicle.LocalizedName} removed. File {plate} deleted", 2500);
                    }
                    // File.Delete(fullPath);
                }
                */

                if (vehicle.Exists())
                {
                    /*
                    Function.Call(Hash.SET_ENTITY_AS_NO_LONGER_NEEDED, vehicle);
                    vehicle.MarkAsNoLongerNeeded();
                    vehicle.IsPersistent = false;
                    */
                }
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

        private void SaveVehicle(PlayerCharacter owner, Vehicle vehicle)
        {
            try
            {
                if (vehicle != null)
                {
                    VehicleData data = CreateVehicleData(vehicle, owner);

                    if (!playerVehicleRegistry[owner].Contains(data))
                    {
                        AIS.CreateBlip(vehicle.Position, BlipSprite.PersonalVehicleCar, BlipColor.White, data.FullName, carBlips);
                        playerVehicleRegistry[owner].Add(data);
                        SaveVehicleToXml(data);

                        if (persistence_debugEnabled)
                        {
                            N.ShowSubtitle($"Vehicle {data.FullName} saved for Player {owner}!", 2500);
                        }
                    }
                    else if (persistence_debugEnabled)
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


        public static Entity AttachKeyFobObject()
        {
            try
            {
                Ped GPC = Game.Player.Character;
                if (GPC.Bones[28422].Index != -1)
                {
                    // Prop prop = World.CreateProp(lr_prop_carkey_fob, Vector3.Add(GPC.Position, GPC.ForwardVector), Vector3.Zero, true, false);
                    // prop.AttachTo(GPC, GPC.Bones[28422], Vector3.Zero, Vector3.Zero);

                    int handle = Function.Call<int>(Hash.CREATE_OBJECT, lr_prop_carkey_fob, GPC.Position.X, GPC.Position.Y, GPC.Position.Z, true, true, true);
                    Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, handle, GPC, GPC.Bones[28422].Index, carkeyObjectPosition.X, carkeyObjectPosition.Y, carkeyObjectPosition.Z, carkeyObjectRotation.X, carkeyObjectRotation.Y, carkeyObjectRotation.Z, false, false, false, false, 2, true);
                    return Entity.FromHandle(handle);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("AIS.AttachKeyFobObject", ex);
            }
            return null;
        }

        public static void PlayLockingAnimation(Vehicle vehicle, int duration)
        {
            try
            {
                if (vehicle == null) return;

                Ped ped = Game.Player.Character;
                Function.Call(Hash.SET_PED_CURRENT_WEAPON_VISIBLE, ped, false); // Not sure if this is a good function to use...

                carkeyProp = AttachKeyFobObject();

                Game.Player.Character.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10f, 1500, (AnimationFlags)49);
                Game.Player.Character.Task.LookAt(vehicle.Position, duration);

                // Script.Wait(500);
                carkeyProp.Detach();
                carkeyProp.Delete();
                // Script.Wait(500);
                Function.Call(Hash.SET_PED_CURRENT_WEAPON_VISIBLE, ped, true);
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

        // Functions: 
        #region Functions:
        private VehicleData CreateVehicleData(Vehicle vehicle, PlayerCharacter owner)
        {
            return new VehicleData
            {
                Owner = owner,
                LicensePlate = vehicle.Mods.LicensePlate,
                ModelHash = vehicle.Model.Hash,
                BrandName = Game.GetLocalizedString(N.GetMakeNameFromModel(vehicle.Model)),
                LocalizedName = vehicle.LocalizedName,
                FullName = $"{Game.GetLocalizedString(N.GetMakeNameFromModel(vehicle.Model))} {vehicle.LocalizedName}",
                Position = vehicle.Position,
                Heading = vehicle.Heading,
                PrimaryColor = (int)vehicle.Mods.PrimaryColor,
                SecondaryColor = (int)vehicle.Mods.SecondaryColor,
            };
        }

        #endregion


        public static List<Blip> carBlips = new List<Blip>();

        public static void SaveVehicleToXml(VehicleData data)
        {
            try
            {
                string fileName = $"{data.LicensePlate}.xml";
                string fullPath = Path.Combine(xmlPath, fileName);

                XmlSerializer serializer = new XmlSerializer(typeof(VehicleData));

                using (FileStream stream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    serializer.Serialize(stream, data);
                }

                FileSecurity fileSecurity = new FileSecurity();
                fileSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.Read | FileSystemRights.Write,
                    AccessControlType.Allow));

                File.SetAccessControl(fullPath, fileSecurity);

                if (persistence_debugEnabled)
                {
                    N.ShowSubtitle($"{data.FullName} : {data.LicensePlate} : {data.Owner} saved to {xmlPath}", 2500);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("Persistence.SaveVehicleToXml", ex);
            }   
        }
        public static VehicleData LoadVehicleFromXml(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(xmlPath, fileName);

                if (File.Exists(fullPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VehicleData));

                    using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
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




        private void OnAborted(object sender, EventArgs e)
        {
            SavePlayerVehicleRegistry();
            AIS.DeleteAllBlips(carBlips);
            playerVehicleRegistry.Clear();
        }
    }
}
