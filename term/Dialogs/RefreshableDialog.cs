using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;

namespace MTG_CLI
{
    [ExcludeFromCodeCoverage] // For now - maybe I can separate logic from UI?
    abstract class RefreshableDialog
    {
        static int cnt = 0;

        protected void ClearTmpViews(Dialog dlg)
        {
            if (dlg.Visible)
            {
                List<View> removable = new();
                foreach (View view in dlg.Subviews[0].Subviews)
                {
                    if (view.Id.StartsWith("tmp"))
                    {
                        removable.Add(view);
                    }
                }
                foreach (View view in removable)
                    dlg.Subviews[0].Remove(view);
                dlg.SetNeedsDisplay();
            }
        }

        protected T tmpView<T>(T view) where T : View
        {
            view.Id = "tmp" + cnt;
            cnt++;
            return view;
        }
    }
}