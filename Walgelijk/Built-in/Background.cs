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
        public static BackgroundComponent CreateBackground(Scene scene, IReadableTexture texture)
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
                Size = texture.Size,
                RenderTask = new ShapeRenderTask(PrimitiveMeshes.Quad) { ScreenSpace = true }
            });

            return bg;
        }

        /// <summary>
        /// Handles backgrounds created using <see cref="Background.CreateBackground(Scene, IReadableTexture)"/>
        /// </summary>
        public class BackgroundSystem : Walgelijk.System
        {
            public override void Render()
            {
                var windowSize = Scene.Game.Window.Size;
                var stretch = Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(0, windowSize.Y);

                foreach (var bg in Scene.GetAllComponentsOfType<BackgroundComponent>())
                {
                    if (!bg.Visible || bg.Material == null || bg.RenderTask == null)
                        continue;

                    var textureSize = bg.Size;

                    Vector2 imageSize = windowSize;
                    Vector2 imagePos = Vector2.Zero;

                    switch (bg.Mode)
                    {
                        case BackgroundMode.Stretch:
                            imageSize = windowSize;
                            break;
                        case BackgroundMode.Contain:
                        case BackgroundMode.Cover:
                            var aspectRatio = textureSize.X / textureSize.Y;

                            imageSize = windowSize;
                            bool a = windowSize.X / aspectRatio > windowSize.Y;

                            if (bg.Mode == BackgroundMode.Contain)
                                a = !a;

                            if (a)
                                imageSize.Y = windowSize.X / aspectRatio;
                            else
                                imageSize.X = windowSize.Y * aspectRatio;

                            imagePos = windowSize / 2 - imageSize / 2;
                            break;
                        case BackgroundMode.Center:
                            imageSize = textureSize;
                            imagePos = windowSize / 2 - imageSize / 2;
                            break;
                        default:
                            break;
                    }

                    bg.RenderTask.Material = bg.Material;
                    bg.RenderTask.ModelMatrix = Matrix3x2.CreateScale(imageSize) * Matrix3x2.CreateTranslation(
                        bg.Offset.X * windowSize.X + imagePos.X, 
                        bg.Offset.Y * windowSize.Y + imagePos.Y) * stretch;
                    //bg.RenderTask.ModelMatrix = stretch * Matrix3x2.CreateTranslation(bg.Offset.X * windowSize.X, bg.Offset.Y * windowSize.Y);

                    RenderQueue.Add(bg.RenderTask, bg.RenderOrder);
                }
            }
        }

        /// <summary>
        /// Contains the texture to render and what properties the background has
        /// </summary>
        public class BackgroundComponent : Component
        {
            /// <summary>
            /// Material to draw with
            /// </summary>
            public Material? Material;
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
            /// Manner in which to draw the background 
            /// </summary>
            public BackgroundMode Mode;

            /// <summary>
            /// Size of the texture
            /// </summary>
            public Vector2 Size;

            /// <summary>
            /// Relevant render task
            /// </summary>
            public ShapeRenderTask? RenderTask;
        }

        /// <summary>
        /// Manners in which to draw the background
        /// </summary>
        public enum BackgroundMode
        {
            /// <summary>
            /// Fill the whole screen
            /// </summary>
            Stretch,
            /// <summary>
            /// Original size, centered
            /// </summary>
            Center,
            /// <summary>
            /// Fill the whole screen while keeping the aspect ratio
            /// </summary>
            Cover,
            /// <summary>
            /// Grow as large as possible without cropping
            /// </summary>
            Contain
        }
    }
}
