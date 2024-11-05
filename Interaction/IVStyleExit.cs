using System;
using System.ComponentModel.Design;
using AdvancedInteractionSystem;
using GTA;
using GTA.Native;

namespace AdvancedInteractionSystem
{
    public class IVStyleExit : Script
    {
        private bool keepEngineRunning = false;
        private bool eng = false;
        private int enforce = 0;
        private int dele = 0;
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

                #region Fix for IVStyleExit mod: 
                bool initialEngineState = vehicle.IsEngineRunning;
                
                if (initialEngineState)
                {
                    if (engineOnTime < 200)
                    {
                        engineOnTime++;
                    }                    
                    engineOffTime = 0;
                }
                else
                {
                    if (engineOffTime < 200)
                    {
                        engineOffTime++;
                    }
                }

                if (engineOffTime > 30)
                {
                    engineOnTime = 0;
                }
                #endregion

                if (keepEngineRunning || exitHeld)
                {
                    if (enforce < 10)
                    {
                        if (engineOnTime > 10)
                        {
                            keepEngineRunning = true;
                            vehicle.IsEngineRunning = true;
                        }
                        enforce++;
                        return;
                    }

                    if (dele < 200)
                    {
                        dele++;
                        return;
                    }

                    if (exitHeld)
                    {
                        vehicle.IsEngineRunning = false;
                        eng = true;
                    }
                    else
                    {
                        keepEngineRunning = true;
                        eng = false;
                    }

                    if (rest < 1000 && eng)
                    {
                        rest++;
                        return;
                    }
                    else
                    {
                        eng = false;
                    }
                    keepEngineRunning = false;
                    rest = 0;
                    dele = 0;
                    enforce = 0;
                }
            }
            else
            {
                keepEngineRunning = false;
                rest = 0;
                dele = 0;
                enforce = 0;
            }
        }
    }
}