namespace Walgelijk
{
    /// <summary>
    /// Generates entity identities
    /// </summary>
    public struct IdentityGenerator
    {
        private static int lastIdentity = -1;

        public static int Generate()
        {
            lastIdentity++;
            return lastIdentity;
        }
    }
}
