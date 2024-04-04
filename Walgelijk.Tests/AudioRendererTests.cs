using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using Walgelijk;
using Walgelijk.Mock;
using Walgelijk.OpenTK;

namespace Tests;

[TestClass]
public class AudioRendererTests
{
    public readonly static Func<AudioRenderer>[] Renderers =
    {
        () => new OpenALAudioRenderer()
        {
            Volume = 0 // try not to deafen the programmer ♥
        },
    };

    [TestMethod]
    public void FixedAudioLifetime()
    {
        foreach (var createNew in Renderers)
        {
            var renderer = createNew();
            {
                var audioData = renderer.LoadSound("machine_blaster-01.wav");

                Assert.IsNotNull(audioData, "Loaded audio data is null");
                Assert.IsTrue(audioData.Duration > TimeSpan.Zero, "Loaded audio data duration is zero");

                var sound = new Sound(audioData);

                Assert.IsNotNull(sound, "Loaded sound is null");
                Assert.AreEqual(audioData, sound.Data, "Loaded sound and loaded audio data don't match");
                Assert.AreEqual(SoundState.Idle, sound.State);

                bool started = false;

                var t = TimeSpan.Zero;
                while (t <= audioData.Duration - TimeSpan.FromMilliseconds(500))
                {
                    if (!started)
                    {
                        renderer.Play(sound);
                        started = true;
                    }

                    var dt = TimeSpan.FromSeconds(1 / 30f);
                    t += dt;

                    renderer.Process((float)dt.TotalSeconds);
                    Assert.AreEqual(SoundState.Playing, sound.State);
                    Assert.IsTrue(renderer.IsPlaying(sound));
                    Thread.Sleep(dt);
                }

                renderer.DisposeOf(sound);
                renderer.DisposeOf(audioData);
            }
            if (renderer is IDisposable d)
                d.Dispose();
        }
    }

    [TestMethod]
    public void ManyTemporarySources()
    {
        foreach (var createNew in Renderers)
        {
            var renderer = createNew();
            {
                var audioData1 = renderer.LoadSound("machine_blaster-01.wav");
                var audioData2 = renderer.LoadSound("machine_blaster-02.wav");
                var audioData3 = renderer.LoadSound("machine_blaster-03.wav");

                var sounds = new[] { new Sound(audioData1), new Sound(audioData2), new Sound(audioData3) };
                var distr = new FixedIntervalDistributor(30);

                var t = TimeSpan.Zero;
                while (t <= TimeSpan.FromSeconds(5) + sounds.Max(s => s.Data.Duration))
                {
                    var dt = TimeSpan.FromSeconds(1 / 30f);
                    t += dt;

                    if (t <= TimeSpan.FromSeconds(5))
                        for (int i = 0; i < distr.CalculateCycleCount((float)dt.TotalSeconds); i++)
                            renderer.PlayOnce(Utilities.PickRandom(sounds));

                    renderer.Process((float)dt.TotalSeconds);

                    Thread.Sleep(dt);
                }

                foreach (var s in sounds)
                    renderer.DisposeOf(s);
            }
            if (renderer is IDisposable d)
                d.Dispose();
        }
    }
}