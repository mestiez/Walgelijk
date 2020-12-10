namespace Walgelijk
{
    /// <summary>
    /// Generates entity identities
    /// </summary>
    public struct IdentityGenerator
    {
        /// <summary>
        /// Generate a new identity value
        /// </summary>
        /// <returns></returns>
        public static int Generate()
        {
            return Utilities.RandomInt(int.MinValue, int.MaxValue);
        }
    }
}
