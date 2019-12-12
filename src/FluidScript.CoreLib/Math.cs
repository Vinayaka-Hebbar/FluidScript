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
    }
}
