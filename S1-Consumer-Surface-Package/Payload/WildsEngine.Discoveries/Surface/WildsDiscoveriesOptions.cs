using System;

namespace WildsEngine.Discoveries.Surface
{
    public sealed class WildsDiscoveriesOptions
    {
        private double _geometryEpsilon = 1e-10;

        public double GeometryEpsilon
        {
            get => _geometryEpsilon;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value < 0.0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _geometryEpsilon = value;
            }
        }
    }
}
