using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using Control = GTA.Control;
using GTA.Native;
using GTA.Math;
using GSH = AdvancedInteractionSystem.RefuelHelper;

namespace AdvancedInteractionSystem
{
    public class Fuel : Script
    {
        public static Keys refuelKey = SettingsManager.refuelKey;

        public static Dictionary<string, (float currentFuel, float maxFuel)> fuelRegistry = new Dictionary<string, (float currentFuel, float maxFuel)>();
        public static Queue<string> licenseQueue = new Queue<string>();

        #region Fuel: 
        public static float CurrentFuel = 0f; // Game.Player.Character.CurrentVehicle.FuelLevel
        public static float MaxFuel = 65000f; // Max Volume of Fuel Tank
        public static float LowFuel = MaxFuel * 0.25f; // 25% fuel: Low Fuel warning. 
        public static float MinFuel = MaxFuel * 0.10f; // 10% fuel: Reserve Tank warning.
        public static float BaseConsumption = 0f;
        #endregion

        #region Vehicle: 
        public static Vector3 initialPosition;
        public static float tripOdometer = 0f;
        public static Vector3 currentPosition;
        public static Vector3 lastPosition;
        #endregion

        #region UI: Fuel Bar
        // FuelBar Height x Width: 
        public static float fuelBarHeight = 6f;
        public static float fuelBarWidth = GetBarWidth();
        // FuelBar Position: 
        public static PointF basePosition = new PointF(0.0f, 715f);
        public static PointF fuelBarBackdropPosition = basePosition;
        public static PointF fuelBarBackPosition = new PointF(fuelBarBackdropPosition.X, fuelBarBackdropPosition.Y + 3f);
        public static PointF fuelBarPosition = fuelBarBackPosition;
        // FuelBar Sizes: 
        public static SizeF fuelBarBackdropSize = new SizeF(fuelBarWidth, 12f);
        public static SizeF fuelBarBackSize = new SizeF(fuelBarWidth, fuelBarHeight);
        public static SizeF fuelBarSize = fuelBarBackSize;
        public static Color fuelBarBackdropColor = Color.FromArgb(100, 0, 0, 0);
        public static Color fuelBarBackColor = Color.FromArgb(50, byte.MaxValue, 179, 0);
        public static Color fuelBarColorNormal = Color.FromArgb(150, byte.MaxValue, 179, 0);
        public static Color fuelBarColorWarning = Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 0);
        public static Color fuelBarElectricColorNormal = Color.FromArgb(byte.MaxValue, 12, 110, 201);
        public static Color fuelBarElectricColorWarning = Color.FromArgb(byte.MaxValue, 231, 187, 237);
        public static Rectangle fuelBarBackdrop = new Rectangle(fuelBarBackdropPosition, fuelBarBackdropSize, fuelBarBackdropColor);
        public static Rectangle fuelBarBack = new Rectangle(fuelBarBackPosition, fuelBarBackSize, fuelBarBackColor);
        public static Rectangle fuelBar = new Rectangle(fuelBarPosition, fuelBarSize, fuelBarColorNormal);
        public static Sprite fuelIcon;
        #endregion

        #region UI: Warnings
        public static DateTime lastWarningTime;
        public static TimeSpan warningDuration = TimeSpan.FromMilliseconds(3000);
        public static bool shouldShowWarning = false;
        public static bool hasShownWarning = false;
        #endregion

        #region Fuel Weapon
        public static Prop jerryProp;
        public static Weapon jerryWeapon;
        public static bool jerryEquipped;
        public static bool jerryAmount;
        public static bool jerryQuick; // Early phase testing: Quickly use Jerry can
        #endregion

        public Fuel()
        {
            Tick += OnTick;
            Aborted += OnAborted;
            Interval = 1;
        }

