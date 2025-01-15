using System;
using GTA;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    public class IVStyleExit : Script
    {
        private bool closeDoorOnExit = true;
        private int holdDuration;

        private Ped player = null;
        private Vehicle vehicle = null;
        private bool isPlayerDriving = false;
        private int engineOnTime;
        private int engineOffTime;

        public IVStyleExit()
        {
            Tick += OnTick;
            Interval = 0;
        }

        void OnTick(object sender, EventArgs e)
        {
            player = Function.Call<Ped>(Hash.PLAYER_PED_ID);
            vehicle = player.CurrentVehicle ?? player.LastVehicle;
            isPlayerDriving = vehicle != null && vehicle.GetPedOnSeat(VehicleSeat.Driver) == player;

            if (!isPlayerDriving)
            {
                ResetVariables();
                return;
            }

            bool initialEngineState = vehicle.IsEngineRunning;
            bool exitHeld = Game.IsControlPressed(Control.VehicleExit);
            bool exitJustReleased = Game.IsControlJustReleased(Control.VehicleExit);
            bool keepEngineRunning = engineOnTime > 10;

            if (initialEngineState)
            {
                engineOnTime = Math.Min(engineOnTime + 1, 200);
                engineOffTime = 0;
            }
            else
            {
                engineOffTime = Math.Min(engineOffTime + 1, 200);
                if (engineOffTime > 30) engineOnTime = 0;
            }

            if (exitHeld)
            {
                holdDuration++;
            }

            if (exitJustReleased)
            {
                if (holdDuration < 15) // Tap
                {
                    if (keepEngineRunning)
                    {
                        vehicle.IsEngineRunning = true;
                    }
                    // vehicle.IsEngineRunning = true;
                }
                else // Hold
                {
                    vehicle.IsEngineRunning = false;
                }

                player.Task.LeaveVehicle(vehicle, closeDoorOnExit);
                ResetVariables();
            }
        }

        private void ResetVariables()
        {
            if (holdDuration != 0)
            {
                holdDuration = 0;
            }
        }
    }
}