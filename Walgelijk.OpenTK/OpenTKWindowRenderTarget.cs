using OpenTK.Graphics.OpenGL;
using System.Collections;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using System.Runtime.CompilerServices;

namespace Walgelijk.OpenTK
{
    public class OpenTKWindowRenderTarget : Walgelijk.RenderTarget
    {
        internal OpenTKWindow Window { get; set; }
        private Vector2 size;

        public override Vector2 Size
        {
            get => size;

            set
            {
                size = value;
                GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            }
        }

        internal void Initialise()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Window.Graphics.CurrentTarget = this;
        }
    }
}
