﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace hagen.wf
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var activityLogger = new ActivityLogger())
            {
                log.Info("Startup");
                var main = new Main();
                Application.Run(main);
                log.Info("Shutdown");
            }
        }
    }
}