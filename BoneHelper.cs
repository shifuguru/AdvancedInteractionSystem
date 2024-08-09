using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;
using System.Diagnostics;

namespace AdvancedInteractionSystem
{
    public class BoneHelper : Script
    {
        public static bool debugEnabled = SettingsManager.debugEnabled;
        public static List<Tuple<string, Vector3>> GetVehicleBones(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null) 
                    return null;
                if (vehicle != null && vehicle.Exists())
                {
                    return new List<Tuple<string, Vector3>>
                    {
                        new Tuple<string, Vector3>("door_dside_f", vehicle.Bones["door_dside_f"].Position),
                        new Tuple<string, Vector3>("door_pside_f", vehicle.Bones["door_pside_f"].Position),
                        new Tuple<string, Vector3>("door_dside_r", vehicle.Bones["door_dside_r"].Position),
                        new Tuple<string, Vector3>("door_pside_r", vehicle.Bones["door_pside_r"].Position),
                        new Tuple<string, Vector3>("boot", vehicle.Bones["boot"].Position),
                        new Tuple<string, Vector3>("bonnet", vehicle.Bones["bonnet"].Position),
                        new Tuple<string, Vector3>("petroltank", vehicle.Bones["petroltank"].Position),
                    };
                }
                else 
                    return null;
            }
            catch (Exception ex)
            {
                AIS.LogException("GetVehicleBones", ex);
                return null;
            }
            
        }

        public static Tuple<string, Vector3> GetClosestBone(Vehicle vehicle, Vector3 position)
        {
            try
            {
                if (vehicle == null || position == null)
                    return null;
                if (vehicle != null && vehicle.Exists())
                {
                    var bones = GetVehicleBones(vehicle);
                    Tuple<string, Vector3> closestBone = null;
                    float closestBoneDistance = float.MaxValue;

                    foreach (var bone in bones)
                    {
                        float distance = Vector3.Distance(Game.Player.Character.Position, bone.Item2);
                        if (distance < closestBoneDistance)
                        {
                            closestBone = bone;
                            closestBoneDistance = distance;
                        }
                    }
                    return closestBone;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                AIS.LogException("BoneHelper.GetClosestBone", ex);
                return null;
            }
        }
    }
}
