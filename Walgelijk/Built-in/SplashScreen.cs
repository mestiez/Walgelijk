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
        public static Scene CreateScene(IReadableTexture? background, Logo[] logos, Action onEnd, Transition transition = Transition.Cut, bool canSkip = true)
        {
            var scene = new Scene();

            var splash = scene.CreateEntity();
            scene.AttachComponent(splash, new SplashScreenComponent
            {
                Background = background,
                Logos = logos,
                OnEnd = onEnd,
                CanSkip = canSkip,
                Transition = transition
            });
            scene.AttachComponent(splash, new TransformComponent());
            scene.AttachComponent(splash, new RectangleShapeComponent
            {
                Material = new Material(Shader.Default)
            });

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

            if (background != null)
            {
                var b = Background.CreateBackground(scene, background);
                b.Mode = Background.BackgroundMode.Contain;
            }

            return scene;
        }

        /// <summary>
        /// Component with splash screen information
        /// </summary>
        public class SplashScreenComponent : Component
        {
            /// <summary>
            /// Array of logos
            /// </summary>
            public Logo[] Logos = Array.Empty<Logo>();

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

            /// <summary>
            /// Optional background that is drawn under everything
            /// </summary>
            public IReadableTexture? Background;

            /// <summary>
            /// Transition between logos
            /// </summary>
            public Transition Transition;

            /// <summary>
            /// Amount of time the transition takes
            /// </summary>
            public TimeSpan TransitionDuration = TimeSpan.FromSeconds(0.25f);

            /// <summary>
            /// Can the entire sequence be skipped by pushing any button (mouse or keyboard)? 
            /// </summary>
            public bool CanSkip;
        }

        public enum Transition
        {
            /// <summary>
            /// No transition. Just cut.
            /// </summary>
            Cut,
            /// <summary>
            /// Let logos fade in and fade out before switching to the next logo. <b>This is not a crossfade</b>
            /// </summary>
            FadeInOut
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
            /// Translational offset
            /// </summary>
            public Vector2 Offset;
            /// <summary>
            /// Sound to play
            /// </summary>
            public Sound? Sound;

            /// <summary>
            /// Create a logo with a texture and an optional sound
            /// </summary>
            public Logo(IReadableTexture texture, Vector2 offset = default, float duration = 1f, Sound? sound = null)
            {
                Texture = texture;
                Offset = offset;
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

            private static float GetAlphaForTransition(Transition transition, float time, float transitionDuration)
            {
                switch (transition)
                {
                    case Transition.FadeInOut:
                        return Utilities.Clamp(MathF.Min(time / transitionDuration, 1 - (time - 1 + transitionDuration) / transitionDuration));
                    default:
                    case Transition.Cut:
                        return 1;
                }
            }

            private void HandleSplash(SplashScreenComponent splash)
            {
                var entity = splash.Entity;

                var rect = Scene.GetComponentFrom<RectangleShapeComponent>(entity);
                var transform = Scene.GetComponentFrom<TransformComponent>(entity);

                if (rect == null || transform == null)
                    return;

                var duration = splash.Logos[splash.CurrentLogoIndex].Duration;

                splash.CurrentTime += Time.DeltaTime;
                splash.Lifetime += Time.DeltaTime;

                var progress = splash.CurrentTime / duration;
                rect.Color = new Color(1, 1, 1, GetAlphaForTransition(splash.Transition, progress, (float)splash.TransitionDuration.TotalSeconds / duration));

                if (splash.CanSkip && (Input.AnyKey || Input.AnyMouseButton))
                {
                    splash.CurrentTime = float.MaxValue;
                    splash.CurrentLogoIndex = splash.Logos.Length;
                }

                if (splash.CurrentTime > duration)
                {
                    splash.CurrentTime = 0;
                    splash.CurrentLogoIndex++;
                    if (splash.CurrentLogoIndex >= splash.Logos.Length)
                    {
                        if (splash.OnEnd != null)
                        {
                            Audio.StopAll();
                            splash.OnEnd?.Invoke();
                        }
                        else
                            Scene.RemoveEntity(entity);
                        return;
                    }

                    setLogo(splash.Logos[splash.CurrentLogoIndex]);
                }

                if (splash.Lifetime == 0)
                    setLogo(splash.Logos[0]);

                void setLogo(Logo logo)
                {
                    var texture = logo.Texture;
                    var sound = logo.Sound;

                    if (sound != null)
                        Audio.PlayOnce(sound);

                    transform.Scale = new Vector2(texture.Width, texture.Height);
                    transform.Position = logo.Offset;
                    transform.RecalculateModelMatrix(Matrix3x2.Identity);
                    rect.Material.SetUniform(ShaderDefaults.MainTextureUniform, texture);
                }
            }
        }
    }
}
