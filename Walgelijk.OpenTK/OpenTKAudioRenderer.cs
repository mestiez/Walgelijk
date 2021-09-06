using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.OpenTK
{
    public class OpenTKAudioRenderer : AudioRenderer
    {
        public OpenTKAudioRenderer()
        {
            //global::OpenTK.Audio.OpenAL.AL.Enable(global::OpenTK.Audio.OpenAL.ALCapability.Invalid)
        }

        public override float Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool Muted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Vector2 ListenerPosition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Sound LoadSound(string path)
        {
            throw new NotImplementedException();
        }

        public override void Pause(ref Sound sound)
        {
            throw new NotImplementedException();
        }

        public override void Play(ref Sound sound)
        {
            throw new NotImplementedException();
        }

        public override void Play(ref Sound sound, Vector2 worldPosition)
        {
            throw new NotImplementedException();
        }

        public override void PlayOnce(Sound sound)
        {
            throw new NotImplementedException();
        }

        public override void PlayOnce(Sound sound, Vector2 worldPosition)
        {
            throw new NotImplementedException();
        }

        public override void Stop(ref Sound sound)
        {
            throw new NotImplementedException();
        }

        public override void StopAll()
        {
            throw new NotImplementedException();
        }
    }
}
