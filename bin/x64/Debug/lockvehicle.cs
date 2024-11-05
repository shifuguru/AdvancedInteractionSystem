public static void LockVehicle(this Vehicle vehicle, Ped ped)
   {
     ped.SetPedCurrentWeaponVisible();
     Prop prop = World.CreateProp(Helper.carKeyModel, Vector3.Add(ped.Position, ped.ForwardVector), Vector3.Zero, true, false);
     prop.AttachTo(ped, ped.GetBoneIndex(28422), Vector3.Zero, Vector3.Zero);
     ped.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10f, 1500, (AnimationFlags) 49);
     Indicator.veh = vehicle;
     Script.Wait(500);
     using (WaveStream waveStream = new WaveStream((Stream) File.OpenRead(string.Format("{0}lock.wav", (object) Helper.soundPath))))
     {
       waveStream.Volume = Helper.alarmVolume;
       using (System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer((Stream) waveStream))
         soundPlayer.Play();
     }
     vehicle.LockStatus = (VehicleLockStatus) 3;
     vehicle.HasAlarm = true;
     Script.Wait(500);
     (prop).Detach();
     (prop).Delete();
     Indicator.veh = (Vehicle) null;
   }

   public static void UnlockVehicle(this Vehicle vehicle, Ped ped)
   {
     ped.SetPedCurrentWeaponVisible();
     Prop prop = World.CreateProp(Helper.carKeyModel, Vector3.Add((ped).Position, (ped).ForwardVector), Vector3.Zero, true, false);
     (prop).AttachTo(ped, ped.GetBoneIndex((Bone) 28422), Vector3.Zero, Vector3.Zero);
     ped.Task.PlayAnimation("anim@mp_player_intmenu@key_fob@", "fob_click_fp", 10f, 1500, (AnimationFlags) 49);
     Indicator.veh = vehicle;
     Script.Wait(500);
     using (WaveStream waveStream = new WaveStream((Stream) File.OpenRead(string.Format("{0}unlock.wav", (object) Helper.soundPath))))
     {
       waveStream.Volume = Helper.alarmVolume;
       using (System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer((Stream) waveStream))
         soundPlayer.Play();
     }
     vehicle.LockStatus = (VehicleLockStatus) 1;
     Script.Wait(500);
     (prop).Detach();
     (prop).Delete();
     Indicator.veh = (Vehicle) null;
   }

   public static void SetPedCurrentWeaponVisible(this Ped ped)
    {
      Function.Call((Hash) 514998932744280688L, new InputArgument[5]
      {
        ((ped).Handle),
        (false),
        (true),
        (true),
        (true)
      });
    }




    public void HandleAISpinoutHelp(Vehicle veh)
      {
        if (!CanWeUse(veh) || !veh.IsOnAllWheels || ForwardSpeed(veh) <= 10.0 || !IsSliding(veh, 6f) || IsPotentiallySliding(veh, 1.5f))
          return;
        if (Function.Call<Vector3>(Hash.GET_​ENTITY_​SPEED_​VECTOR, new InputArgument[2]
        {
          (veh),
          (true)
        }).X > 1.0)
        {
          if (debugEnabled)
            World.DrawMarker((MarkerType) 0, Vector3.Add(Vector3.Add((veh).Position, Vector3.Multiply((veh).ForwardVector, -2f)), (veh).RightVector), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1f, 1f, 1f), Color.Blue);
          Function.Call(Hash.APPLY_​FORCE_​TO_​ENTITY,
            (veh),
            (3),
            (0.1f),
            (0.0f),
            (0.0f),
            (0.0f),
            (2f),
            (-0.2f),
            (0),
            (true),
            (true),
            (true),
            (true),
            (true));
        }
        else if (Function.Call<Vector3>(Hash.GET_​ENTITY_​SPEED_​VECTOR, (veh), (true)).X < -1.0)
        {
          if (debugEnabled)
            World.DrawMarker((MarkerType) 0, Vector3.Subtract(Vector3.Add((veh).Position, Vector3.Multiply((veh).ForwardVector, -2f)), (veh).RightVector), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1f, 1f, 1f), Color.Blue);
          Function.Call(Hash.APPLY_​FORCE_​TO_​ENTITY,
            (veh),
            (3),
            (-0.1f),
            (0.0f),
            (0.0f),
            (0.0f),
            (2f),
            (-0.2f),
            (0),
            (true),
            (true),
            (true),
            (true),
            (true));
        }
      }
