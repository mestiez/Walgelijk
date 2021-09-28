using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Utility struct that provides background image creation functionality
    /// </summary>
    public struct Background
    {
        /// <summary>
        /// Creates a background for the scene. Also creates the necessary systems if they are not already present. By default, backgrounds are rendered at render order (-1000, 0)
        /// </summary>
        public static EntityWith<BackgroundComponent> CreateBackground(Scene scene, IReadableTexture texture)
        {
            if (!scene.HasSystem<BackgroundSystem>())
                scene.AddSystem(new BackgroundSystem());
            var ent = scene.CreateEntity();
            var mat = new Material(Material.DefaultTextured);
            mat.SetUniform(ShaderDefaults.MainTextureUniform, texture);
            var bg = scene.AttachComponent(ent, new BackgroundComponent
            {
                Visible = true,
                Material = mat,
                RenderOrder = new RenderOrder(-1000, 0),
                RenderTask = new ShapeRenderTask(PrimitiveMeshes.Quad) { ScreenSpace = true }
            });

            return new EntityWith<BackgroundComponent>(bg, ent);
        }

        /// <summary>
        /// Handles backgrounds created using <see cref="Background.CreateBackground(Scene, IReadableTexture)"/>
        /// </summary>
        public class BackgroundSystem : Walgelijk.System
        {
            public override void Render()
            {
                var windowSize = Scene.Game.Window.Size;
                var stretch = Matrix4x4.CreateScale(windowSize.X, -windowSize.Y, 1) * Matrix4x4.CreateTranslation(0, windowSize.Y,0);

                foreach (var item in Scene.GetAllComponentsOfType<BackgroundComponent>())
                {
                    var bg = item.Component;
                    if (!bg.Visible || bg.Material == null)
                        continue;

                    bg.RenderTask.Material = bg.Material;
                    bg.RenderTask.ModelMatrix = stretch * Matrix4x4.CreateTranslation(bg.Offset.X, bg.Offset.Y, 0);

                    RenderQueue.Add(bg.RenderTask, bg.RenderOrder);
                }
            }
        }

        /// <summary>
        /// Contains the texture to render and what properties the background has
        /// </summary>
        public class BackgroundComponent
        {
            /// <summary>
            /// Material to draw with
            /// </summary>
            public Material Material;
            /// <summary>
            /// The order at which to draw the backgound. 
            /// </summary>
            public RenderOrder RenderOrder;
            /// <summary>
            /// Whether to draw it at all
            /// </summary>
            public bool Visible = true;
            /// <summary>
            /// Translational offset
            /// </summary>
            public Vector2 Offset;

            /// <summary>
            /// Relevant render task
            /// </summary>
            public ShapeRenderTask RenderTask;
        }
    }
}
