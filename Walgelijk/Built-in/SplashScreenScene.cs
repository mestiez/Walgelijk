using System;
using System.Linq;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Utility struct that provides splash screen scene creation functionality
    /// </summary>
    public struct SplashScreenScene
    {
        /// <summary>
        /// Create a splash screen scene
        /// </summary>
        /// <param name="logos">array of logos</param>
        /// <param name="durationPerLogo"></param>
        /// <param name="switchTo"></param>
        /// <returns></returns>
        public static Scene CreateScene(Texture[] logos, float durationPerLogo, Action onEnd)
        {
            Scene scene = new Scene();

            var splash = scene.CreateEntity();
            var shape = new RectangleShapeComponent
            {
                Material = new Material(Shader.Default)
            };

            scene.AttachComponent(splash, shape);
            scene.AttachComponent(splash, new SplashScreenComponent
            {
                Logos = logos,
                DurationPerLogo = durationPerLogo,
                OnEnd = onEnd
            });

            scene.AttachComponent(splash, new TransformComponent());

            var camera = scene.CreateEntity();
            scene.AttachComponent(camera, new TransformComponent());
            scene.AttachComponent(camera, new CameraComponent
            {
                PixelsPerUnit = 1
            });

            scene.AddSystem(new ShapeRendererSystem());
            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new CameraSystem());
            scene.AddSystem(new SplashScreenSystem());

            return scene;
        }

        /// <summary>
        /// Component with splash screen information
        /// </summary>
        public class SplashScreenComponent
        {
            /// <summary>
            /// Array of logos
            /// </summary>
            public Texture[] Logos = { };

            /// <summary>
            /// Seconds each logo should be visible for
            /// </summary>
            public float DurationPerLogo = 1f;

            /// <summary>
            /// Current elapsed time since the last logo change
            /// </summary>
            public float CurrentTime = 0;

            /// <summary>
            /// Current elapsed time since the creation
            /// </summary>
            public float Lifetime = 0;

            /// <summary>
            /// Current logo index
            /// </summary>
            public int CurrentLogoIndex = 0;

            /// <summary>
            /// What to do once all logos have been displayed
            /// </summary>
            public Action OnEnd;
        }
        
        /// <summary>
        /// System that handles <see cref="SplashScreenComponent"/>
        /// </summary>
        public class SplashScreenSystem : System
        {
            public override void Update()
            {
                var splashes = Scene.GetAllComponentsOfType<SplashScreenComponent>();

                if (splashes.Count() > 1)
                {
                    Logger.Warn($"There can only be a single instance of {nameof(SplashScreenComponent)} in the scene");
                    return;
                }
                else if (!splashes.Any()) return;

                var splash = splashes.First();
                HandleSplash(splash);
            }

            private void HandleSplash(EntityWith<SplashScreenComponent> splash)
            {
                var entity = splash.Entity;
                var component = splash.Component;

                var rect = Scene.GetComponentFrom<RectangleShapeComponent>(entity);
                var transform = Scene.GetComponentFrom<TransformComponent>(entity);

                if (component.Lifetime == 0)
                    setTexture(component.Logos[0]);

                component.CurrentTime += Time.UpdateDeltaTime;
                component.Lifetime += Time.UpdateDeltaTime;

                if (component.CurrentTime > component.DurationPerLogo)
                {
                    component.CurrentTime = 0;
                    component.CurrentLogoIndex++;
                    if (component.CurrentLogoIndex >= component.Logos.Length)
                    {
                        if (component.OnEnd != null)
                            component.OnEnd?.Invoke();
                        else
                            Scene.RemoveEntity(entity);
                        return;
                    }

                    setTexture(component.Logos[component.CurrentLogoIndex]);
                }

                void setTexture(Texture texture)
                {
                    transform.Scale = new Vector2(texture.Width, texture.Height);
                    transform.RecalculateModelMatrix();
                    rect.Material.SetUniform(ShaderDefaults.MainTextureUniform, texture);
                }
            }
        }
    }
}
