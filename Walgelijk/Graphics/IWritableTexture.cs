namespace Walgelijk
{
    /// <summary>
    /// A texture that can be written to
    /// </summary>
    public interface IWritableTexture
    {
        /// <summary>
        /// Set a pixel to a colour
        /// </summary>
        public void SetPixel(int x, int y, Color color);
    }    
}
