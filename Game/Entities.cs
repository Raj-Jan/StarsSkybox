using Engine;

namespace Game
{
    public abstract class DynamicBody
    {
        public virtual Point Position { get; set; } = Point.Zero;
        public virtual Matrix World { get; set; } = Matrix.Identity;

        protected virtual Vector3 VelocityL { get; set; }
        protected virtual Vector3 VelocityA { get; set; }

        public virtual void Update(ITime time)
        {
            Position += time.Elapsed * VelocityL;
            World = World * Matrix.CreateRotation(time.Elapsed * VelocityA);
        }
    }
}
