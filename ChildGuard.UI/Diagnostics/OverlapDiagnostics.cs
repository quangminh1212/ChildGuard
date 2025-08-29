using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace ChildGuard.UI.Diagnostics
{
#if DEBUG
    internal static class OverlapDiagnostics
    {
        public static void Attach(Control root, Form owner, Action<string>? onOverlap = null)
        {
            if (root == null || owner == null) return;
            root.ControlAdded += (_, __) => owner.BeginInvoke(new Action(() => Check(root, onOverlap)));
            root.ControlRemoved += (_, __) => owner.BeginInvoke(new Action(() => Check(root, onOverlap)));
            owner.Resize += (_, __) => Check(root, onOverlap);
        }

        public static void Check(Control parent, Action<string>? onOverlap = null)
        {
            try
            {
                var controls = parent.Controls.OfType<Control>().Where(c => c.Visible).ToList();
                for (int i = 0; i < controls.Count; i++)
                {
                    for (int j = i + 1; j < controls.Count; j++)
                    {
                        var a = controls[i];
                        var b = controls[j];
                        var ra = a.RectangleToScreen(a.ClientRectangle);
                        var rb = b.RectangleToScreen(b.ClientRectangle);
                        if (ra.IntersectsWith(rb))
                        {
                            var msg = $"[UI-OVERLAP] {a.Name ?? a.GetType().Name} and {b.Name ?? b.GetType().Name} overlap in {parent.Name ?? parent.GetType().Name}";
                            Debug.WriteLine(msg);
                            onOverlap?.Invoke(msg);
                        }
                    }
                }
            }
            catch { }
        }
    }
#endif
}

