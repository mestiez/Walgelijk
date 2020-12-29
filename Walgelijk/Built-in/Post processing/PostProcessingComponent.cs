using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// A post processing effect container
    /// </summary>
    public class PostProcessingComponent
    {
        /// <summary>
        /// Ordered collection of effects
        /// </summary>
        public List<IPostProcessingEffect> Effects { get; set; } = new List<IPostProcessingEffect>();
    }
}
