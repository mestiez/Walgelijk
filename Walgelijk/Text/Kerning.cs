namespace Walgelijk
{
    /// <summary>
    /// Structure with kerning information
    /// </summary>
    public struct Kerning
    {
        /// <summary>
        /// Preceding character
        /// </summary>
        public char FirstChar;
        /// <summary>
        /// Current character
        /// </summary>
        public char SecondChar;
        /// <summary>
        /// Offset amount
        /// </summary>
        public int Amount;
    }
}
