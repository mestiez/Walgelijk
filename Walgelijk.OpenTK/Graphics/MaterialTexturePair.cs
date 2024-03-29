﻿using System;

namespace Walgelijk.OpenTK
{
    public struct MaterialTexturePair : IEquatable<MaterialTexturePair>
    {
        public LoadedMaterial Material;
        public LoadedTexture Texture;
        public int UniformLocation;

        public MaterialTexturePair(LoadedMaterial material, LoadedTexture texture, int uniformLocation)
        {
            Material = material;
            Texture = texture;
            UniformLocation = uniformLocation;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialTexturePair pair &&
                   pair.Material.ProgramHandle == Material.ProgramHandle &&
                   pair.UniformLocation == UniformLocation &&
                   pair.Texture.Handle == Texture.Handle;
        }

        public bool Equals(MaterialTexturePair other)
        {
            return other.Material.ProgramHandle == Material.ProgramHandle &&
                   other.UniformLocation == UniformLocation &&
                   other.Texture.Handle == Texture.Handle;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Material, Texture);
        }

        public static bool operator ==(MaterialTexturePair left, MaterialTexturePair right)
        {
            return left.Material.ProgramHandle == right.Material.ProgramHandle &&
                   left.UniformLocation == right.UniformLocation &&
                   left.Texture.Handle == right.Texture.Handle;
        }

        public static bool operator !=(MaterialTexturePair left, MaterialTexturePair right)
        {
            return !(left == right);
        }
    }
}
