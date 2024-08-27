using GTA.Math;
using GTA;
using GTA.UI;
using Screen = GTA.UI.Screen;
using GTA.Native;
using System.Drawing;

namespace AdvancedInteractionSystem
{
    // N = Native = GTA.Native;
    public class N
    {
        public static string GetGxtName(string name)
        {
            return Game.GetLocalizedString(name);
        }
        public static string GetMakeNameFromModel(Model model)
        {
            return Function.Call<string>(Hash.GET_MAKE_NAME_FROM_VEHICLE_MODEL, model);
        }
        public static void SetWaypoint(Vector3 position)
        {
            Function.Call(Hash.SET_NEW_WAYPOINT, position.X, position.Y);
        }
        public static float GetRandomFloatInRange(float min, float max)
        {
            return Function.Call<float>(Hash.GET_RANDOM_FLOAT_IN_RANGE, min, max);
        }
        public static void ShowHelpText(string text)
        {
            Function.Call(Hash.BEGIN_​TEXT_​COMMAND_​DISPLAY_​HELP, "STRING");
            Function.Call(Hash.ADD_​TEXT_​COMPONENT_​SUBSTRING_​PLAYER_​NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, false, false, -1);
        }
        public static void ShowSubtitle(string text, int duration)
        {
            Screen.ShowSubtitle(text, duration);
        }
        public static void DisplayNotification(string text, bool blinking)
        {
            Notification.Show(text, blinking);
        }
        public static void DisplayNotificationSMS(NotificationIcon icon, string sender, string subject, string message, bool fadeIn, bool blinking)
        {
            Notification.Show(icon, sender, subject, message, fadeIn, blinking);
        }

        public static void DrawDoorMarker(MarkerType markerType, Vector3 position, Color MarkerColor_Door)
        {
            World.DrawMarker(
                markerType,
                position,
                Vector3.Zero,
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                MarkerColor_Door,
                true,
                true,
                false,
                "",
                "",
                false
                );
        }

        public static void DrawMarker(int markerType, Vector3 position, float scale, int red, int green, int blue, int alpha, bool Bounces, bool FaceCamera)
        {
            Function.Call(
                Hash.DRAW_MARKER,
                markerType,
                position.X, position.Y, position.Z - 1,
                0, 0, 0,
                0, 0, 0,
                scale, scale, scale,
                red, green, blue, alpha,
                Bounces,
                FaceCamera,
                2,
                false
                );
        }

        public enum MarkerTypes
        {
            MarkerTypeUpsideDownCone = 0,
            MarkerTypeVerticalCylinder = 1,
            MarkerTypeThickChevronUp = 2,
            MarkerTypeThinChevronUp = 3,
            MarkerTypeCheckeredFlagRect = 4,
            MarkerTypeCheckeredFlagCircle = 5,
            MarkerTypeVerticleCircle = 6,
            MarkerTypePlaneModel = 7,
            MarkerTypeLostMCDark = 8,
            MarkerTypeLostMCLight = 9,
            MarkerTypeNumber0 = 10,
            MarkerTypeNumber1 = 11,
            MarkerTypeNumber2 = 12,
            MarkerTypeNumber3 = 13,
            MarkerTypeNumber4 = 14,
            MarkerTypeNumber5 = 15,
            MarkerTypeNumber6 = 16,
            MarkerTypeNumber7 = 17,
            MarkerTypeNumber8 = 18,
            MarkerTypeNumber9 = 19,
            MarkerTypeChevronUpx1 = 20,
            MarkerTypeChevronUpx2 = 21,
            MarkerTypeChevronUpx3 = 22,
            MarkerTypeHorizontalCircleFat = 23,
            MarkerTypeReplayIcon = 24,
            MarkerTypeHorizontalCircleSkinny = 25,
            MarkerTypeHorizontalCircleSkinny_Arrow = 26,
            MarkerTypeHorizontalSplitArrowCircle = 27,
            MarkerTypeDebugSphere = 28,
            MarkerTypeDallorSign = 29,
            MarkerTypeHorizontalBars = 30,
            MarkerTypeWolfHead = 31
        };

