﻿using hagen.plugin.office;
using NetOffice.OutlookApi;
using Sidi.CommandLine;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen.plugin.office
{
    [Usage("Meeting minutes from outlook")]
    public class MarkdownMeetingMinutes
    {
        static IEnumerable<string> GetParticipants(AppointmentItem a)
        {
            return a.Recipients.Cast<Recipient>().Select(_ => $"{_.Name};");
        }

        static string GetHumanReadableDate(AppointmentItem a)
        {
            if (a.Start.Date.Equals(a.End.Date))
            {
                // same-day
                return $"{a.Start:yyyy-MM-dd}, {a.Start:HH:mm} - {a.End:HH:mm}";
            }
            else
            {
                return $"{a.Start:yyyy-MM-dd} {a.Start:HH:mm} - {a.End:yyyy-MM-dd} {a.End:HH:mm}";
            }
        }

        [Usage("Create a meeting minute from selected outlook appointment")]
        public void CreateMeetingMinutes()
        {
            var outlook = OutlookExtensions.GetRunningApplication();
            if (outlook == null)
            {
                return;
            }

            var selectedAppointment = outlook.ActiveExplorer().Selection
                .OfType<AppointmentItem>().FirstOrDefault();

            if (selectedAppointment == null) return;

            var mdFile = CreateMeetingMinutesMarkdownFile(selectedAppointment);

            TextEditor.Open(mdFile);
        }

        static LPath CreateMeetingMinutesMarkdownFile(AppointmentItem selectedAppointment)
        {
            var chpRoot = new LPath(@"C:\src\chp");
            var mdFile = chpRoot.CatDir(
                "doc", "meetings",
                $"{selectedAppointment.Start:yyyy-MM-dd} {Sidi.IO.LPath.GetValidFilename(selectedAppointment.Subject)}", 
                "Readme.md");

            if (!mdFile.Exists)
            {
                mdFile.EnsureParentDirectoryExists();
                mdFile.WriteAllText(GetMarkdown(selectedAppointment));
            }
            return mdFile;
        }

        static string GetMarkdown(AppointmentItem a)
        {
            using (var markdown = new StringWriter())
            {
                markdown.WriteLine(
$@"# {a.Subject}

Date: {GetHumanReadableDate(a)}  
Location: {a.Location}  
Participants:  
{String.Join("\r\n", GetParticipants(a).Select(_ => $"* {_}"))}

## Goal

{a.Body}

## Discussion

## Summary

## Next Steps

");
                return markdown.ToString();
            }
        }
    }
}