        #region FUEL BAR: 
        public static void UpdateFuelBarColor()
        {
            if (CurrentFuel < LowFuel)
            {
                fuelBar.Color = fuelBarColorWarning;
            }
            else
            {
                fuelBar.Color = fuelBarColorNormal;
            }
        }
        public static float GetBarWidth()
        {
            double aspectRatio = N.GetAspectRatio(false);

            if (aspectRatio <= 1.3333333730697632)
            {
                if (aspectRatio == 1.25)
                    return byte.MaxValue;
                if (aspectRatio == 1.3333333730697632)
                    return 240f;
            }
            else
            {
                if (aspectRatio == 1.5)
                    return 212f;
                if (aspectRatio == 1.6000000238418579)
                    return 200f;
                if (aspectRatio == 1.6666666269302368)
                    return 191f;
            }
            return 180f;
        }
        public static PointF Position
        {
            set
            {
                fuelBarBackdrop.Position = value;
                fuelBarBack.Position = new PointF(value.X, value.Y + 3f);
                fuelBar.Position = fuelBarBack.Position;
            }
        }
        public static PointF GetSafezoneBounds()
        {
            float safeZoneSize = N.GetSafeZoneSize();
            float num1 = 1280f;
            float num2 = 720f;
            return new PointF((int)Math.Round((num1 - num1 * safeZoneSize) / 2.0 + 1.0), (int)Math.Round((num2 - num2 * safeZoneSize) / 2.0 - 2.0));
        }
        public static void RenderFuelBar(float currentFuel, float maxFuel, bool isElectric)
        {
            PointF safezoneBounds = GetSafezoneBounds();
            Position = new PointF(basePosition.X + safezoneBounds.X, basePosition.Y - safezoneBounds.Y);

            float fuel = (currentFuel / maxFuel) * 100f;
            fuelBar.SizeF = new SizeF(fuelBarWidth / 100f * fuel, fuelBarHeight);
            fuelBar.Color = fuelBarColorNormal;

            fuelBarBackdrop.Draw();
            fuelBarBack.Draw();
            fuelBar.Draw();
        }
        #endregion

        #region FUEL NOTIFICATIONS: 
        public static void UpdateFuelWarning()
        {
            // Distance to gas station: 
            float distance = World.GetDistance(Game.Player.Character.Position, GSH.closestGasStation);

            if (CurrentFuel > LowFuel || distance < 25f)
            {
                AIS.StopFlashingAllBlips(GSH.fuelBlips);
                shouldShowWarning = false;
                hasShownWarning = false;
                return;
            }

            // float percentage = (currentFuel / maxFuel) * 100f;
            string message = GenerateWarningMessage();
            UpdateWaypointAlert();

            shouldShowWarning = (DateTime.Now - lastWarningTime) >= warningDuration;

            // Display Fuel Warnings < 25%: 
            if (Game.Player.Character.LastVehicle.IsEngineRunning && CurrentFuel <= LowFuel)
            {
                if (!hasShownWarning && shouldShowWarning)
                {
                    // DIA_DRIVER
                    // texture = Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, "DIA_DRIVER", false);
                    NotificationIcon icon = NotificationIcon.Carsite;
                    string sender = "Fuel Warning:";
                    string subject = "";
                    N.DisplayNotificationSMS(icon, sender, subject, message, false, false);
                    DisplayFuelWarning(message, distance);
                }
            }
        }
        private static void DisplayFuelWarning(string message, float distance)
        {
            if (DateTime.Now - lastWarningTime < warningDuration)
            {
                // for icon I would prefer to use DIA_DRIVER but this requires texture streaming.. may revise later.. 
                N.DisplayNotificationSMS(NotificationIcon.Carsite, "Fuel Warning: ", "", message, false, false);
            }
        }
        private static string GenerateWarningMessage()
        {
            double percentage = Math.Round(CurrentFuel / MaxFuel * 100, 2, MidpointRounding.ToEven);

            if (CurrentFuel <= 1f)
            {
                lastWarningTime = DateTime.Now;
                fuelBar.Color = fuelBarColorWarning;
                return "~r~Empty~s~";
            }
            else if (CurrentFuel <= LowFuel)
            {
                lastWarningTime = DateTime.Now;
                fuelBar.Color = fuelBarColorWarning;
                return $"~r~{percentage}% remaining. Reserve tank in-use.~s~ ~n~~r~Fill up immediately or risk engine failure!~s~";
            }
            else
            {
                lastWarningTime = DateTime.Now;
                fuelBar.Color = fuelBarColorWarning;
                return $"~o~{percentage}% remaining~s~ ~n~Fill up at the nearest Gas Station";
            }
        }
        // WAYPOINT:
        private static void UpdateWaypointAlert()
        {
            bool waypointActive = Function.Call<bool>(Hash.IS_WAYPOINT_ACTIVE);
            float distanceToWaypoint = World.GetDistance(GSH.closestGasStation, World.WaypointPosition);
            bool isWaypointToClosestGasStation = waypointActive && distanceToWaypoint <= 15f;

            if ((DateTime.Now - lastWarningTime) < warningDuration)
            {
                return;
            }

            if (isWaypointToClosestGasStation)
            {
                AIS.StopFlashingAllBlips(GSH.fuelBlips);
                // lastWarningTime = null;
                return;
            }

            if (GSH.closestBlip != null)
            {
                AIS.FlashBlip(GSH.closestBlip);
            }
            
            N.ShowHelpText("Press ~INPUT_CONTEXT~ for directions to the closest Gas Station");

            if (Game.IsControlJustPressed(Control.Context))
            {
                N.SetWaypoint(GSH.closestGasStation);
            }

            lastWarningTime = DateTime.Now;
        }
        // ENGINE: 
        public static void ManageEngineCeasure()
        {
            // This method is actually no longer required as we can just directly manipulate the vehicle's fuel level. 
            // Once the Fuel Level reaches 0, the vehicle shuts off by the game. 

            if (CurrentFuel <= 1f)
            {
                N.SetVehicleEngineOn(Game.Player.Character.LastVehicle, false, false, true);
            }
        }
        #endregion


