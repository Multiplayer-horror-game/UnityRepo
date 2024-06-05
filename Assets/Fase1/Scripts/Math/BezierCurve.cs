

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fase1
{
    public class BezierCurve
    {
        public static Vector2 CalculateBezierPoint(float t, Vector2[] nodes) {
            float x = (float)CalculateCoordinate(t, 0, nodes);
            float y = (float)CalculateCoordinate(t, 1, nodes);
            return new Vector3(x, y);
        }

        public static double CalculateCoordinate(float t, int axis, Vector2[] nodes){
            double result = 0;
            int n = nodes.Length - 1;

            for (int i = 0; i <= n; i++) {
                double binomialCoefficient = CalculateBinomialCoefficient(n, i);
                double term = binomialCoefficient * MathF.Pow(1 - t, n - i) * System.Math.Pow(t, i);

                double coordinate = axis switch
                {
                    0 => nodes[i].x,
                    1 => nodes[i].y,
                    _ => 0
                };

                // Access the coordinates using getX(), getY(), and getZ()

                result += term * coordinate;
            }

            return result;
        }
        
        public static double CalculateCoordinate1D(float t,  float[] nodes){
            double result = 0;
            int n = nodes.Length - 1;

            for (int i = 0; i <= n; i++) {
                double binomialCoefficient = CalculateBinomialCoefficient(n, i);
                double term = binomialCoefficient * MathF.Pow(1 - t, n - i) * System.Math.Pow(t, i);

                result += term;
            }

            return result;
        }

        public static double CalculateBinomialCoefficient(int n, int k) {
            return (double) Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        public static int Factorial(int n) {
            if (n <= 1) {
                return 1;
            }
            return n * Factorial(n - 1);
        }

        public static Vector2 DeCasteljau(List<Vector2> controlPoints, float t) {
            List<Vector2> points = new List<Vector2>(controlPoints);

            while (points.Count > 1) {
                List<Vector2> newPoints = new();

                for (int i = 0; i < points.Count - 1; i++)
                {
                    newPoints.Add(Interpolate(points[i], points[i + 1],t));
                }
                
                points = newPoints;
            }

            return points.First();
        }
        
        public static Vector2 Interpolate(Vector2 begin, Vector2 end, float t) {
            float xInterpolated = (float)begin.x + (end.x - begin.x) * t;
            float yInterpolated = (float)begin.y + (end.y - begin.y) * t;
            return new Vector2(xInterpolated, yInterpolated);
        }
    }
}