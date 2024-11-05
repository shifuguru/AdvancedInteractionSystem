using System;
using System.IO;
using GTA;

namespace AdvancedInteractionSystem
{
    public class IVStyleExit : Script
    {
        public static bool closeDoorOnExit;
        public static bool keepEngineRunning;
        public static int exitHeldTime;
        public static Ped player = null;
        public static Vehicle vehicle = null;
        public static bool isPlayerDriving = false;
        public readonly string defaultContent = "; 1 = enable; 0 = disable" + "?closeDoorOnExit=1";

        public IVStyleExit()
        {
            readConfig();
            Tick += new EventHandler(OnTick);
            Interval = 1;
        }

        public void OnTick(object sender, EventArgs e)
        {
            player = Game.Player.Character;
            vehicle = player.CurrentVehicle;

            isPlayerDriving = vehicle != null && vehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.Player.Character;

            if (isPlayerDriving)
            {
                bool exitHeld = Game.IsControlPressed(Control.VehicleExit);

                if (keepEngineRunning || exitHeld && vehicle.LockStatus.Equals(1))
                {
                    if (!exitHeld)
                        player.Task.LeaveVehicle(vehicle, closeDoorOnExit);

                    ++exitHeldTime;

                    if (exitHeldTime < 11)
                    {
                        keepEngineRunning = true;
                        vehicle.IsEngineRunning = true;
                        return;
                    }

                    if (exitHeldTime < 211)
                        return;

                    vehicle.IsEngineRunning = !exitHeld;

                    if (exitHeldTime < 1211 && !vehicle.IsEngineRunning)
                        return;

                    vehicle.IsEngineRunning = false;
                }
            }
            keepEngineRunning = false;
            exitHeldTime = 0;
        }

        private void readConfig()
        {
            string path = BaseDirectory + "\\IVStyleExit.custom";
            if (!File.Exists(path))
            {
                createConfig();
            }
            else
            {
                try
                {
                    string[] strArray = File.ReadAllLines(path);
                    int num = 0;
                    foreach (string str1 in strArray)
                    {
                        ++num;
                        string str2 = str1.Trim();
                        if (str2[0] != ';' && !string.IsNullOrEmpty(str2) && str2[0] == '?' && str2.StartsWith("?closeDoorOnExit=") && str2.Substring("?closeDoorOnExit=".Length)[0] == '1')
                        {
                            closeDoorOnExit = true;
                            break;
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private void createConfig()
        {
            string path = BaseDirectory + "\\IVStyleExit.custom";
            try
            {
                File.Create(path).Close();
                File.WriteAllText(path, defaultContent);
            }
            catch
            {

            }
        }

        //..
    }
}