using System;
using GTA;
using GTA.Native;

public class IVStyleExit : Script
{
    private bool keepEngineRunning = false;
    private bool eng = false;
    private int enforce = 0;
    private int dele = 0;
    private int rest = 0;

    private Ped player;
    private Vehicle veh;
    private bool isPlayerDriving = false;
    public IVStyleExit()
    {
        Tick += OnTick;
        Interval = 0;
    }

    void OnTick(object sender, EventArgs e)
    {
        player = Function.Call<Ped>(Hash.PLAYER_PED_ID);
        veh = player.CurrentVehicle;
        isPlayerDriving = veh != null && veh.GetPedOnSeat(VehicleSeat.LeftFront) == player;

        if (veh != null && isPlayerDriving)
        {
            if (keepEngineRunning || Game.IsControlPressed(2, Control.VehicleExit))
            {
                if (enforce < 10)
                {
                    keepEngineRunning = true;
                    veh.EngineRunning = true;
                    enforce++;
                    return;
                }

                if (dele < 200)
                {
                    dele++;
                    return;
                }

                if (Game.IsControlPressed(2, Control.VehicleExit))
                {
                    veh.EngineRunning = false;
                    eng = true;
                }
                else
                {
                    veh.EngineRunning = true;
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
