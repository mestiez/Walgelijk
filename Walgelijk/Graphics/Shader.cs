﻿using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Structure that holds the code for a full shader program
    /// </summary>
    [Serializable]
    public class Shader
    {
        /// <summary>
        /// Create a shader from vertex and fragment shader code
        /// </summary>
        /// <param name="vertexShader"></param>
        /// <param name="fragmentShader"></param>
        public Shader(string vertexShader, string fragmentShader)
        {
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;
        }

        /// <summary>
        /// Create an empty shader
        /// </summary>
        public Shader() { }

        /// <summary>
        /// The vertex shader of the program
        /// </summary>
        public string VertexShader { get; set; }

        /// <summary>
        /// The fragment shader of the program
        /// </summary>
        public string FragmentShader { get; set; }

        /// <summary>
        /// Load shader from files
        /// </summary>
        /// <param name="vertPath"></param>
        /// <param name="fragPath"></param>
        /// <returns></returns>
        public Shader Load(string vertPath, string fragPath)
        {
            string vert = File.ReadAllText(vertPath);
            string frag = File.ReadAllText(fragPath);
            return new Shader(vert, frag);
        }

        /// <summary>
        /// Default shader. Renders vertex colours.
        /// </summary>
        public static Shader Default
        {
            get
            {
                return new Shader
                {
                    VertexShader = ShaderConstants.DefaultVertex,
                    FragmentShader = ShaderConstants.DefaultFragment
                };
            }
        }
    }
}