using System;
using System.Collections.Generic;
using GTA;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    public class DaytimeHeadlights : Script
    {
        public static bool lightsEnabled = true;
        public static int intensity = 1;
        public static bool xenonEnabled = false;
        public static bool otherXenonEnabled = false;
        public static bool ForceAllVehicles = false;

        public DaytimeHeadlights()
        {
            // Tick += OnTick;
        }

        public static void OnTick(object sender, EventArgs e)
        {
            try 
            {
                if (!SettingsManager.modEnabled || !lightsEnabled)
                {
                    // RESET LIGHTS: 
                    Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.LastVehicle, 0);

                    if (Game.Player.Character.CurrentVehicle != null)
                    {
                        Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.Character.CurrentVehicle, 0);
                    }
                    /*
                    set's if the vehicle has lights or not.
                    not an on off toggle.
                    p1 = 0 ;vehicle normal lights, off then lowbeams, then highbeams
                    p1 = 1 ;vehicle doesn't have lights, always off
                    p1 = 2 ;vehicle has always on lights
                    p1 = 3 ;or even larger like 4,5,... normal lights like =1
                    note1: when using =2 on day it's lowbeam,highbeam
                    but at night it's lowbeam,lowbeam,highbeam
                    note2: when using =0 it's affected by day or night for highbeams don't exist in daytime.
                     */
                    return;
                }
                if (ForceAllVehicles)
                {
                    foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(Game.Player.Character, 100f))
                    {
                        if (Function.Call<bool>(Hash.GET_IS_VEHICLE_ENGINE_RUNNING, nearbyVehicle))
                        {
                            if (otherXenonEnabled)
                            {
                                Function.Call(Hash.TOGGLE_VEHICLE_MOD, nearbyVehicle, 22, true);
                            }

                            Function.Call(Hash.SET_VEHICLE_LIGHTS, nearbyVehicle, 3);
                            float lightMultiplier = intensity * 0.2f;
                            Function.Call(Hash.SET_VEHICLE_LIGHT_MULTIPLIER, nearbyVehicle, lightMultiplier);
                        }
                    }
                }

                int playerVehicleLightState = 0;

                if (Function.Call<bool>(Hash.GET_IS_VEHICLE_ENGINE_RUNNING, Game.Player.Character.LastVehicle))
                {
                    playerVehicleLightState = 2;
                }

                Function.Call(Hash.SET_VEHICLE_LIGHTS, Game.Player.LastVehicle, playerVehicleLightState);
            } 
            catch (Exception ex) 
            {
                AIS.LogException("DaytimeHeadlights.OnTick()", ex);
            }
        }
    }
}
