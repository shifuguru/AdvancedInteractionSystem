using GTA;
using System.Collections.Generic;

namespace AdvancedInteractionSystem
{
    public class Helper
    {
        public static bool dispVehName = false;
        public static Control saveKey = (Control)51;
        public static bool showBlips = true;
        public static int alarmVolume = 100;
        public static bool saveDamage = true;
        public static string soundPath = ".\\scripts\\Persistence\\Sound\\";
        public static List<Vehicle> listOfVeh = new List<Vehicle>();
        public static List<Vehicle> listOfTrl = new List<Vehicle>();
        public static bool IsVehicleLoaded = false;
        public static bool IsVehicleLoading = false;
        public static string modDecor = "inm_persistence";
        public static string lastVehDecor = "inm_persistence_last_vehicle";
        public static string modDecor2 = "inm_persistence_2";
        public static string nitroModDecor = "inm_nitro_active";
        public static string flatbedModDecor = "inm_flatbed_installed";
        public static string lastFbVehDecor = "inm_flatbed_last";
        public static Model carKeyModel = new Model("lr_prop_carkey_fob");



    }
}