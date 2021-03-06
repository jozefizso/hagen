// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZedGraph;

namespace activityReport
{
    public static class PointPairListExtension
    {
        public static PointPairList Average(this PointPairList d, TimeSpan window)
        {
            return d.Average(window.ToXDate().XLDate);
        }
        
        public static PointPairList Average(this PointPairList d, double window)
        {
            PointPairList integral = Integrate(d);
            PointPairList a = new PointPairList();
            for (int i = 0; i < d.Count; ++i)
            {
                double x = d[i].X;
                a.Add(x, (integral.InterpolateX(x) - integral.InterpolateX(x - window)) / window);
            }
            return a;
        }

        public static PointPairList Quantile(this PointPairList d, int count, double quantile)
        {
            PointPairList a = new PointPairList();
            for (int i = 0; i < d.Count; ++i)
            {
                List<double> v = new List<double>();
                for (int j = i; j >= Math.Max(0, i - count); --j)
                {
                    v.Add(d[j].Y);
                }
                v.Sort();
                a.Add(d[i].X, v[Math.Max(0,Math.Min(v.Count-1, (int)(quantile*count)))]);
            }
            return a;
        }

        public static PointPairList ProbabilityDensityFunction(this PointPairList d)
        {
            List<double> v = new List<double>();
            for (int i = 0; i < d.Count; ++i)
            {
                v.Add(d[i].Y);
            }
            v.Sort();
            PointPairList a = new PointPairList();
            for (int i = 0; i < v.Count; ++i)
            {
                a.Add(v[i], (double)i / (double)v.Count);
            }
            return a;
        }

        public static PointPairList Integrate(this PointPairList d)
        {
            PointPairList a = new PointPairList();
            double s = d[0].X * d[0].Y;
            s = 0;
            a.Add(d[0].X, s);
            for (int i = 1; i < d.Count; ++i)
            {
                s += (d[i].X - d[i - 1].X) * d[i].Y;
                a.Add(d[i].X, s);
            }
            return a;
        }

        public static PointPairList Accumulate(this PointPairList d)
        {
            PointPairList a = new PointPairList();
            double s = 0;
            for (int i = 1; i < d.Count; ++i)
            {
                s += d[i].Y;
                a.Add(d[i].X, s);
            }
            return a;
        }

        public static PointPairList Detrend(this PointPairList d, double dx, double dy)
        {
            PointPair p0 = d.Last();

            return d.Map(x =>
                {
                    x.Y -= p0.Y + (x.X - p0.X) * (dy / dx);
                });
        }

        public static PointPairList SumY(this PointPairList d)
        {
            PointPairList a = new PointPairList();
            double s = 0;
            for (int i = 0; i < d.Count; ++i)
            {
                s += d[i].Y;
                a.Add(d[i].X, s);
            }
            return a;
        }

        public static PointPairList Derive(this PointPairList d)
        {
            PointPairList derivation = new PointPairList();
            for (int i = 1; i < d.Count; ++i)
            {
                double den = (d[i].X - d[i - 1].X);
                if (den != 0)
                {
                    derivation.Add(d[i].X, (d[i].Y - d[i - 1].Y) / den);
                }
            }
            return derivation;
        }

        public delegate void MapFunction(PointPair x);
        
        public static PointPairList Map(this PointPairList d, MapFunction f)
        {
            PointPairList result = new PointPairList(d);
            for (int i = 0; i < d.Count; ++i)
            {
                f(result[i]);
            }
            return result;
        }

        public static PointPairList RemoveOffset(this PointPairList x)
        {
            double offset = x[0].X;
            return Map(x, delegate(PointPair p)
            {
                p.X = p.X - offset;
            });
        }

        public static PointPairList ToPointPairList(this IEnumerable<PointPair> input)
        {
            var ppl = new PointPairList();
            ppl.AddRange(input);
            return ppl;
        }

        public static XDate ToXDate(this TimeSpan dt)
        {
            DateTime t = new DateTime(2009, 1, 1);
            return new XDate(new XDate(t + dt).XLDate - new XDate(t).XLDate);
        }
    }
}
