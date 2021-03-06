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
using System.Text.RegularExpressions;
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;
using Sidi.Test;

namespace hagen
{
    internal class TextPosition
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Regex regex = new Regex(new[]
        {
            (PathPattern + @"\((?<Line>\d+)\,(?<Column>\d+)\)"),
            (PathPattern + @"\((?<Line>\d+)\)"),
            (PathPattern + @"\:line\ (?<Line>\d+)"),
            (@"{0}".F(PathPattern)),
        }.Join("|"));

        static string PathPattern
        {
            get
            {
                return @"\b(?<Path>[a-zA-Z]\:[^\s:""]+)";
                /*
                return "[^" + (System.IO.Path.GetInvalidPathChars()
                    .Select(c => Regex.Escape(new string(c, 1)))
                    .Join(""))
                    + "]+";
                 */
            }
        }

        static TextPosition FromRegex(Match m)
        {
            return new TextPosition()
            {
                Path = ResolvePath(GetValue(m, "Path", String.Empty)),
                Line = Int32.Parse(GetValue(m, "Line", "1")),
                Column = Int32.Parse(GetValue(m, "Column", "1")),
            };
        }

        static string ResolvePath(string p)
        {
            p = p.Replace("/", @"\");

            if (p.StartsWith(@"\n4"))
            {
                return @"Q:" + p;
            }
            
            return p;
        }

        static string GetValue(Match m, string key, string defaultValue)
        {
            var g = m.Groups[key];
            if (g.Success)
            {
                return g.Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public LPath Path;
        public int Line = 1;
        public int Column = 1;

        public override string ToString()
        {
            if (Line <= 1 && Column <= 1)
            {
                return String.Format("{0}", Path);
            }
            else
            {
                return String.Format("{0}({1},{2})", Path, Line, Column);
            }
        }

        public static IEnumerable<TextPosition> Extract(string query)
        {
            return regex.Matches(query).Cast<Match>()
                .SafeSelect(m => TextPosition.FromRegex(m));
        }
    }
}
