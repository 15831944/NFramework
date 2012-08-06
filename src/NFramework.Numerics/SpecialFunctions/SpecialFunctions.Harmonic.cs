﻿using System;

namespace NSoft.NFramework.Numerics {
    public static partial class SpecialFunctions {
        /// <summary>
        /// Computes the <paramref name="t"/>'th Harmonic number.
        /// </summary>
        /// <param name="t">The Harmonic number which needs to be computed.</param>
        /// <returns>The t'th Harmonic number.</returns>
        public static double Harmonic(int t) {
            return MathTool.EulerMascheroni + DiGamma(t + 1.0);
        }

        /// <summary>
        /// Compute the generalized harmonic number of order n of m. (1 + 1/2^m + 1/3^m + ... + 1/n^m)
        /// </summary>
        /// <param name="n">The order parameter.</param>
        /// <param name="m">The power parameter.</param>
        /// <returns>General Harmonic number.</returns>
        public static double GeneralHarmonic(int n, double m) {
            double sum = 0;
            for(var i = 0; i < n; i++) {
                sum += Math.Pow(i + 1, -m);
            }
            return sum;
        }

        /// <summary>
        /// Computes the Digamma function which is mathematically defined as the derivative of the logarithm of the gamma function.
        /// This implementation is based on
        ///     Jose Bernardo
        ///     Algorithm AS 103:
        ///     Psi ( Digamma ) Function,
        ///     Applied Statistics,
        ///     Volume 25, Number 3, 1976, pages 315-317.
        /// Using the modifications as in Tom Minka's lightspeed toolbox.
        /// </summary>
        /// <param name="x">The argument of the digamma function.</param>
        /// <returns>The value of the DiGamma function at <paramref name="x"/>.</returns>
        public static double DiGamma(double x) {
            const double C = 12.0;
            const double D1 = -0.57721566490153286;
            const double D2 = 1.6449340668482264365;
            const double S = 1e-6;
            const double S3 = 1.0 / 12.0;
            const double S4 = 1.0 / 120.0;
            const double S5 = 1.0 / 252.0;
            const double S6 = 1.0 / 240.0;
            const double S7 = 1.0 / 132.0;

            if(Double.IsNegativeInfinity(x) || Double.IsNaN(x)) {
                return Double.NaN;
            }

            // Handle special cases.
            if(x <= 0 && Math.Abs(Math.Floor(x) - x) < double.Epsilon) {
                return Double.NegativeInfinity;
            }

            // Use inversion formula for negative numbers.
            if(x < 0) {
                return DiGamma(1.0 - x) + (MathTool.Pi / Math.Tan(-MathTool.Pi * x));
            }

            if(x <= S) {
                return D1 - (1 / x) + (D2 * x);
            }

            double result = 0;
            while(x < C) {
                result -= 1 / x;
                x++;
            }

            if(x >= C) {
                var r = 1 / x;
                result += Math.Log(x) - (0.5 * r);
                r *= r;

                result -= r * (S3 - (r * (S4 - (r * (S5 - (r * (S6 - (r * S7))))))));
            }

            return result;
        }

        /// <summary>
        /// <para>Computes the inverse Digamma function: this is the inverse of the logarithm of the gamma function. This function will
        /// only return solutions that are positive.</para>
        /// <para>This implementation is based on the bisection method.</para>
        /// </summary>
        /// <param name="p">The argument of the inverse digamma function.</param>
        /// <returns>The positive solution to the inverse DiGamma function at <paramref name="p"/>.</returns>
        public static double DiGammaInv(double p) {
            if(Double.IsNaN(p)) {
                return Double.NaN;
            }

            if(Double.IsNegativeInfinity(p)) {
                return 0.0;
            }

            if(Double.IsPositiveInfinity(p)) {
                return Double.PositiveInfinity;
            }

            var x = Math.Exp(p);
            for(var d = 1.0; d > 1.0e-15; d /= 2.0) {
                x += d * Math.Sign(p - DiGamma(x));
            }

            return x;
        }

        /// <summary>
        /// Computes the logit function. see: http://en.wikipedia.org/wiki/Logit
        /// </summary>
        /// <param name="p">The parameter for which to compute the logit function. This number should be
        /// between 0 and 1.</param>
        /// <returns>The logarithm of <paramref name="p"/> divided by 1.0 - <paramref name="p"/>.</returns>
        public static double Logit(double p) {
            p.ShouldBeBetween(0.0, 1.0, "p");

            return Math.Log(p / (1.0 - p));
        }

        /// <summary>
        /// Computes the logistic function. see: http://en.wikipedia.org/wiki/Logistic
        /// </summary>
        /// <param name="p">The parameter for which to compute the logistic function.</param>
        /// <returns>The logistic function of <paramref name="p"/>.</returns>
        public static double Logistic(double p) {
            return 1.0 / (Math.Exp(-p) + 1.0);
        }
    }
}