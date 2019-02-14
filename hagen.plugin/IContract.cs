﻿// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace hagen
{
    public interface IContract
    {
        TimeSpan MaxWorkTimePerDay { get; }
        TimeSpan MaxHomeOfficeIdleTime { get; }
        TimeSpan RegularWorkTimePerWeek { get; }
        int WorkDaysPerWeek { get; }
        TimeSpan PauseTimePerDay { get; }
        TimeSpan RegularWorkTimePerDay { get; }
    }
}
