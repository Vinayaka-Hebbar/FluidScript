namespace FluidScript
{
    /// <summary>
    /// Provides constants and static methods for trigonometric, logarithmic, and other
    /// common mathematical functions.
    /// </summary>
    public sealed class Math : FSObject
    {
        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// </summary>
        /// <param name="x"> A double-precision floating-point number to be raised to a power.</param>
        /// <param name="y">A double-precision floating-point number that specifies a power.</param>
        /// <returns>The number x raised to the power y.</returns>
        [Runtime.Register("pow")]
        public static Double Pow(Double x, Double y)
        {
            return new Double(System.Math.Pow(x.m_value, y.m_value));
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <param name="d"> A number.</param>
        /// <returns>
        /// One of the values in the following table. d parameter Return value Zero, or positive
        ///The positive square root of d. Negative System.Double.NaNEquals System.Double.NaNSystem.Double.NaNEquals
        ///System.Double.PositiveInfinitySystem.Double.PositiveInfinity
        ///</returns>
        [Runtime.Register("sqrt")]
        public static Double Sqrt(Double d)
        {
            return new Double(System.Math.Sqrt(d.m_value));
        }

        /// <summary>
        /// Returns the sine of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <returns>
        /// The sine of a. If a is equal to System.Double.NaN, System.Double.NegativeInfinity,
        /// or System.Double.PositiveInfinity, this method returns System.Double.NaN.
        /// </returns>
        [Runtime.Register("sin")]
        public static Double Sin(Double a)
        {
            return new Double(System.Math.Sin(a.m_value));
        }

        /// <summary>
        /// Returns the angle whose sine is the specified number.
        /// </summary>
        /// <param name="d">
        /// A number representing a sine, where d must be greater than or equal to -1, but
        /// but less than or equal to 1.
        /// </param>
        /// <returns>
        /// An angle, θ, measured in radians, such that 0 ≤θ≤π-or- System.Double.NaN if d
        /// &lt; -1 or d &gt; 1 or d equals System.Double.NaN.
        /// </returns>
        [Runtime.Register("asin")]
        public static Double Asin(Double d)
        {
            return new Double(System.Math.Asin(d.m_value));
        }

        /// <summary>
        /// Returns the hyperbolic sine of the specified angle.
        /// </summary>
        /// <param name="value">An angle, measured in radians.</param>
        /// <returns>
        /// The hyperbolic sine of value. If value is equal to System.Double.NegativeInfinity,
        /// System.Double.PositiveInfinity, or System.Double.NaN, this method returns a System.Double
        /// equal to value.
        /// </returns>
        [Runtime.Register("sinh")]
        public static Double Sinh(Double value)
        {
            return new Double(System.Math.Sinh(value.m_value));
        }

        /// <summary>
        /// Returns the cosine of the specified angle.
        /// </summary>
        /// <param name="d">An angle, measured in radians.</param>
        /// <returns>
        /// The cosine of d. If d is equal to System.Double.NaN, System.Double.NegativeInfinity,
        ///  or System.Double.PositiveInfinity, this method returns System.Double.NaN.
        /// </returns>
        [Runtime.Register("cos")]
        public static Double Cos(Double d)
        {
            return new Double(System.Math.Cos(d.m_value));
        }

        /// <summary>
        /// Returns the angle whose cosine is the specified number.
        /// </summary>
        /// <param name="d">
        /// A number representing a cosine, where d must be greater than or equal to -1,
        /// but less than or equal to 1.
        /// </param>
        /// <returns>
        /// An angle, θ, measured in radians, such that 0 ≤θ≤π-or- System.Double.NaN if d
        /// &lt; -1 or d &gt; 1 or d equals System.Double.NaN.
        /// </returns>
        [Runtime.Register("acos")]
        public static Double Acos(Double d)
        {
            return new Double(System.Math.Acos(d.m_value));
        }

        /// <summary>
        /// Returns the hyperbolic cosine of the specified angle.
        /// </summary>
        /// <param name="value">An angle, measured in radians.</param>
        /// <returns>
        /// The hyperbolic cosine of value. If value is equal to System.Double.NegativeInfinity
        /// or System.Double.PositiveInfinity, System.Double.PositiveInfinity is returned.
        /// If value is equal to System.Double.NaN, System.Double.NaN is returned.
        /// </returns>
        [Runtime.Register("cosh")]
        public static Double Cosh(Double value)
        {
            return new Double(System.Math.Cosh(value.m_value));
        }

        /// <summary>
        /// Returns the tangent of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <returns>
        /// The tangent of a. If a is equal to System.Double.NaN, System.Double.NegativeInfinity,
        /// or System.Double.PositiveInfinity, this method returns System.Double.NaN.
        /// </returns>
        [Runtime.Register("tan")]
        public static Double Tan(Double a)
        {
            return new Double(System.Math.Tan(a.m_value));
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified number.
        /// </summary>
        /// <param name="d"> A number representing a tangent.</param>
        /// <returns>
        /// An angle, θ, measured in radians, such that -π/2 ≤θ≤π/2.-or- System.Double.NaN
        /// if d equals System.Double.NaN, -π/2 rounded to double precision (-1.5707963267949)
        /// if d equals System.Double.NegativeInfinity, or π/2 rounded to double precision
        /// (1.5707963267949) if d equals System.Double.PositiveInfinity.
        /// </returns>
        [Runtime.Register("atan")]
        public static Double Atan(Double d)
        {
            return new Double(System.Math.Atan(d.m_value));
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle.
        /// </summary>
        /// <param name="value">An angle, measured in radians.</param>
        /// <returns>
        /// The hyperbolic tangent of value. If value is equal to System.Double.NegativeInfinity,
        /// this method returns -1. If value is equal to System.Double.PositiveInfinity,
        /// this method returns 1. If value is equal to System.Double.NaN, this method returns
        /// System.Double.NaN.
        /// </returns>
        [Runtime.Register("tanh")]
        public static Double Tanh(Double value)
        {
            return new Double(System.Math.Tan(value.m_value));
        }
        
        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional
        /// digits.
        /// </summary>
        /// <param name="value">A double-precision floating-point number to be rounded.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <returns>
        /// The number nearest to value that contains a number of fractional digits equal
        /// to digits.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// digits is less than 0 or greater than 15.
        /// </exception>
        [Runtime.Register("round")]
        public static Double Round(Double value, Integer digits)
        {
            return new Double(System.Math.Round(value.m_value, digits.m_value));
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified
        /// double-precision floating-point number.
        /// </summary>
        /// <param name="a">A double-precision floating-point number.</param>
        /// <returns>
        /// The smallest integral value that is greater than or equal to a. If a is equal
        /// to System.Double.NaN, System.Double.NegativeInfinity, or System.Double.PositiveInfinity,
        /// that value is returned. Note that this method returns a System.Double instead
        /// of an integral type.
        /// </returns>
        [Runtime.Register("ceiling")]
        public static Double Ceiling(Double a)
        {
            return new Double(System.Math.Ceiling(a.m_value));
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified double-precision
        /// floating-point number.
        /// </summary>
        /// <param name="d">A double-precision floating-point number.</param>
        /// <returns>
        /// The largest integer less than or equal to d. If d is equal to System.Double.NaN,
        /// System.Double.NegativeInfinity, or System.Double.PositiveInfinity, that value
        /// is returned.
        /// </returns>
        [Runtime.Register("floor")]
        public static Double Floor(Double d)
        {
            return new Double(System.Math.Floor(d.m_value));
        }

        /// <summary>
        ///  Returns e raised to the specified power.
        /// </summary>
        /// <param name="d">A number specifying a power.</param>
        /// <returns>
        /// The number e raised to the power d. If d equals System.Double.NaN or System.Double.PositiveInfinity,
        /// that value is returned. If d equals System.Double.NegativeInfinity, 0 is returned.
        /// </returns>
        [Runtime.Register("exp")]
        public static Double Exp(Double d)
        {
            return new Double(System.Math.Exp(d.m_value));
        }

        /// <summary>
        /// Calculates the integral part of a specified double-precision floating-point number.
        /// </summary>
        /// <param name="d">A number to truncate.</param>
        /// <returns>
        /// The integral part of d; that is, the number that remains after any fractional
        /// digits have been discarded, or one of the values listed in the following table.
        /// dReturn 
        /// </returns>
        [Runtime.Register("truncate")]
        public static Double Truncate(Double d)
        {
            return new Double(System.Math.Truncate(d.m_value));
        }
    }
}
