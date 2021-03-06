﻿using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet;
using SolidWorks.Interop.sldworks;

namespace SolidworksAddinFramework
{
    public static class SurfaceExtensions
    {
        /// <summary>
        /// Represents a point on a surface. 
        /// (X,Y,Z) and (U,V)
        /// </summary>
        public class PointUv
        {
            public double X { get; }

            public double Y { get; }

            public double Z { get; }

            public double U { get; }

            public double V { get; }

            public double[] Point => new[] {X, Y, Z};
            public double[] UV => new[] {U, V};

            public PointUv(double x, double y, double z, double u, double v)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
                this.U = u;
                this.V = v;
            }
        }

        /// <summary>
        /// Gets the closest point on a surface to the input point
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static PointUv GetClosestPointOnTs(this ISurface surface, double[] p)
        {
            var x = p[0];
            var y = p[1];
            var z = p[2];

            return ClosestPointOnTs(surface, x, y, z);
        }

        /// <summary>
        /// Gets the closest point on a surface to the input point
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static PointUv ClosestPointOnTs(this ISurface surface, double x, double y, double z)
        {
            var r = (double[]) surface.GetClosestPointOn(x, y, z);
            return new PointUv(r[0], r[1], r[2], r[3], r[4]);
        }
    }

}
