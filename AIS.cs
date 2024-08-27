using System;
using System.IO;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Linq;

namespace AdvancedInteractionSystem
{
    public class AIS : Script
    {
        public static string log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AIS", "AIS.log");
        // Settings: 
        public static string modName = "Advanced Interaction System";
        public static bool modEnabled = SettingsManager.modEnabled;

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // WEATHER: 
        public static float GetWeatherConditionRate(string weather)
        {
            float weatherCondition;

            switch (weather)
            {
                case "Clear":
                    weatherCondition = 1.0f;
                    break;
                case "ExtraSunny":
                    weatherCondition = 0.9f;
                    break;
                case "Clouds":
                    weatherCondition = 1.1f;
                    break;
                case "Smog":
                    weatherCondition = 1.2f;
                    break;
                case "Foggy":
                    weatherCondition = 1.2f;
                    break;
                case "Overcast":
                    weatherCondition = 1.1f;
                    break;
                case "Raining":
                    weatherCondition = 1.3f;
                    break;
                case "ThunderStorm":
                    weatherCondition = 1.4f;
                    break;
                case "Clearing":
                    weatherCondition = 1.2f;
                    break;
                case "Neutral":
                    weatherCondition = 0.5f;
                    break;
                case "Snowing":
                    weatherCondition = 1.8f;
                    break;
                case "Blizzard":
                    weatherCondition = 2.0f;
                    break;
                case "Snowlight":
                    weatherCondition = 1.5f;
                    break;
                case "Christmas":
                    weatherCondition = 1.5f;
                    break;
                case "Halloween":
                    weatherCondition = 5.0f;
                    break;
                default:
                    weatherCondition = 1.0f;
                    break;
            }
            return weatherCondition;
        }

        public static void CreateBlip(Vector3 position, BlipSprite sprite, float scale, BlipColor color, string blipName, List<Blip> blips)
        {
            if (position == null || blips == null) return;
            Blip blip = World.CreateBlip(position);
            if (blip == null) return;
            blip.Sprite = sprite;
            blip.Scale = scale;
            blip.Color = color;
            blip.Name = blipName;
            blip.IsShortRange = SettingsManager.shortRangeBlips;
            blips.Add(blip);
        }

        public static Vector3 FlashBlip(Blip blip)
        {
            blip.IsShortRange = false;
            blip.IsFlashing = true;
            blip.FlashInterval = 800;
            return blip.Position;
        }

        public static void StopFlashingBlip(Blip blip)
        {
            blip.IsFlashing = false;
            blip.IsShortRange = true;
        }
        public static void StopFlashingAllBlips(List<Blip> blips)
        {
            if (blips == null) return;
            foreach (Blip blip in blips)
            {
                if (blip != null && blip.Exists())
                {
                    blip.IsFlashing = false;
                    blip.IsShortRange = true;
                }
            }
        }
        public static void DeleteAllBlips(List<Blip> blips)
        {
            if (blips == null) return;
            foreach (Blip blip in blips)
            {
                if (blip != null && blip.Exists())
                {
                    blip.Delete();
                }
            }
            blips.Clear();
        }

        public AIS()
        {
            SettingsManager.LoadSettings();
        }

        // EXCEPTION LOGGING: 
        public static void LogException(string methodName, Exception ex)
        {
            try
            {
                string message = $"[{DateTime.Now}] Error in {methodName} method. Exception: {ex.Message}";
                File.AppendAllText(log, $"{message}{Environment.NewLine}");
            }
            catch (Exception ex0)
            {
                Console.WriteLine($"Failed to log exception: {ex0.Message}");
            }
        }
    }
}
