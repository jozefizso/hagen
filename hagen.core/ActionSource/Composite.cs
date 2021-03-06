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
using System.Reflection;
using Sidi.CommandLine;
using Sidi.Extensions;
using Sidi.IO;
using Sidi.Util;
using System.Reactive.Linq;

namespace hagen.ActionSource
{
    public class Composite : IActionSource3
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Composite(params IActionSource3[] sources)
        {
            if (sources.Any(_ => _ == null))
            {
                throw new ArgumentNullException(nameof(sources));
            }
            this.Sources = sources.ToList();
        }

        public IObservable<IResult> GetActions(IQuery query)
        {
            var actionObservables = Sources.SafeSelect(source =>
            {
                log.Debug(source);
                return source.GetActions(query);
            }).ToList();
            return actionObservables.Merge();
        }

        IList<IActionSource3> Sources;

        public void Add(IActionSource3 source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Sources = Sources.Concat(source).ToList();
        }
    }
}
