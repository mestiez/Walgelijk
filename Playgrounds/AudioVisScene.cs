using System.Numerics;
using Walgelijk;
using Walgelijk.Onion;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public struct AudioVisScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new AudioVisSystem());
        scene.AddSystem(new OnionSystem());
        game.UpdateRate = 0;
        game.FixedUpdateRate = 8;
        return scene;
    }

    public class AudioVisSystem : Walgelijk.System
    {
        private AudioVisualiser? vis;

        private (string Name, Sound Sound)[] music =
        {
            LoadMusic("panning_test.ogg"),
            LoadMusic("mc4.ogg"),
            LoadMusic("warning_party_imminent.ogg"),
        };

        static (string, Sound) LoadMusic(string file) => (file, new Sound(Resources.Load<StreamAudioData>(file), false));

        public override void Update()
        {
            Ui.Layout.FitWidth(false).Height(48).HorizontalLayout();
            Ui.StartScrollView();
            {
                foreach (var track in music)
                {
                    Ui.Layout.FitHeight().Width(100).StickTop();
                    if (Ui.Button(track.Name, track.Sound.GetHashCode()))
                    {
                        Audio.StopAll();
                        vis = new AudioVisualiser(track.Sound, fftSize: 512, bufferSize: 1024, barCount: 64);
                        //Audio.SetTime(track.Sound, 0);
                        Audio.Play(track.Sound);
                    }
                }
            }
            Ui.End();

            Draw.Reset();
            Draw.ScreenSpace = true;

            Draw.Colour = Colors.Black;
            Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

            if (vis != null)
            {
                vis.Update(Audio, Time.DeltaTime);
                float x = 0;
                float w = Window.Width / vis.BarCount;
                foreach (var item in vis.GetVisualiserData())
                {
                    Draw.Colour = Color.FromHsv(0.7f + item * 0.2f, 1, 1);
                    var bar = (1 - item) * Window.Height;
                    Draw.Quad(new Rect(x, Window.Height, x + w, (1 - item) * Window.Height).SortComponents());
                    //Draw.Line(new Vector2(x, bar), new Vector2(x + w, bar), 5);
                    x += w;
                }
            }
        }
    }
}
