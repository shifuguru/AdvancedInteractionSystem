using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    internal class Indicators : Script
    {
        private int refTime = Game.GameTime;
        private int interval = 3000;
        public Indicators()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!SettingsManager.modEnabled || !SettingsManager.indicatorsEnabled) return;
            
            Vehicle vehicle = InteractionManager.currentVehicle;

            if (vehicle != null && vehicle.Exists())
            {
                float currentSpeed = InteractionManager.currentVehicle.Speed;
                float steeringAngle = (float)Math.Round(InteractionManager.currentVehicle.SteeringAngle, 0);
                Vector3 pos = InteractionManager.currentVehicle.Position;
                if (Function.Call<bool>(Hash.IS_POINT_ON_ROAD, pos.X, pos.Y, pos.Z, InteractionManager.currentVehicle) && steeringAngle != 0 && currentSpeed > -1.0 && currentSpeed < 10)
                {
                    refTime = Game.GameTime;

                    if (steeringAngle > 0.1f) // Left
                    {
                        vehicle.IsLeftIndicatorLightOn = true;
                        vehicle.IsRightIndicatorLightOn = false;
                    }
                    else if (steeringAngle < -0.1f) // Right
                    {
                        vehicle.IsLeftIndicatorLightOn = false;
                        vehicle.IsRightIndicatorLightOn = true;
                    }
                    else
                    {
                        vehicle.IsLeftIndicatorLightOn = false;
                        vehicle.IsRightIndicatorLightOn = false;
                    }
                }
                if (Game.GameTime <= refTime + interval) return;

                vehicle.IsLeftIndicatorLightOn = false;
                vehicle.IsRightIndicatorLightOn = false;
            }
            else
            {
                Vehicle lastVehicle = Game.Player.LastVehicle;
                if (lastVehicle != null && lastVehicle.Exists())
                {
                    Vector3 pos = lastVehicle.Position;
                    if (Function.Call<bool>(Hash.IS_POINT_ON_ROAD, pos.X, pos.Y, pos.Z, lastVehicle))
                    {
                        lastVehicle.IsRightIndicatorLightOn = true;
                        lastVehicle.IsLeftIndicatorLightOn = true;
                    }
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