        public static string GetPlayerName(Ped player)
        {
            if (player.Model == new Model(PedHash.Michael)) return "Michael";
            else if (player.Model == new Model(PedHash.Franklin)) return "Franklin";
            else if (player.Model == new Model(PedHash.Trevor)) return "Trevor";
            else return "Other";
        }

        public static bool IsVehicleLocked(Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.GET_VEHICLE_DOORS_LOCKED_FOR_PLAYER, vehicle);
        }

        // .. 

        public static float GetEntityRoll(Entity entity)
        {
            return Function.Call<float>(Hash.GET_ENTITY_ROLL, entity);
        }

        public static float GetEntitySpeed(Entity entity)
        {
            return Function.Call<float>(Hash.GET_ENTITY_SPEED, entity);
        }

        public static Vector3 GetEntitySpeedVector(Entity entity, bool relative)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, entity, relative);
        }

        public static bool HasEntityCollidedWithAnything(Entity entity)
        {
            return Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, entity);
        }

        public static void DrawRect(float x, float y, float width, float height, int r, int g, int b, int a)
        {
            Function.Call<int>(Hash.DRAW_RECT, x, y, width, height, r, g, b, a, 0);
        }

        public static float GetSafeZoneSize()
        {
            return Function.Call<float>(Hash.GET_SAFE_ZONE_SIZE);
        }

        public static float GetAspectRatio(bool b)
        {
            return Function.Call<float>(Hash.GET_ASPECT_RATIO, b);
        }

        public static bool HasStreamedTextureDictLoaded(string textureDixt)
        {
            return Function.Call<bool>(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, textureDixt);
        }

        public static int RequestStreamedTextureDict(string textureDict, bool p1)
        {
            return Function.Call<int>(Hash.REQUEST_STREAMED_TEXTURE_DICT, textureDict, p1);
        }

        public static void BeginTextCommandDisplayText(string format)
        {
            // Function.Call(INativeValue.x25FBB336DF1804CB(format));
        }

        public static void EndTextCommandDisplayText(float x, float y)
        {
            //NativeFunction.Natives.xCD015E5BB0D96A57(x, y);
        }

        public static void AddTextComponentSubstringPlayerName(string text)
        {
            //NativeFunction.Natives.x6C188BE134E074AA(text);
        }

        public static bool IsHudHidden()
        {
            return Function.Call<bool>(Hash.IS_HUD_HIDDEN);
        }

        public static bool IsRadarHidden()
        {
            return Function.Call<bool>(Hash.IS_RADAR_HIDDEN);
        }

        public static void HideHudComponentThisFrame(int id)
        {
            Function.Call<int>(Hash.HIDE_HUD_COMPONENT_THIS_FRAME, id);
        }

        public static float GetFrameTime()
        {
            return Function.Call<float>(Hash.GET_FRAME_TIME);
        }

        public static int GetProfileSetting(int profileSetting)
        {
            return Function.Call<int>(Hash.GET_PROFILE_SETTING, profileSetting);
        }

        public static int SetPedConfigFlag(Ped ped, int flagId, bool value)
        {
            return Function.Call<int>(Hash.SET_PED_CONFIG_FLAG, ped, flagId, value);
        }

        public static int UpdateOnScreenKeyboard()
        {
            return Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD);
        }

        public static bool DoesObjectOfTypeExistAtCoords(float x, float y, float z, float radius, uint hash)
        {
            return Function.Call<bool>(Hash.DOES_OBJECT_OF_TYPE_EXIST_AT_COORDS, x,y,z,radius,hash,false);
        }

        public static int GetControlValue(int control, int action)
        {
            return Function.Call<int>(Hash.GET_CONTROL_VALUE, control,action);
        }

        public static float GetControlNormal(int control, int action)
        {
            return Function.Call<float>(Hash.GET_CONTROL_NORMAL, control,action);
        }


        public static int SetPedIntoVehicle(Ped ped, Vehicle vehicle, int seatIndex)
        {
            return Function.Call<int>(Hash.SET_PED_INTO_VEHICLE, ped, vehicle, seatIndex);
        }

        public static int DisablePlayerFiring(Player player, bool toggle)
        {
            return Function.Call<int>(Hash.DISABLE_PLAYER_FIRING, player, toggle);
        }

        public static int SetPlayerCanDoDriveBy(Player player, bool toggle)
        {
            return Function.Call<int>(Hash.SET_PLAYER_CAN_DO_DRIVE_BY, player, toggle);
        }

        public static bool HasModelLoaded(Model model)
        {
            //IL_0011: Unknown result type (might be due to invalid IL or missing references)
            return Function.Call<bool>(Hash.HAS_MODEL_LOADED, model);
        }

        public static int RequestAnimDict(string animDict)
        {
            return Function.Call<int>(Hash.REQUEST_ANIM_DICT, animDict);
        }

        public static int RequestCollisionAtCoord(float x, float y, float z)
        {
            return Function.Call<int>(Hash.REQUEST_COLLISION_AT_COORD, x,y,z);
        }

        public static int RequestModel(Model model)
        {
            //IL_0011: Unknown result type (might be due to invalid IL or missing references)
            return Function.Call<int>(Hash.REQUEST_MODEL, model);
        }

        public static bool GetIsTaskActive(Ped ped, int taskIndex)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped, taskIndex);
        }

        public static bool IsVehicleTyreBurst(Vehicle vehicle, int wheelID, bool completely)
        {
            return Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, wheelID, completely);
        }

        public static void SetVehicleEngineOn(Vehicle vehicle, bool value, bool instantly, bool disableAutoStart)
        {
            Function.Call<int>(Hash.SET_VEHICLE_ENGINE_ON, vehicle, value, instantly, disableAutoStart);
        }

        public static float SetVehicleForwardSpeed(Vehicle vehicle, float speed)
        {
            return Function.Call<float>(Hash.SET_VEHICLE_FORWARD_SPEED, vehicle, speed);
        }

        public static bool SetVehicleBrakeLights(Vehicle vehicle, bool toggle)
        {
            return Function.Call<bool>(Hash.SET_VEHICLE_BRAKE_LIGHTS, vehicle, toggle);
        }

        public static int SetVehicleBodyHealth(Vehicle vehicle, float value)
        {
            return Function.Call<int>(Hash.SET_VEHICLE_BODY_HEALTH, vehicle, value);
        }

        public static void SetVehicleMaxSpeed(Vehicle vehicle, float speed)
        {
            Function.Call<int>(Hash.SET_VEHICLE_MAX_SPEED, vehicle, speed);
        }

        public static int SetVehicleTyreBurst(Vehicle vehicle, int index, bool onRim, float p3)
        {
            return Function.Call<int>(Hash.SET_VEHICLE_TYRES_CAN_BURST, vehicle, index, onRim, p3);
        }

        public static int SetVehicleTyreFixed(Vehicle vehicle, int tyreIndex)
        {
            return Function.Call<int>(Hash.SET_VEHICLE_TYRE_FIXED, vehicle, tyreIndex);
        }

        public static int GetLastPedInVehicleSeat(Vehicle vehicle, int seatIndex)
        {
            return Function.Call<int>(Hash.GET_LAST_PED_IN_VEHICLE_SEAT, vehicle, seatIndex);
        }

        public static float GetVehicleBodyHealth(Vehicle vehicle)
        {
            return Function.Call<float>(Hash.GET_VEHICLE_BODY_HEALTH,vehicle);
        }

        public static int RollDownWindows(Vehicle vehicle)
        {
            return Function.Call<int>(Hash.ROLL_DOWN_WINDOWS, vehicle);
        }
    }
#if false // Decompilation log
'13' items in cache
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
------------------
Resolve: 'RagePluginHook, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'RagePluginHook, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Could not find by name: 'System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll'
------------------
Resolve: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Drawing.dll'
------------------
Resolve: 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Microsoft.CSharp.dll'
#endif

}

