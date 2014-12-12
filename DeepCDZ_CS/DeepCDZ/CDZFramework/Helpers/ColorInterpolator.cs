using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZFramework
{
    public static class ColorInterpolator
    {
        delegate byte ComponentSelector(Color color);
        static ComponentSelector _redSelector = color => color.R;
        static ComponentSelector _greenSelector = color => color.G;
        static ComponentSelector _blueSelector = color => color.B;

        public static Color InterpolateBetween(
            Color endPoint1,
            Color endPoint2,
            Color endPoint3,
            double lambda)
        {
            if (lambda < 0)
                lambda = 0;
            if (lambda > 1)
                lambda = 1;

            if (lambda < 0.5)
                return InterpolateBetween(endPoint1, endPoint2, lambda * 2);
            else
                return InterpolateBetween(endPoint2, endPoint3, lambda / 2.0);
        }

        public static Color InterpolateBetween(
            Color endPoint1,
            Color endPoint2,
            double lambda)
        {
            if (lambda < 0)
                lambda = 0;
            if (lambda > 1)
                lambda = 1;

            Color color = Color.FromArgb(
                InterpolateComponent(endPoint1, endPoint2, lambda, _redSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _greenSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _blueSelector)
            );

            return color;
        }

        static byte InterpolateComponent(
            Color endPoint1,
            Color endPoint2,
            double lambda,
            ComponentSelector selector)
        {
            return (byte)(selector(endPoint1)
                + (selector(endPoint2) - selector(endPoint1)) * lambda);
        }
    }
}
