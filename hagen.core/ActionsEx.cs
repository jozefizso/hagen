// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using Sidi.Persistence;
using System.IO;
using Sidi.IO;
using System.Runtime.InteropServices;

namespace hagen
{
    public static class ActionsEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void UpdateStartMenu(this Collection<Action> actions)
        {
            FileActionFactory f = new FileActionFactory();
            foreach (var p in new string[]{
                AllUsersStartMenu,
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            })
            {
                try
                {
                    foreach (var a in f.Recurse(p))
                    {
                        actions.AddOrUpdate(a);
                    }
                }
                catch (Exception e)
                {
                    log.Warn(f, e);
                }
            }

            foreach (var i in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                try
                {
                    var a = f.Create(Environment.GetFolderPath((Environment.SpecialFolder)i));
                    a.Name = i.ToString();
                    actions.AddOrUpdate(a);
                }
                catch (Exception)
                {
                }
            }
        }

    [DllImport("shell32.dll")]
    static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner,
       [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
    const int CSIDL_COMMON_STARTMENU = 0x16;  // \Windows\Start Menu\Programs

        public static string AllUsersStartMenu
        {
            get
            {
                StringBuilder path = new StringBuilder(260);
                SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_STARTMENU, false);
                return path.ToString();
            }
        }

        public static void Cleanup(this Collection<Action> actions)
        {
            var toDelete = actions.Where(x => !x.CommandObject.IsWorking).ToList();

            foreach (var a in toDelete)
            {
                actions.Remove(a);
            }
        }

        public static void AddOrUpdate(this Collection<Action> actions, Action newAction)
        {
            var ea = actions.Find("Command = @command", "command", newAction.Command);
            if (ea != null)
            {
                newAction.Id = ea.Id;
                actions.Update(newAction);
            }
            else
            {
                actions.Add(newAction);
            }
        }
    }
}
