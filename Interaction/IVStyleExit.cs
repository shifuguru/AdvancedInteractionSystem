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

        public IVStyleExit()
        {
            Tick += OnTick;
            Interval = 0;
        }

        void OnTick(object sender, EventArgs e)
        {
            player = Function.Call<Ped>(Hash.PLAYER_PED_ID);
            vehicle = player.CurrentVehicle;
            isPlayerDriving = vehicle != null && vehicle.GetPedOnSeat(VehicleSeat.Driver) == player;

            if (!isPlayerDriving)
            {
                ResetVariables();
                return;
            }

            bool exitHeld = Game.IsControlPressed(Control.VehicleExit);
            bool exitJustReleased = Game.IsControlJustReleased(Control.VehicleExit);

            if (exitHeld)
            {
                holdDuration++;
            }

            if (exitJustReleased)
            {
                if (holdDuration < 15)
                {
                    vehicle.IsEngineRunning = true;
                }
                else
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