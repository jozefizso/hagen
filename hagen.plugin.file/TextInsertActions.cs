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
using System.Windows.Forms;
using Sidi.Util;
using Sidi.CommandLine;
using Sidi.Extensions;

namespace hagen
{
    [Usage("Actions for inserting text")]
    public class TextInsertActions
    {
        [Usage("inserts files in clipboard as text")]
        public void InsertFilesAsText()
        {
            if (Clipboard.ContainsFileDropList())
            {
                var text = Clipboard.GetFileDropList().Cast<string>().Join();
                InsertText(text);
            }
        }

        void InsertText(string text)
        {
            Clipboard.SetText(text);
            SendKeys.Send("+{INS}");
        }

        [Usage("Inserts the current date")]
        public void InsertDate()
        {
            InsertText(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        [Usage("Inserts the current time (RFC3339 format)")]
        public void InsertTime()
        {
            InsertText(DateTime.Now.ToString("yyyyMMddTHHmmss"));
        }

        [Usage("Inserts a random password")]
        public void InsertPassword()
        {
            var r = new Random();
            var text = Enumerable.Range(0, 3)
                .Select(p => new string(Enumerable.Range(0, 4).Select(c => (char)('a' + r.Next('z' - 'a'))).ToArray()))
                .Join(".");
            InsertText(text);
        }
    }
}
