﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Forms;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace hagen
{
    public class ActionChooser
    {
        class SimpleActionSource : IActionSource2, IActionSource3
        {
            public SimpleActionSource(IList<IAction> actions)
            {
                this.actions = actions;
            }

            IList<IAction> actions;

            public IObservable<IAction> GetActions(string query)
            {
                if (String.IsNullOrEmpty(query))
                {
                    return actions.ToObservable();
                }

                var regex = new Regex(Regex.Escape(query), RegexOptions.IgnoreCase);

                return actions.Where(x => regex.IsMatch(x.Name)).ToObservable();
            }

            public IObservable<IResult> GetActions(IQuery query)
            {
                return GetActions(query.Text).Select(_ => _.ToResult());
            }
        }

        public static void AddCancel(Form f)
        {
            var buttonCancel = new Button();
            buttonCancel.Location = new System.Drawing.Point(94, 451);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 23);
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            f.Controls.Add(buttonCancel);
            f.CancelButton = buttonCancel;
        }

        public static void Choose(IContext context, IList<IAction> actions)
        {
            using (var sb = new SearchBox(context, new SimpleActionSource(actions)))
            {
                var f = sb.AsForm("Select");
                AddCancel(f);
                f.Visible = false;

                sb.ItemsActivated += (s, e) =>
                {
                    var selectedActions = sb.SelectedActions.ToList();
                    f.Close();
                    foreach (var i in selectedActions)
                    {
                        i.Execute();
                    }
                };

                f.Shown += (s, e) => { sb.QueryText = String.Empty; };
                f.WindowState = FormWindowState.Maximized;
                f.ShowDialog();
            }
        }
    }
}
