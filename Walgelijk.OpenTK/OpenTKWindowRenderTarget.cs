using OpenTK.Graphics.OpenGL;
using Vector2 = System.Numerics.Vector2;

namespace Walgelijk.OpenTK
{
    public class OpenTKWindowRenderTarget : RenderTarget
    {
        internal OpenTKWindow Window { get; set; }
        private Vector2 size;

        public override Vector2 Size
        {
            get => size;

            set
            {
                size = value;
                if (Window.Graphics.CurrentTarget == this)
                    GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            }
        }

        internal void Initialise()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GPUObjects.RenderTargetDictionary.Set(this, 0);
            Window.internalGraphics.CurrentTarget = this;
        }
    }
}
