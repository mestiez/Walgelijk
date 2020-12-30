﻿using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// A post processing effect container
    /// </summary>
    public class PostProcessingComponent
    {
        /// <summary>
        /// The render order at which to start applying effects
        /// </summary>
        public RenderOrder Begin { get; set; } = DefaultLayers.CameraOperations.WithOrder(1);

        /// <summary>
        /// The render order at which to stop applying effects
        /// </summary>
        public RenderOrder End { get; set; } = DefaultLayers.UI.WithOrder(-1);

        /// <summary>
        /// Ordered collection of effects
        /// </summary>
        public List<IPostProcessingEffect> Effects { get; set; } = new List<IPostProcessingEffect>();

        /// <summary>
        /// The task that applies the effects
        /// </summary>
        public ActionRenderTask EffectTask { get; internal set; } = null;
    }
}
