using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.UI;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;
using GTA.Native;
using GTA.Math;
using System.Linq;
using GTA.NaturalMotion;
using System.Windows.Forms;

namespace AdvancedInteractionSystem
{
    public class Fuel : Script
    {
        public static bool modEnabled = SettingsManager.modEnabled;
        public static bool fuel_debugEnabled = SettingsManager.fuel_debugEnabled;
        public static bool fuelEnabled = SettingsManager.fuelEnabled;
        public static Keys refuelKey = SettingsManager.refuelKey;
        public static bool isRefueling = false;
        public static Vehicle currentVehicle = InteractionManager.currentVehicle;
        public static Vehicle lastVehicle = null;
        public static Ped GPC = Game.Player.Character;

        public static Vector3[] gasStations = new Vector3[24]
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
        };
        public static Vector3 closestGasStation = Vector3.Zero; // Furthest gas station
        public static float closestDistance = float.MaxValue;
        public static float fuelPumpRadius = 10f;
        public static Blip fuelBlip;
        public static Blip closestBlip;
        public static bool blipsAreFlashing = false;
        // public static Blip currentFlashingBlip = null;
        public static List<Blip> fuelBlips = new List<Blip>();
        
        public static Dictionary<string, float> carsList = new Dictionary<string, float>();
        public static Queue<string> licenseQueue = new Queue<string>();


        #region Vehicle: 
        public static bool isInVehicle = false;
        public static bool isEngineRunning = false;
        public static bool isEngineDestroyed = false;
        public static Vector3 initialPosition;
        public static float tripOdometer = 0f;
        public static Vector3 currentPosition;
        public static Vector3 lastPosition;
        #endregion

        #region Fuel:
        public static float fuelPrice = 3.5f;
        public static float fuelRefilled = 0f;
        public static int refillCost = 0;
        public static float initialFuel = 0f;
        public static float refilledFuel = 0f;
        // public static int refuelCost = 0;
        public static float maxFuel = 65000f; // Max Volume of Fuel Tank
        public static float lowFuel = maxFuel * 0.25f; // 25% fuel: Low Fuel warning. 
        public static float minFuel = maxFuel * 0.10f; // 10% fuel: Reserve Tank warning.
        public static float currentFuel = 0f;
        public static float baseConsumption = 0f;
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
        public static DateTime? lastWarningTime;
        public static TimeSpan warningDuration = TimeSpan.FromMilliseconds(3000);
        //public static bool shouldWarn = false;
        #endregion
        
        
        public Fuel()
        {
            // string textureDict, string textureName, SizeF size, PointF position
            CreateGasStations();
            Tick += OnTick;
            Aborted += OnAborted;
            Interval = 1;
        }


        private static void DisplayFuelWarning(string message, float distance)
        {
            if (DateTime.Now - lastWarningTime < warningDuration)
            {
                // for icon I would prefer to use DIA_DRIVER but this requires texture streaming.. may revise later.. 
                // N.DisplayNotificationSMS(NotificationIcon.Carsite, "Fuel Warning: ", "", message, false, false);
            }
        }

