using System;
using System.IO;
using System.Media;


namespace AdvancedInteractionSystem
{
    public class AudioManager
    {
        private static string audioPath = Path.Combine("scripts", "AIS", "audio");

        public static void PlaySound(string filename)
        {
            try
            {
                string fullPath = Path.Combine(audioPath, filename);

                if (File.Exists(fullPath))
                {
                    using (SoundPlayer player = new SoundPlayer(fullPath))
                    {
                        player.Play(); // Play sound asynchronously
                    }
                }
                else
                {
                    AIS.LogException("PlaySound", new FileNotFoundException($"Sound file not found: {fullPath}"));
                }
            }
            catch (Exception ex)
            {
                AIS.LogException("AudioManager.PlaySound", ex);
            }
        }
    }
}
