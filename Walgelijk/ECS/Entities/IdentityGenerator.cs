namespace Walgelijk
{
    /// <summary>
    /// Generates entity identities
    /// </summary>
    public struct IdentityGenerator
    {
        private static int i = 0;

        /// <summary>
        /// Generate a new identity value
        /// </summary>
        public static int Generate()
        {
            return i++;// Utilities.RandomInt(int.MinValue, int.MaxValue);
        }
    }
}
