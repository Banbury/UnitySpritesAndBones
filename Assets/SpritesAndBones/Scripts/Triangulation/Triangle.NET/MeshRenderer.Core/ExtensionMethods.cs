// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TMeshRenderer.Core
{
    using System;
    //using System.Drawing;
	using UnityEngine;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        #region Color extention methods

        /// <summary>
        /// Converts a Color to a float array containing normalized R, G ,B, A values.
        /// </summary>
        public static float[] ToFloatArray4(this Color color)
        {
            return new float[] {
                ((float)color.r) / 255.0f,
                ((float)color.g) / 255.0f,
                ((float)color.b) / 255.0f,
                ((float)color.a) / 255.0f
            };
        }

        #endregion
    }
}
