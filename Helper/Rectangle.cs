using System.Drawing;

namespace AdvancedInteractionSystem
{
    public interface IElement
    {
        void Draw();
        void Draw(SizeF offset);
        bool Enabled { get; set; }
        PointF Position { get; set; }
        Color Color { get; set; }
    }
    public class Rectangle : IElement
    {
        public SizeF SizeF { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual PointF Position { get; set; }
        public virtual Color Color { get; set; }
        public Rectangle()
        {
            Enabled = true;
            Position = new PointF();
            SizeF = new SizeF(1280f, 720f);
            Color = Color.Transparent;
        }

        public Rectangle(PointF position, SizeF size)
        {
            Enabled = true;
            Position = position;
            SizeF = size;
            Color = Color.Transparent;
        }

        public Rectangle(PointF position, SizeF size, Color color)
        {
            Enabled = true;
            Position = position;
            SizeF = size;
            Color = color;
        }

        public virtual void Draw() => this.Draw(SizeF.Empty);

        public virtual void Draw(SizeF offset)
        {
            if (!this.Enabled)
                return;
            PointF position = this.Position;
            double x = (double)position.X + (double)offset.Width;
            position = this.Position;
            double y = (double)position.Y + (double)offset.Height;
            Rectangle.Draw(new PointF((float)x, (float)y), this.SizeF, this.Color);
        }

        public static void Draw(PointF position, SizeF size, Color color)
        {
            float width = size.Width / 1280f;
            float height = size.Height / 720f;
            N.DrawRect((float)((double)position.X / 1280.0 + (double)width * 0.5), (float)((double)position.Y / 720.0 + (double)height * 0.5), width, height, (int)color.R, (int)color.G, (int)color.B, (int)color.A);
        }
    }
}
