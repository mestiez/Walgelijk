using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Walgelijk.Entities
{
    /// <summary>
    /// Stores and manages components and systems
    /// </summary>
    public sealed class Scene
    {
        // TODO alles hier. maak een manier om components uit entities te halen en alles en alles en alles
    }

    public interface ISystem
    {
        void Execute();
        void Render();
    }

    public struct Entity
    {
        public int Identity { get; set; }
    }

    public struct TransformComponent
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
    }
}
