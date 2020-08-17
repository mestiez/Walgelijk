using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Basically just a stack of 4x4 matrices
    /// </summary>
    public class TransformationStack
    {
        private Stack<Matrix4x4> matrixStack = new Stack<Matrix4x4>();
        private Matrix4x4 finalMatrix;
        private bool isFinalMatrixCached = false;

        /// <summary>
        /// Pop a matrix off the stack
        /// </summary>
        public Matrix4x4 Pop()
        {
            isFinalMatrixCached = false;
            return matrixStack.Pop();
        }

        /// <summary>
        /// Push a matrix on the stack
        /// </summary>
        public void Push(Matrix4x4 matrix)
        {
            isFinalMatrixCached = false;
            matrixStack.Push(matrix);
        }

        /// <summary>
        /// Get an immutable array of all matrices in the stack
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<Matrix4x4> GetMatrices()
        {
            return matrixStack.ToImmutableArray();
        }

        /// <summary>
        /// Copies all matrices in the stack into the given buffer
        /// </summary>
        /// <returns></returns>
        public void GetMatrices(Matrix4x4[] buffer)
        {
            matrixStack.CopyTo(buffer, 0);
        }

        /// <summary>
        /// Get the final matrix. The result is cached.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetFinalMatrix()
        {
            if (isFinalMatrixCached) return finalMatrix;

            Matrix4x4 result = Matrix4x4.Identity;
            foreach (var item in matrixStack)
                result *= item;

            isFinalMatrixCached = true;
            finalMatrix = result;
            return finalMatrix;
        }
    }
}
