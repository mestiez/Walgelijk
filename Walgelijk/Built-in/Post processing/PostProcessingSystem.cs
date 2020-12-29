using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// System that handles all built-in post processing
    /// </summary>
    public class PostProcessingSystem : System
    {
        private bool needsNewRT = true;

        private RenderTexture rt0;
        private RenderTexture rt1;

        private TargetRenderTask targetTask;
        private TargetRenderTask windowTargetTask;

        private ActionRenderTask postProcessingTask;
        private ShapeRenderTask fullScreenQuadTask;

        private Material fullscreenMaterial;

        public override void Initialise()
        {
            Scene.Game.Window.OnResize += OnResize;

            windowTargetTask = new TargetRenderTask
            {
                Target = Scene.Game.Window.RenderTarget
            };

            targetTask = new TargetRenderTask();
            postProcessingTask = new ActionRenderTask(ApplyEffects);

            fullscreenMaterial = new();
            fullScreenQuadTask = new ShapeRenderTask(PrimitiveMeshes.Quad, material: fullscreenMaterial) { ScreenSpace = true };
        }

        private void CreateRenderTextures()
        {
            needsNewRT = false;

            disposeOf(rt0);
            disposeOf(rt1);

            rt0 = getNewTexture();
            rt1 = getNewTexture();

            targetTask.Target = rt0;

            Logger.Log($"New post-processing RenderTextures created with size {rt0.Size}");

            void disposeOf(RenderTexture rt)
            {
                if (rt != null) rt.Dispose();
            }

            RenderTexture getNewTexture()
            {
                var s = Scene.Game.Window.Size;
                return new RenderTexture((int)s.X, (int)s.Y, hdr: true);
            }
        }

        private void OnResize(object sender, Vector2 e)
        {
            needsNewRT = true;
        }

        public override void PreRender()
        {
            if (needsNewRT)
                CreateRenderTextures();

            RenderQueue.Add(targetTask, int.MinValue);
        }

        public override void PostRender()
        {
            var size = Scene.Game.Window.Size;
            fullScreenQuadTask.ModelMatrix = Matrix4x4.CreateTranslation(0, -1, 0) * Matrix4x4.CreateScale(size.X, -size.Y, 1);

            RenderQueue.Add(postProcessingTask, int.MaxValue);
            RenderQueue.Add(windowTargetTask, int.MaxValue);
            RenderQueue.Add(fullScreenQuadTask, int.MaxValue);
        }

        private void ApplyEffects(IGraphics graphics)
        {
            var containers = Scene.GetAllComponentsOfType<PostProcessingComponent>();
            RenderTexture last = null;

            foreach (var item in containers)
            {
                var entity = item.Entity;
                var container = item.Component;

                var a = rt0;
                var b = rt1;
                bool flipState = false;

                foreach (var effect in container.Effects)
                {
                    effect.Process(a, b, graphics, Scene);
                    last = b;

                    if (flipState)
                    {
                        a = rt0;
                        b = rt1;
                    }
                    else
                    {
                        a = rt1;
                        b = rt0;
                    }

                    flipState = !flipState;
                }
            }

            fullscreenMaterial.SetUniform(ShaderDefaults.MainTextureUniform, last ?? rt0);
        }
    }
}