        public static void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (!SettingsManager.modEnabled || !SettingsManager.fuelEnabled) return;

                Ped GPC = Game.Player.Character;
                if (GPC == null || !GPC.Exists() || GPC.IsDead) return;

                UpdateFuelForAllVehicles();
                
                // If Current Vehicle doesn't exist, does the Last Vehicle?
                Vehicle currentVehicle = InteractionManager.currentVehicle;

                if (currentVehicle != null && currentVehicle.Exists())
                {
                    RenderFuelBar(CurrentFuel, MaxFuel, false);
                    GSH.GetClosestGasStation();
                    GSH.PumpLogic(currentVehicle);
                }
                else currentVehicle = Game.Player.Character.LastVehicle;

                if (currentVehicle == null || !currentVehicle.Exists()
                        || currentVehicle.IsBicycle
                        || currentVehicle.HighGear <= 1
                        || currentVehicle.IsBoat
                        || currentVehicle.IsAircraft)
                    return;

                InitialiseFuelRegistry(currentVehicle);
                UpdateFuelForCurrentVehicle(currentVehicle);

                UpdateFuelWarning();
                UpdateFuelBarColor();
                // ManageEngineCeasure();
                
                GSH.RefillWithJerryCan(InteractionManager.closestVehicle);
            }
            catch (Exception ex)
            {
                AIS.LogException("Fuel.OnTick", ex);
            }
        }

        // FUEL REGISTRY:
        #region
        public static void InitialiseFuelRegistry(Vehicle vehicle)
        {
            string license = vehicle.Mods.LicensePlate;

            if (!string.IsNullOrEmpty(license) && !CompareLicense(license))
            {
                float maxFuel = vehicle.HandlingData.PetrolTankVolume * 1000;
                float initialFuel = N.GetRandomFloatInRange(MinFuel, maxFuel);
                AddVehicleToRegistry(license, initialFuel, maxFuel);
            }
        }

        public static void AddVehicleToRegistry(string license, float initialFuel, float maxFuel)
        {
            if (!CompareLicense(license))
            {
                fuelRegistry[license] = (initialFuel, maxFuel);
                licenseQueue.Enqueue(license);

                if (fuelRegistry.Count > 32)
                {
                    RemoveOldestCar();
                }
            }
        }

        public static void UpdateVehicleFuel(string license, float fuelConsumption)
        {
            // Updates the Fuel Level in the Database, 
            // This allows persistent Fuel Levels if the car despawns.
            if (CompareLicense(license))
            {
                var (currentFuel, maxFuel) = fuelRegistry[license];
                currentFuel = Math.Max(currentFuel - fuelConsumption, 0);
                fuelRegistry[license] = (currentFuel, maxFuel);

            }
        }

        public static (float currentFuel, float maxFuel) GetVehicleFuelLevels(string license)
        {
            if (fuelRegistry.TryGetValue(license, out var vehicleFuelLevels))
            {
                return vehicleFuelLevels;
            }
            return (0, 60);
        }

        public static void UpdateFuelForCurrentVehicle(Vehicle vehicle)
        {
            try
            {
                // get the current vehicle license
                string license = vehicle.Mods.LicensePlate;
                if (license != null)
                {
                    CurrentFuel = GetVehicleFuelLevels(license).currentFuel;
                    MaxFuel = GetVehicleFuelLevels(license).maxFuel;
                    vehicle.FuelLevel = GetVehicleFuelLevels(license).currentFuel;
                }
                else
                {
                    if (SettingsManager.fuel_debugEnabled)
                    {
                        N.ShowSubtitle("No License Plate found!", 200);
                    }
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("Fuel.GetFuelForCurrentVehicle", ex);
            }
        }
        public static void UpdateFuelForAllVehicles()
        {
            var licensesToRemove = new List<string>();

            foreach (var license in fuelRegistry.Keys.ToList())
            {
                Vehicle vehicle = GetVehicleByLicense(license);

                if (vehicle != null && vehicle.Exists())
                {
                    float consumptionRate = CalculateFuelConsumptionRate(vehicle);
                    UpdateVehicleFuel(license, consumptionRate * Game.LastFrameTime);
                }
                else
                {
                    licensesToRemove.Add(license);
                }
            }

            foreach (var license in licensesToRemove)
            {
                fuelRegistry.Remove(license);
            }
        }

        #endregion

        public static void RemoveOldestCar()
        {
            if (licenseQueue.Count > 0)
            {
                string oldestLicense = licenseQueue.Dequeue();
                fuelRegistry.Remove(oldestLicense);
            }
        }
        
        public static bool CompareLicense(string license)
        {
            return fuelRegistry.ContainsKey(license);
        }




        #region FUEL SYSTEM: 
        public static float GetBaseConsumption(Vehicle vehicle)
        {
            if (vehicle == null || vehicle.HandlingData == null) return 0f;
            float mass = vehicle.HandlingData.Mass;
            float driveForce = vehicle.HandlingData.InitialDriveForce;
            float weatherCondition = AIS.GetWeatherConditionRate(World.Weather.ToString());

            float baseConsumption = mass * driveForce * weatherCondition;
            
            return baseConsumption;
        }
        public static float CalculateFuelConsumptionRate(Vehicle vehicle)
        {
            if (vehicle == null) return 0f;

            // Determine engine state (running or not)
            int engineState = vehicle.IsEngineRunning ? 1 : 0;

            float baseConsumption = GetBaseConsumption(vehicle);
            float rpm = vehicle.CurrentRPM * 10;
            float engineHealth = vehicle.EngineHealth;

            // Throttle = Throttle Position (0 - 1). 
            float throttle = Math.Max(0, vehicle.Throttle);
            float acceleration = Math.Max(0, vehicle.Acceleration);
            float speedKph = Math.Abs(vehicle.Speed * 3.6f);

            // Engine Temperature / efficiency
            float engineTemp = (float)Math.Round(vehicle.EngineTemperature, 1);
            float tempFactor = engineTemp < 40 ? 1.5f : 1.0f; // Adjust factor based on engine temperature.

            // BASE CONSUMPTION.

            // bool isGrounded = vehicle.IsOnAllWheels;
            // string wheelType = vehicle.Mods.WheelType.ToString();
            // float passengers = vehicle.PassengerCount;

            // RoadFrictionMultiplier = Factor representing the road conditions and tire traction. 
            // EnvironmentalMultiplier = Factor representing environmental conditions suchas temperature and humidity.
            // BehaviourMultiplier = Factor representing driver behaviour (agressive driving, frequent braking etc.). 
            // VehicleSpecsMultiplier = Factor representing vehicle specification (weight, aerodynamics, fuel system condition).

            // float temp = vehicle.EngineTemperature;
            // float fuel = vehicle.FuelLevel;
            // int gears = vehicle.HighGear;
            // int gear = vehicle.CurrentGear;
            // float gearRatio = gear / gears;
            // float burnoutMultiplier = vehicle.HandlingData.LowSpeedTractionLossMultiplier;
            // float tankVolume = vehicle.HandlingData.PetrolTankVolume;
            // float tractionMin = vehicle.HandlingData.TractionCurveMin;
            // float tractionMax = vehicle.HandlingData.TractionCurveMax;

            // FUEL CONSUMPTION RATE FORMULA: 
            float consumptionRate =
                engineState // 0 or 1
                * baseConsumption // 700 
                * rpm // 0.2
                * (acceleration + rpm) 
                * (speedKph / 100f + rpm)
                * tempFactor
                / engineHealth;

            if (speedKph < 1)
            {
                consumptionRate *= 0.1f;
            }
            // float km/L = totalDistancedTravelled / (currentFuel / 1000f);
            float liters = (CurrentFuel / 1000f);

            // float fuelConsumptionRate = baseConsumption * (throttle * acceleration * rpm / engHealth) * (speed + minSpeed) * roadFrictionMultiplier * environmentalMultiplier * behaviouralMultiplier * vehicleSpecsMultiplier;

            string textColor = consumptionRate >= 1f ? "~r~" : "~w~";

            if (SettingsManager.fuel_debugEnabled)
            {
                N.ShowSubtitle(
                $"Fuel = ~y~{liters:F2} Liters~s~ " 
                + $"Rate: ~p~{consumptionRate:F3}~s~ "
                + $"RPM: ~r~{rpm * 1000:F0}~s~ "
                + $"~n~Speed: ~y~{speedKph}~s~ Health: ~g~{engineHealth}~s~ "
                + $"~n~Temp: ~o~{engineTemp}C~s~ Weather: ~o~{World.Weather.ToString()}~s~ "
                , 500);
            }

            /*
            // 1. FRICTION. 
            // 2. DRAG. 
            // 3. WEIGHT
            // 4. MOMENTUM. 
            // EFFICIENCY ~?~
            // RESISTENCE

            // 1/ ROAD CONDITIONS:
            // Surface type 
            // Wetness level
            // Tire Traction

            // 2/ DRIVING BEHAVIOUR: 
            // Agressive Acceleration
            // Frequent Braking 
            // Idling time 

            // 3/ VEHICLE SPECIFICS: 
            // Fuel efficiency rating (miles per gallon/liters per kilometer)
            // Engine type (gasoline, diesel, electric)
            // Vehicle Weight and aerodynamics 
            // Tire pressure 

            // 5/ MAINTENANCE CONDITION: 
            // Fuel system condition (tank damage)
            // Distance travelled 

            // FORMULA:
            // Fuel Consumption = Base Consumption * (Throttle * Acceleration * RPM / Engine Health) * Speed * RoadFrictionMultiplier * EnvironmentalMultiplier * BehaviourMultiplier * VehicleSpecsMultiplier
            // BaseConsumption = Base fuel consumption rate of the vehicle under ideal conditions. 
            // >>> Base Consumption Rate = Engine Power × Vehicle Weight × Driving Conditions Factor 

            // Lamborghini / Super cars should see about
            // 18 miles per gallon
            // 13.0675 L / 100 Km 

            // Measure Distance Traveled:
            // You can keep track of the distance traveled by the vehicle using its position.
            // You'll need to store the initial position when the vehicle starts moving and then calculate the distance between subsequent positions.

            // Measure Fuel Consumed:
            // You already have the logic to calculate fuel consumption rate(fuelConsumptionRate) in your CalculateFuelConsumptionRate method.

            // Calculate L/KM:
            // Once you have the distance traveled and the fuel consumed, you can use the appropriate units to calculate Liters per Kilometer.
            */
                
            return consumptionRate;
        }
        public static Vehicle GetVehicleByLicense(string license)
        {
            foreach (Vehicle vehicle in World.GetAllVehicles())
            {
                if (vehicle.Mods.LicensePlate == license)
                {
                    return vehicle;
                }
            }
            return null;
        }
        #endregion



        // TRIP METER:
        public static void UpdateTripReading(Vehicle vehicle)
        {
            if (vehicle == null) return;
            currentPosition = vehicle.Position;
        }
        public static void ResetTripReading(Vehicle vehicle)
        {
            lastPosition = currentPosition;
        }



        public static void OnAborted(object sender, EventArgs e)
        {
            AIS.DeleteAllBlips(GSH.fuelBlips);
            fuelRegistry.Clear();
            licenseQueue.Clear();
        }
    }
}
