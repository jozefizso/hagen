﻿using NUnit.Framework;
using hagen.plugin.office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Test;

using Sidi.Extensions;
using Sidi.Util;
using System.Text.RegularExpressions;
using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;

namespace hagen.plugin.office.Tests
{
    [TestFixture()]
    public class OutlookExtensionsTests : TestBase
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test(), Explicit("requires Outlook")]
        public void BetterDecline()
        {
            var app = OutlookExtensions.ProvideApplication();
            var explorer = app.ActiveExplorer();
            log.Info(explorer.Caption);

            var declinedFolder = app.ProvideFolder(OlDefaultFolders.olFolderCalendar, "declined");

            foreach (var i in explorer.Selection.OfType<AppointmentItem>())
            {
                log.Info(i.Subject);
                i.Respond(OlMeetingResponse.olMeetingDeclined);
                i.Move(declinedFolder);
            }
        }

        [Test(), Explicit("requires outlook")]
        public void ProvideFolderTest()
        {
            var app = OutlookExtensions.ProvideApplication();
            var f1 = app.ProvideFolder(OlDefaultFolders.olFolderCalendar, "test");
            log.Info(
                app.GetSession()
                    .GetDefaultFolder(OlDefaultFolders.olFolderCalendar)
                    .Folders.OfType<MAPIFolder>()
                    .Select(_ => _.Name)
                    .Join());

            var f2 = app.ProvideFolder(OlDefaultFolders.olFolderCalendar, "test");
            log.Info(
                app.GetSession()
                    .GetDefaultFolder(OlDefaultFolders.olFolderCalendar)
                    .Folders.OfType<MAPIFolder>()
                    .Select(_ => _.Name)
                    .Join());

            Assert.AreEqual(f1.Name, f2.Name);
            log.Info(
                app.GetSession()
                    .GetDefaultFolder(OlDefaultFolders.olFolderCalendar)
                    .Folders.OfType<MAPIFolder>()
                    .Select(_ => _.Name)
                    .Join());
        }

        [Test]
        public void OutlookQueryFormat_formats_date_correctly()
        {
            Assert.AreEqual("24.10.2002 00:00", new DateTime(2002, 10, 24, 0, 0, 0).OutlookQueryFormat());
        }

        [Test, Explicit("Requires outlook")]
        public void GetOutlookAppointmentsActiveIn()
        {
            var app = OutlookExtensions.GetRunningApplication();
            var calendar = app.GetCalendar();

            var time = new TimeInterval(new DateTime(2019, 2, 20, 10, 0, 0), new DateTime(2019, 2, 20, 20, 15, 0));
            var appointments = calendar.GetOutlookAppointmentsActiveIn(time);
            foreach (var i in appointments)
            {
                Console.WriteLine($"{i.Start} {i.End} {i.RecurrenceState} {i.Subject}");
            }
            Assert.AreEqual(4, appointments.Count());
        }

        [Test, Explicit("does not run on Jenkins")]
        public void AppointmentReminder()
        {
            var app = OutlookExtensions.ProvideApplication();
            var ns = app.GetNamespace("MAPI");
            var calendarFolder = ns.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            var ti = new TimeInterval(DateTime.Now, TimeSpan.FromMinutes(60));
            var upcoming = app.GetOutlookAppointmentsStartingIn(calendarFolder, ti);

            log.Info(upcoming.ListFormat()
                    .Add(_ => _.Subject)
                    .Add(_ => _.Location)
                    .Add(_ => _.Start)
                    .Add(_ => _.End)
            );
        }

        [Test(), Explicit("requires outlook")]
        public void TryGetSelectedMailTest()
        {
            var app = OutlookExtensions.ProvideApplication();
            Assert.IsTrue(app.TryGetSelectedMail(out var mail));
            log.Info(mail.Subject);
        }

        [Test(), Explicit("requires outlook")]
        public void InviteEveryoneTest()
        {
            var app = OutlookExtensions.ProvideApplication();
            Assert.IsTrue(app.TryGetSelectedMail(out var mail));
            app.InviteEveryone(mail);
        }

        [Test(), Explicit("requires outlook")]
        public void ReplyDuTest()
        {
            var app = OutlookExtensions.ProvideApplication();
            Assert.IsTrue(app.TryGetSelectedMail(out var mail));
            app.ReplyDu(mail);
        }

        [Test(), Explicit("requires outlook")]
        public void DelegateTest()
        {
            var app = OutlookExtensions.ProvideApplication();
            Assert.IsTrue(app.TryGetSelectedMail(out var mail));
            app.Delegate(mail);
        }

        [Test]
        public void UpdateGreeting()
        {
            var body = @"Hallo Andreas, 

text text";

            var newBody = Regex.Replace(body, @"^Hallo .*,\ \r\n\r\n", "Hallo Christian, \r\n\r\n");
            Assert.AreEqual(@"Hallo Christian, 

text text", newBody);
        }

        [Test, Explicit]
        public void ManipulateBody()
        {
            var outlook = OutlookExtensions.GetRunningApplication();
            var inspector = outlook.ActiveInspector();
            dynamic editor = inspector.WordEditor;
            dynamic selection = editor.Windows[1].Selection;
            selection.InsertBefore("Hello");
        }
    }
}