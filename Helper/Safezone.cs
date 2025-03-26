using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdvancedInteractionSystem
{
    public class Safezone : Script
    {
        public Safezone()
        {
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            // Get the safe zone size (value between 0.0 and 1.0)
            float safeZone = Function.Call<float>(Hash.GET_SAFE_ZONE_SIZE);

            // Adjust the text scale based on the safe zone (you can tweak this multiplier)
            float scale = 0.35f * safeZone;

            // Draw the text on the screen
            Vector2 position = new Vector2(0.5f, 0.5f); // Center of the screen
            string text = "Scaled Text Example";

            // Use the safe zone scale to adjust text size
            DrawText(text, position, scale);
        }

        private void DrawText(string text, Vector2 position, float scale)
        {
            // The DrawText function (scaled version)
            //UIElement.DrawText(text, position.X, position.Y, scale, scale, true, true, Color.White);
        }
    }
}
