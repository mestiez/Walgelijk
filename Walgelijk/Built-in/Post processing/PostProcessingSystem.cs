using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// System that handles post processing effects with <see cref="PostProcessingComponent"/> and <see cref="IPostProcessingEffect"/>
    /// </summary>
    [global::System.Obsolete]
    public class PostProcessingSystem : System
    {
        private bool needsNewRT = true;

        private RenderTexture? rt0;
        private RenderTexture? rt1;

        //TODO uitzoeken of rt1 wel nodig is en of je niet gewoon de target van de window kan gebruiken????????

        private ActionRenderTask? rt0TargetTask;

        private Material? fullscreenMaterial;
        private Matrix4x4 fullscreenModel;

        public override void Initialise()
        {
            Scene.Game.Window.OnResize += OnResize;

            rt0TargetTask = new ActionRenderTask(PrepareTarget);
            fullscreenMaterial = new();
        }

        private void CreateRenderTextures()
        {
            needsNewRT = false;

            disposeOf(rt0);
            disposeOf(rt1);

            rt0 = getNewTexture();
            rt1 = getNewTexture();

            Logger.Log($"New post-processing RenderTextures created with size {rt0.Size}");

            void disposeOf(RenderTexture? rt)
            {
                if (rt != null)
                    rt.Dispose();
            }

            RenderTexture getNewTexture()
            {
                var s = Scene.Game.Window.Size;
                return new RenderTexture((int)s.X, (int)s.Y, hdr: true);
            }
        }

        private void OnResize(object? sender, Vector2 e)
        {
            needsNewRT = true;
        }

        public override void PreRender()
        {
            if (needsNewRT)
                CreateRenderTextures();

            var containers = Scene.GetAllComponentsOfType<PostProcessingComponent>();

            if (rt0TargetTask != null)
                foreach (var item in containers)
                    if (item.Enabled)
                        RenderQueue.Add(rt0TargetTask, item.Begin);
        }

        public override void PostRender()
        {
            var size = Scene.Game.Window.Size;
            fullscreenModel = Matrix4x4.CreateTranslation(0, -1, 0) * Matrix4x4.CreateScale(size.X, -size.Y, 1);

            var containers = Scene.GetAllComponentsOfType<PostProcessingComponent>();

            foreach (var container in containers)
            {
                if (!container.Enabled)
                    continue;

                if (container.EffectTask == null)
                    container.EffectTask = new ActionRenderTask((g) => { ApplyEffects(container, g); });

                RenderQueue.Add(container.EffectTask, container.End);
            }
        }

        private void PrepareTarget(IGraphics graphics)
        {
            if (rt0 == null || rt1 == null)
                return;

            var windowTarget = Scene.Game.Window.RenderTarget;

            graphics.CurrentTarget = rt1;
            graphics.Clear(Colors.Transparent);
            graphics.CurrentTarget = rt0;
            graphics.Clear(Colors.Transparent);

            rt0.ProjectionMatrix = windowTarget.ProjectionMatrix;
            rt1.ProjectionMatrix = windowTarget.ProjectionMatrix;

            rt0.ViewMatrix = windowTarget.ViewMatrix;
            rt1.ViewMatrix = windowTarget.ViewMatrix;
        }

        private void ApplyEffects(PostProcessingComponent component, IGraphics graphics)
        {
            if (rt0 == null || rt1 == null || fullscreenMaterial == null)
                return;

            RenderTexture? last = null;

            var a = rt0;
            var b = rt1;
            bool flipState = false;

            foreach (var effect in component.Effects)
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

            if (last == null)
                return;

            fullscreenMaterial.SetUniform(ShaderDefaults.MainTextureUniform, last);

            var windowTarget = Scene.Game.Window.RenderTarget;

            windowTarget.ProjectionMatrix = rt0.OrthographicMatrix;
            windowTarget.ViewMatrix = Matrix4x4.Identity;

            graphics.CurrentTarget = windowTarget;
            windowTarget.ModelMatrix = fullscreenModel;
            graphics.Draw(PrimitiveMeshes.Quad, fullscreenMaterial);

            windowTarget.ProjectionMatrix = rt0.ProjectionMatrix;
            windowTarget.ViewMatrix = rt0.ViewMatrix;
        }
    }
}
