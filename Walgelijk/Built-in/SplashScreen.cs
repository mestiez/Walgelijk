using System;
using System.Linq;
using System.Numerics;

namespace Walgelijk
{

    /// <summary>
    /// Utility struct that provides splash screen creation functionality
    /// </summary>
    public struct SplashScreen
    {
        /// <summary>
        /// Create a splash screen scene
        /// </summary>
        /// <param name="logos">Array of logos</param>
        /// <param name="onEnd">What to do when the logo sequence ends. This is usually a scene change</param>
        /// <returns></returns>
        public static Scene CreateScene(Logo[] logos, Action onEnd)
        {
            Scene scene = new Scene();

            var splash = scene.CreateEntity();
            var shape = new RectangleShapeComponent
            {
                Material = new Material(Shader.Default)
            };

            scene.AttachComponent(splash, new SplashScreenComponent
            {
                Logos = logos,
                OnEnd = onEnd
            });

            scene.AttachComponent(splash, new TransformComponent());
            scene.AttachComponent(splash, shape);

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
            public Logo[] Logos = { };

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
            public Action? OnEnd;
        }

        /// <summary>
        /// Structure with information on how to display a logo
        /// </summary>
        public struct Logo
        {
            /// <summary>
            /// Texture to display
            /// </summary>
            public IReadableTexture Texture;
            /// <summary>
            /// How long the logo should appear for
            /// </summary>
            public float Duration;
            /// <summary>
            /// Sound to play
            /// </summary>
            public Sound? Sound;

            /// <summary>
            /// Create a logo with a texture and an optional sound
            /// </summary>
            public Logo(IReadableTexture texture, float duration = 1f, Sound? sound = null)
            {
                Texture = texture;
                Duration = duration;
                Sound = sound;
            }
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

                if (rect == null || transform == null)
                    return;

                if (component.Lifetime == 0)
                    setLogo(component.Logos[0]);

                component.CurrentTime += Time.DeltaTime;
                component.Lifetime += Time.DeltaTime;

                if (component.CurrentTime > component.Logos[component.CurrentLogoIndex].Duration)
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

                    setLogo(component.Logos[component.CurrentLogoIndex]);
                }

                void setLogo(Logo logo)
                {
                    var texture = logo.Texture;
                    var sound = logo.Sound;

                    Audio.StopAll();

                    if (sound != null)
                        Audio.PlayOnce(sound);

                    transform.Scale = new Vector2(texture.Width, texture.Height);
                    transform.RecalculateModelMatrix(Matrix4x4.Identity);
                    rect.Material.SetUniform(ShaderDefaults.MainTextureUniform, texture);
                }
            }
        }
    }
}
