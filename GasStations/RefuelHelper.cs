using System;
using System.Collections.Generic;
using System.ComponentModel;
using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    public class RefuelHelper : Script
    {
        // LIST OF GAS STATIONS:
        public static Vector3[] gasStations = new Vector3[27]
        {
            new Vector3(-2555f, 2334f, 30f),
            new Vector3(-2097f, -320f, 30f),
            new Vector3(-1799f, 803f, 30f),
            new Vector3(-1434f, -274f, 30f),
            new Vector3(-724f, -935f, 30f),
            new Vector3(-526f, -1212f, 30f),
            new Vector3(-90f, 6415f, 30f),
            new Vector3(-71f, -1762f, 30f),
            new Vector3(50f, 2776f, 30f),
            new Vector3(180f, 6603f, 30f),
            new Vector3(264f, 2609f, 30f),
            new Vector3(265f, -1261f, 30f),
            new Vector3(621f, 269f, 30f),
            new Vector3(819f, -1027f, 30f),
            new Vector3(1182f, -330f, 30f),
            new Vector3(1209f, -1402f, 30f),
            new Vector3(1212f, 2657f, 30f),
            new Vector3(1687f, 4929f, 30f),
            new Vector3(1702f, 6418f, 30f),
            new Vector3(1786f, 3331f, 30f),
            new Vector3(2005f, 3775f, 30f),
            new Vector3(2537f, 2593f, 30f),
            new Vector3(2581f, 362f, 30f),
            new Vector3(2683f, 3264f, 30f),
            //
            new Vector3(7090.21f, 10429.16f, 10.10f),
            new Vector3(7003.55f, 8876.31f, 10.10f),
            // Chilliad Town:
            new Vector3(354.86f, 5370.63f, 670.02f),
        };
        public static List<Blip> fuelBlips = new List<Blip>();
        public static Blip fuelBlip;
        public static Blip closestBlip;
        public static Vector3 closestGasStation = Vector3.Zero; // Furthest gas station
        public static float closestDistance = float.MaxValue;
        public static float gasStationRadius = 15f;
        public static float fuelPumpRadius = 2.5f;
        public static bool blipsAreFlashing = false;

        public static int refillCost = 0;
        public static float fuelPrice = 3.5f;
        public static float fuelRefilled = 0f;
        public static float initialFuel = 0f;
        public static float refilledFuel = 0f;
        public static bool isRefueling = false;
        // public static float maxFuel = Fuel.maxFuel;

        public RefuelHelper()
        {
            CreateGasStations();
        }

        #region GAS STATIONS:
        public static void CreateGasStations()
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, "weapon@w_sp_jerrycan");

            // CREATE GAS STATION BLIPS: 
            for (int index = 0; index < gasStations.Length; ++index)
            {
                AIS.CreateBlip(gasStations[index], (BlipSprite)361, 0.6f, BlipColor.White, "Gas Station", fuelBlips);
            }
        }
        public static void GetClosestGasStation()
        {
            Vector3 playerPosition = Game.Player.Character.Position;
            closestDistance = float.MaxValue;

            foreach (Blip blip in fuelBlips)
            {
                float distance = Vector3.Distance(playerPosition, blip.Position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestGasStation = blip.Position;
                    closestBlip = blip;
                }
            }
        }
        // PUMP MANAGER:
        public static void PumpLogic(Vehicle vehicle)
        {
            if (!IsVehicleNearGasStation(vehicle)) return;

            if (!IsVehicleNearAnyPump(vehicle))
            {
                N.ShowSubtitle("Get closer to a fuel pump to refuel", 1500);
                return;
            }

            if (Fuel.CurrentFuel >= Fuel.MaxFuel || !vehicle.IsOnAllWheels || !vehicle.IsStopped || vehicle.IsUpsideDown || vehicle.IsDead)
            {
                // Tell the player to right their vehicle?
                if (isRefueling)
                {
                    isRefueling = false;
                    FinishedRefueling();
                }
                return;
            }

            if (vehicle.IsEngineRunning)
            {
                isRefueling = false;
                N.ShowHelpText("Hold ~INPUT_VEH_HEADLIGHT~ to turn Engine off before refueling");
                return;
            }

            float fuelToFill = (Fuel.MaxFuel - Fuel.CurrentFuel) / 1000;
            int totalCostToFill = (int)Math.Round(fuelPrice * fuelToFill, 1, MidpointRounding.AwayFromZero);
            string message = $"Hold ~INPUT_VEH_HANDBRAKE~ to refuel ~n~Price per liter: ~g~${fuelPrice}~s~";

            // START FUELLING: 
            if (Game.IsControlPressed(Control.VehicleHandbrake))
            {
                if (!isRefueling)
                {
                    isRefueling = true;
                    initialFuel = Fuel.CurrentFuel;
                }
                if (isRefueling)
                {
                    Refuel(vehicle);
                    float fuelFilled = (Fuel.CurrentFuel - initialFuel) / 1000;
                    int fuelFilledCost = (int)Math.Round(fuelPrice * fuelFilled, 1, MidpointRounding.AwayFromZero);
                    N.ShowHelpText($"{message} ~n~Total: ~g~${fuelFilledCost}~s~");
                }
            }
            // STOP FUELLING: 
            if (Game.IsControlJustReleased(Control.VehicleHandbrake))
            {
                if (isRefueling)
                {
                    isRefueling = false;
                    FinishedRefueling();
                    Persistence.ResetTripometer(vehicle);
                }
            }
            if (!isRefueling)
            {
                N.ShowHelpText($"{message} ~n~Total to fill: ~g~${totalCostToFill}~s~");
            }
        }

        private static readonly Model[] GasPumpModels = new Model[]
        {
            new Model("prop_gas_pump_1a"),
            new Model("prop_gas_pump_1b"),
            new Model("prop_gas_pump_1c"),
            new Model("prop_gas_pump_old2"),
            new Model("prop_gas_pump_old3"),
            new Model("prop_vintage_pump")
        };

        public static bool IsVehicleNearGasStation(Vehicle vehicle)
        {
            if (gasStations != null)
            {
                for (int index = 0; index < gasStations.Length; ++index)
                {
                    if (Game.Player.Character.Position.DistanceTo2D(gasStations[index]) <= gasStationRadius)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsVehicleNearAnyPump(Vehicle vehicle)
        {
            // Vector3 fuelTankPos = GetVehicleTankPos(vehicle);

            if (gasStations != null)
            {
                for (int index = 0; index < gasStations.Length; ++index)
                {
                    if (Game.Player.Character.Position.DistanceTo2D(gasStations[index]) <= gasStationRadius)
                    {
                        // Get closest pump:
                        Prop[] nearbyProps = World.GetNearbyProps(Game.Player.Character.Position, fuelPumpRadius, GasPumpModels);

                        if (nearbyProps.Length > 0)
                        {
                            if (SettingsManager.fuel_debugEnabled)
                            {
                                N.ShowSubtitle($"Found ~b~{nearbyProps.Length}~s~ fuel pump(s) nearby", 1500);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        // JERRY CAN: 
        #region
        public static void RefillWithJerryCan(Vehicle vehicle)
        {
            if (vehicle == null) return;

            string license = vehicle.Mods.LicensePlate;
            string animDict = "weapon@w_sp_jerrycan";
            string animName = "fire_intro";
            
            if (Game.Player.Character.Position.DistanceTo(vehicle.Position) <= 3f && InteractionHandler.IsPlayerFacingVehicle(vehicle))
            {
                if (Fuel.CompareLicense(license))
                {
                    if (Game.Player.Character.Weapons.Current.LocalizedName == "Jerry Can")
                    {
                        Weapon jerryCan = Game.Player.Character.Weapons.Current;

                        // Player can start to refuel.
                        if (Game.IsControlPressed(Control.Aim))
                        {
                            // Check Jerry Can has enough ammo 
                            if (jerryCan.Ammo > 0 || jerryCan.AmmoInClip > 0)
                            {
                                Fuel.RenderFuelBar(Fuel.CurrentFuel, Fuel.MaxFuel, false);

                                if (Fuel.CurrentFuel < Fuel.MaxFuel)
                                {
                                    if (Game.IsControlPressed(Control.Context))
                                    {
                                        Game.Player.Character.Task.PlayAnimation(animDict, animName, 0f,0f, 1500, (AnimationFlags)49, 1);
                                        Refuel(vehicle);
                                        --jerryCan.Ammo;
                                    }
                                    else
                                    {
                                        // Set Initial Fuel if the player is not holding buttons and has Jerry Can active
                                        initialFuel = Fuel.fuelRegistry[license].currentFuel;
                                    }
                                }
                            }
                            else
                            {
                                N.ShowHelpText("Jerry can is empty");
                            }
                        }
                        N.ShowHelpText("Hold ~INPUT_AIM~ and ~INPUT_CONTEXT~ to refuel your vehicle with the Jerry Can");
                    }
                }
            }
        }
        #endregion

        #region REFUELLING:
        public static void Refuel(Vehicle vehicle)
        {
            string license = vehicle.Mods.LicensePlate;
            if (Fuel.CompareLicense(license))
            {
                // Fuel increment in milliliters
                float fillAmount = 20f * SettingsManager.refuelSpeedMultiplier;
                float currentFuel = Fuel.GetVehicleFuelLevels(license).currentFuel;
                float maxFuel = Fuel.GetVehicleFuelLevels(license).maxFuel;

                // Calculate fuel filled during this session
                fuelRefilled = currentFuel - initialFuel;
                // Increment the currentFuel by the fillAmount per tick
                if (currentFuel < maxFuel)
                {
                    Fuel.UpdateVehicleFuel(license, -fillAmount);
                }

                // Fuel.CurrentFuel += fillAmount;
                // Fuel.CurrentFuel = AIS.Clamp(Fuel.CurrentFuel, 0f, Fuel.MaxFuel); // Clamp to prevent breaking the ceiling 
            }
        }
        public static void FinishedRefueling()
        {
            refillCost = CalculateRefillCost(ref fuelRefilled);

            if (Game.Player.Money < refillCost)
            {
                Game.Player.WantedLevel += 1;
                N.DisplayNotification($"~r~You were reported stealing gas~s~", false);
            }
            DeductMoney(refillCost);
            fuelRefilled = 0f;
        }
        public static void DeductMoney(int amount)
        {
            int money = Game.Player.Money - amount;
            float fuel = fuelRefilled / 1000;


            if (Game.Player.Money >= amount)
            {
                Game.Player.Money = money;

                N.DisplayNotification($"Refilled ~y~{fuel}~s~ liters for ~g~${amount}~s~", false);
            }
        }
        public static int CalculateRefillCost(ref float fuelRefilled)
        {
            float litersRefueled = fuelRefilled / 1000f;
            refillCost = (int)(litersRefueled * fuelPrice);

            return refillCost;
        }
        #endregion

    }
}
