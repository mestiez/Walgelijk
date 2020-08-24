namespace Walgelijk
{
    /// <summary>
    /// Generates entity identities
    /// </summary>
    public struct IdentityGenerator
    {
        private static int lastIdentity = -1;

        /// <summary>
        /// Generate a new identity value
        /// </summary>
        /// <returns></returns>
        public static int Generate()
        {
            lastIdentity++;
            return lastIdentity;
        }
    }
}
