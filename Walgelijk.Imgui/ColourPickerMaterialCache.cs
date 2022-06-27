namespace Walgelijk.Imgui
{
    public class ColourPickerMaterialCache : Cache<int, Material>
    {
        protected override Material CreateNew(int raw) =>
            new Material(ColourPickerRendering.Shaders.SaturationValueMaterial);

        protected override void DisposeOf(Material loaded) =>
            Game.Main.Window.Graphics.Delete(loaded);
    }
}
