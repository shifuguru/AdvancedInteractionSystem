using System;
using GTA;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    public class IVStyleExit : Script
    {
        private bool keepEngineRunning = false;
        private int enforce = 0;
        private int delay = 0;
        private int rest = 0;

        private int engineOnTime = 0;
        private int engineOffTime = 0;

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
            isPlayerDriving = vehicle != null && vehicle.GetPedOnSeat(VehicleSeat.LeftFront) == player;

            if (vehicle != null && isPlayerDriving)
            {
                bool exitHeld = Game.IsControlPressed(Control.VehicleExit);
                
                if (vehicle.IsEngineRunning)
                {
                    engineOnTime = Math.Min(engineOnTime + 1, 200);
                    engineOffTime = 0;
                }
                else
                {
                    engineOffTime = Math.Min(engineOffTime + 1, 200);
                    if (engineOffTime > 30) engineOnTime = 0;
                }
                
                if (exitHeld || keepEngineRunning)
                {
                    if (enforce < 10)
                    {
                        if (engineOnTime > 10)
                        {
                            vehicle.IsEngineRunning = true;
                            keepEngineRunning = true;
                        }
                        enforce++;
                        return;
                    }

                    if (delay < 200)
                    {
                        delay++;
                        return;
                    }

                    if (exitHeld)
                    {
                        vehicle.IsEngineRunning = false;
                        keepEngineRunning = false;
                    }
                    else
                    {
                        keepEngineRunning = true;
                    }

                    if (rest < 1000 && !vehicle.IsEngineRunning)
                    {
                        rest++;
                        return;
                    }

                    ResetVariables();
                }
            }
            else
            {
                ResetVariables();
            }
        }

        private void ResetVariables()
        {
            enforce = 0;
            delay = 0;
            rest = 0;
            engineOffTime = 0;
            engineOnTime = 0;
        }
    }
}