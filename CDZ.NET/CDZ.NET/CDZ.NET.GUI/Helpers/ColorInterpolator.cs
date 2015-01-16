using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET
{
    public static class ColorHelper
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
        public static double RGB2GRAYd(Color c)
        {
            return (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B);
        }
        public static float RGB2GRAYf(Color c)
        {
            return (0.2126f * c.R + 0.7152f * c.G + 0.0722f * c.B);
        }

        public static Color RGB2GRAYc(Color c)
        {
            int f = (int)ColorHelper.RGB2GRAYf(c);
            return Color.FromArgb(f,f,f);
        }
    }
}