        // needs revision, occurring per tick 
        private static string GenerateWarningMessage()
        {
            double percentage = Math.Round((currentFuel / maxFuel * 100), 2, MidpointRounding.ToEven);

            if (currentFuel <= 1f)
            {
                lastWarningTime = DateTime.Now;
                fuelBar.Color = fuelBarColorWarning;
                return "~r~Empty~s~";
            }
            else if (currentFuel <= minFuel)
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

        private static void UpdateWaypointAlert()
        {
            bool waypointActive = Function.Call<bool>(Hash.IS_WAYPOINT_ACTIVE);
            float distanceToWaypoint = World.GetDistance(closestGasStation, World.WaypointPosition);
            bool isWaypointToClosestGasStation = waypointActive && distanceToWaypoint <= 15f;

            if ((DateTime.Now - lastWarningTime) < warningDuration)
            {
                return;
            }

            if (isWaypointToClosestGasStation)
            {
                AIS.StopFlashingAllBlips(fuelBlips);
                // lastWarningTime = null;
                return;
            }

            if (closestBlip != null)
            {
                AIS.FlashBlip(closestBlip);
            }
            
            N.ShowHelpText("Press ~INPUT_CONTEXT~ for directions to the closest Gas Station");

            if (Game.IsControlJustPressed(Control.Context))
            {
                N.SetWaypoint(closestGasStation);
            }

            lastWarningTime = DateTime.Now;
        }

        public static void ManageEngineCeasure()
        {
            if (currentFuel <= 1f)
            {
                N.SetVehicleEngineOn(currentVehicle, false, false, true);
            }
        }

        public static void UpdateFuelBarColor()
        {
            if (currentFuel < lowFuel)
            {
                fuelBar.Color = fuelBarColorWarning;
            }
            else
            {
                fuelBar.Color = fuelBarColorNormal;
            }
        }

        public static void UpdateFuelWarning()
        {
            // Distance to gas station: 
            float distance = World.GetDistance(GPC.Position, closestGasStation);
            
            if (currentFuel > lowFuel || distance < 25f)
            {
                AIS.StopFlashingAllBlips(fuelBlips);
                return;
            }

            // float percentage = (currentFuel / maxFuel) * 100f;
            string message = GenerateWarningMessage();
            
            UpdateWaypointAlert();

            // Display Fuel Warnings < 25%: 
            if (currentVehicle.IsEngineRunning && currentFuel <= lowFuel)
            {
                // lastWarningTime = DateTime.Now;
            }

            bool shouldShowWarning = (DateTime.Now - lastWarningTime) >= warningDuration;

            if (shouldShowWarning)
            {
                // DIA_DRIVER
                // texture = Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, "DIA_DRIVER", false);
                NotificationIcon icon = NotificationIcon.Carsite;
                string sender = "Fuel Warning:";
                string subject = "";
                N.DisplayNotificationSMS(icon, sender, subject, message, false, false);
                DisplayFuelWarning(message, distance);
                lastWarningTime = DateTime.Now;
            }
        }

        public static void OnTick(object sender, EventArgs e)
        {
            try
            {
                modEnabled = SettingsManager.modEnabled;
                fuelEnabled = SettingsManager.fuelEnabled;
                fuel_debugEnabled = SettingsManager.fuel_debugEnabled;

                if (!modEnabled || !fuelEnabled) return;

                GPC = Game.Player.Character;
                if (GPC == null || !GPC.Exists() || GPC.IsDead) return;

                isInVehicle = GPC.IsInVehicle();

                currentVehicle = Game.Player.Character.CurrentVehicle;
                if (currentVehicle == null 
                    || !currentVehicle.Exists() 
                    || currentVehicle.IsBicycle 
                    || currentVehicle.HighGear <= 1 
                    || currentVehicle.IsBoat 
                    || currentVehicle.IsAircraft) 
                    return;

                GetClosestGasStation();

                UpdateFuelForCurrentVehicle(currentVehicle);
                
                UpdateFuelBarColor();
                UpdateFuelWarning();

                PumpLogic(currentVehicle);
                ManageEngineCeasure();
                // Get Fuel for all vehicles
            }
            catch (Exception ex)
            {
                AIS.LogException("Fuel.OnTick", ex);
            }
        }

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

        // Fuel Bar: 
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

        // Fuel System: 
        public static void InitializeFuelSystem(Vehicle vehicle)
        {
            if (vehicle == null) return;
            maxFuel = vehicle.HandlingData.PetrolTankVolume * 1000;

            string license = vehicle.Mods.LicensePlate;
            float initialFuel = InitialFuelLevel(license);
            AddCar(license, initialFuel);
            currentFuel = initialFuel;

            if (fuel_debugEnabled)
            {
                N.ShowSubtitle($"Car Added: ~b~{vehicle.LocalizedName}~s~, Plate: {vehicle.Mods.LicensePlate}, Fuel: ~y~{currentFuel}~s~ Liters", 2500);
            }
        }

        public static float InitialFuelLevel(string license)
        {
            if (carsList.ContainsKey(license))
            {
                carsList.TryGetValue(license, out float fuelLevel);
                return fuelLevel;
            }
            else
            {
                return N.GetRandomFloatInRange(minFuel, maxFuel);
            }
        }

        public static float GetBaseConsumption(Vehicle vehicle)
        {
            if (vehicle == null || vehicle.HandlingData == null) return 0f;
            float mass = vehicle.HandlingData.Mass;
            float driveForce = vehicle.HandlingData.InitialDriveForce;
            float weatherCondition = AIS.GetWeatherConditionRate(World.Weather.ToString());

            float baseConsumption = mass * driveForce * weatherCondition;
            
            return baseConsumption;
        }

        // CURRENT FUEL RATE CALCULATION:
        public static float CalculateFuelConsumptionRate(Vehicle vehicle)
        {
            if (vehicle == null) return 0f;

            // Determine engine state (running or not)
            int engineState = vehicle.IsEngineRunning ? 1 : 0;

            float baseConsumption = GetBaseConsumption(vehicle);
            float rpm = vehicle.CurrentRPM;
            float engineHealth = vehicle.EngineHealth;

            // Throttle = Throttle Position (0 - 1). 
            float throttle = vehicle.Throttle;
            float acceleration = Math.Max(0, vehicle.Acceleration);
            float speedKph = Math.Abs(vehicle.Speed * 3.6f);

            // Engine Temperature / efficiency
            float engineTemp = (float)Math.Round(currentVehicle.EngineTemperature, 1);
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
                engineState // 1
                * baseConsumption // 700 
                * rpm // 0.2
                * (acceleration + rpm) 
                * (speedKph / 100f + rpm)
                * tempFactor
                / engineHealth;

            // float lPerKm = (currentFuel / 1000f) / totalDistancedTravelled;
            float liters = (currentFuel / 1000f);

            // float fuelConsumptionRate = baseConsumption * (throttle * acceleration * rpm / engHealth) * (speed + minSpeed) * roadFrictionMultiplier * environmentalMultiplier * behaviouralMultiplier * vehicleSpecsMultiplier;

            string textColor = consumptionRate >= 1f ? "~r~" : "~w~";

            if (fuel_debugEnabled)
            {
                N.ShowSubtitle(
                $"Fuel = ~y~{Math.Round(liters, 4)} Liters~s~ Rate: ~p~{Math.Round(consumptionRate, 3)}~s~ "
                + $"~n~Acceleration: ~r~{Math.Round(acceleration, 2)}~s~ Idle: ~r~{rpm}~s~ "
                + $"~n~Speed: ~y~{speedKph}~s~ Health: ~g~{engineHealth}~s~ "
                + $"~n~Temp: ~o~{engineTemp}C~s~ Factor: ~o~{tempFactor}~s~ "
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

        public static void UpdateFuel(string carName, float newFuel)
        {
            if (carsList.ContainsKey(carName))
            {
                carsList[carName] = newFuel;
            }
        }

        public static void PumpLogic(Vehicle vehicle)
        {
            if (!IsVehicleNearAnyPump(vehicle)) return;

            if (currentFuel >= maxFuel || !vehicle.IsOnAllWheels || !vehicle.IsStopped || vehicle.IsUpsideDown || vehicle.IsDead)
            {
                // Tell the player to right their vehicle? 
                // N.ShowHelpText("");
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
                N.ShowHelpText("Hold ~INPUT_CONTEXT~ to turn Engine off before refueling");
                return;
            }

            float fuelToFill = (maxFuel - currentFuel) / 1000;
            int totalCostToFill = (int)Math.Round(fuelPrice * fuelToFill, 1, MidpointRounding.AwayFromZero);
            string message = $"Hold ~INPUT_VEH_HANDBRAKE~ to refuel ~n~Price per liter: ~g~${fuelPrice}~s~";

            if (Game.IsControlPressed(Control.VehicleHandbrake))
            {
                if (!isRefueling)
                {
                    isRefueling = true;
                    initialFuel = currentFuel;
                }
                if (isRefueling)
                {
                    Refuel();
                    float fuelFilled = (currentFuel - initialFuel) / 1000;
                    int fuelFilledCost = (int)Math.Round(fuelPrice * fuelFilled, 1, MidpointRounding.AwayFromZero);
                    N.ShowHelpText($"{message} ~n~Total: ~g~${fuelFilledCost}~s~");
                }
            }
            if (Game.IsControlJustReleased(Control.VehicleHandbrake))
            {
                if (isRefueling)
                {
                    isRefueling = false;
                    FinishedRefueling();
                }
            }
            if (!isRefueling)
            {
                N.ShowHelpText($"{message} ~n~Total to fill: ~g~${totalCostToFill}~s~.");
            }
        }
        public static void Refuel()
        {
            // While Button is held, increment the Fuel this much in Liters/1000 per tick.
            float fillAmount = 100f;
            // Calculate the amount of fuel that has been filled during this session.
            fuelRefilled = currentFuel - initialFuel;
            // Increment the currentFuel by the fillAmount per tick. 
            currentFuel += fillAmount;
            currentFuel = AIS.Clamp(currentFuel, 0f, maxFuel); // Clamp to prevent breaking the ceiling 
        }
        public static void FinishedRefueling()
        {
            refillCost = CalculateRefillCost(ref fuelRefilled);

            if (Game.Player.Money < refillCost)
            {
                Game.Player.WantedLevel += 1;
                N.DisplayNotification($"~r~You were caught stealing gas~s~ ~n~Police are enroute to your location", false);
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
                
                N.DisplayNotification($"Refilled ~y~{fuel}~s~ liters for ~g~${amount}~s~.", false);
            }
           
        }
        public static int CalculateRefillCost(ref float fuelRefilled)
        {
            float litersRefueled = fuelRefilled / 1000f;
            refillCost = (int)(litersRefueled * fuelPrice);

            return refillCost;
        }
        public static bool IsVehicleNearAnyPump(Vehicle vehicle)
        {
            // Vector3 fuelTankPos = GetVehicleTankPos(vehicle);

            if (gasStations != null)
            {
                for (int index = 0; index < gasStations.Length; ++index)
                {
                    if (Game.Player.Character.Position.DistanceTo2D(gasStations[index]) <= fuelPumpRadius)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

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

        // CARS LIST: 
        public static void AddCar(string license, float initialFuel)
        {
            if (!carsList.ContainsKey(license))
            {
                if (carsList.Count > 32)
                {
                    RemoveOldestCar();
                }

                carsList.Add(license, initialFuel);
                licenseQueue.Enqueue(license);
            }
            
        }
        public static void RemoveCar(string license)
        {
            if (carsList.ContainsKey(license))
            {
                carsList.Remove(license);
                licenseQueue = new Queue<string>(licenseQueue.Where(l => l != license));
            }
        }
        public static void RemoveOldestCar()
        {
            if (licenseQueue.Count > 0)
            {
                string oldestLicense = licenseQueue.Dequeue();
                carsList.Remove(oldestLicense);
            }
        }
        public static bool CompareLicense(string license)
        {
            return carsList.ContainsKey(license);
        }


        public static void UpdateFuelForCurrentVehicle(Vehicle currentVehicle)
        {
            try
            {
                string license = currentVehicle.Mods.LicensePlate;
                if (license != null)
                {
                    if (!CompareLicense(license))
                    {
                        // Re-initialize Fuel System for new vehicle: 
                        InitializeFuelSystem(currentVehicle);
                    }

                    float consumptionRate = CalculateFuelConsumptionRate(currentVehicle);
                    currentFuel = Math.Max(currentFuel - (consumptionRate * Game.LastFrameTime), 0);

                    UpdateFuel(currentVehicle.Mods.LicensePlate, currentFuel);
                    RenderFuelBar(currentFuel, maxFuel, false);
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("Fuel.GetFuelForCurrentVehicle", ex);
            }
        }

        public static void OnAborted(object sender, EventArgs e)
        {
            AIS.DeleteAllBlips(fuelBlips);
            carsList.Clear();
            licenseQueue.Clear();
        }
    }
}
