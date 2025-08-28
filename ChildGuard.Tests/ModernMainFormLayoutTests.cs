using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Xunit;

namespace ChildGuard.Tests
{
    public class ModernMainFormLayoutTests
    {
        private static volatile bool _winformsInitialized = false;
        private static readonly object _initLock = new();
        private static void EnsureWinFormsInitialized()
        {
            if (_winformsInitialized) return;
            lock (_initLock)
            {
                if (_winformsInitialized) return;
                try { Application.SetHighDpiMode(HighDpiMode.SystemAware); } catch { }
                try { Application.EnableVisualStyles(); } catch { }
                try { Application.SetCompatibleTextRenderingDefault(false); } catch { }
                _winformsInitialized = true;
            }
        }

        private void RunInSta(Action action)
        {
            Exception? ex = null;
            var t = new Thread(() =>
            {
                try { EnsureWinFormsInitialized(); action(); }
                catch (Exception e) { ex = e; }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (ex != null) throw ex;
        }

        private Control? GetPrivateFieldControl(object instance, string fieldName)
        {
            var f = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return f?.GetValue(instance) as Control;
        }

        private static bool RectsOverlap(System.Drawing.Rectangle a, System.Drawing.Rectangle b)
        {
            return a.IntersectsWith(b);
        }

        private void AssertNoOverlapRecursive(Control parent)
        {
            // Ignore controls with zero size or invisible
            var children = parent.Controls.Cast<Control>().Where(c => c.Visible && c.Width > 0 && c.Height > 0).ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                for (int j = i + 1; j < children.Length; j++)
                {
                    var ci = children[i];
                    var cj = children[j];
                    // Compare rectangles in parent coordinates
                    var ri = new System.Drawing.Rectangle(ci.Left, ci.Top, ci.Width, ci.Height);
                    var rj = new System.Drawing.Rectangle(cj.Left, cj.Top, cj.Width, cj.Height);
                    Assert.False(RectsOverlap(ri, rj), $"Overlap detected between '{ci.Name ?? ci.GetType().Name}' and '{cj.Name ?? cj.GetType().Name}' in parent '{parent.Name ?? parent.GetType().Name}'");
                }
                // Recurse
                if (children[i].HasChildren)
                    AssertNoOverlapRecursive(children[i]);
            }
        }

        [Fact]
        public void Dashboard_NoOverlap()
        {
            RunInSta(() =>
            {
                using var f = new ChildGuard.UI.ModernMainForm();
                f.CreateControl();
                // Default is Dashboard
                var content = GetPrivateFieldControl(f, "contentPanel");
                Assert.NotNull(content);
                Assert.True(content!.Controls.Count > 0);
                var root = content.Controls[0];
                // Perform layout to ensure sizes
                root.PerformLayout();
                AssertNoOverlapRecursive(root);
            });
        }

        [Fact]
        public void Monitoring_NoOverlap()
        {
            RunInSta(() =>
            {
                using var f = new ChildGuard.UI.ModernMainForm();
                f.CreateControl();
                f.NavigateTo("Monitoring");
                var content = GetPrivateFieldControl(f, "contentPanel");
                Assert.NotNull(content);
                Assert.True(content!.Controls.Count > 0);
                var root = content.Controls[0];
                root.PerformLayout();
                AssertNoOverlapRecursive(root);
            });
        }

        [Fact]
        public void Protection_NoOverlap()
        {
            RunInSta(() =>
            {
                using var f = new ChildGuard.UI.ModernMainForm();
                f.CreateControl();
                f.NavigateTo("Protection");
                var content = GetPrivateFieldControl(f, "contentPanel");
                Assert.NotNull(content);
                Assert.True(content!.Controls.Count > 0);
                var root = content.Controls[0];
                root.PerformLayout();
                AssertNoOverlapRecursive(root);
            });
        }

        [Fact]
        public void Reports_NoOverlap()
        {
            RunInSta(() =>
            {
                using var f = new ChildGuard.UI.ModernMainForm();
                f.CreateControl();
                f.NavigateTo("Reports");
                var content = GetPrivateFieldControl(f, "contentPanel");
                Assert.NotNull(content);
                Assert.True(content!.Controls.Count > 0);
                var root = content.Controls[0];
                root.PerformLayout();
                AssertNoOverlapRecursive(root);
            });
        }

        [Fact]
        public void Settings_NoOverlap()
        {
            RunInSta(() =>
            {
                using var f = new ChildGuard.UI.ModernMainForm();
                f.CreateControl();
                f.NavigateTo("Settings");
                var content = GetPrivateFieldControl(f, "contentPanel");
                Assert.NotNull(content);
                Assert.True(content!.Controls.Count > 0);
                var root = content.Controls[0];
                root.PerformLayout();
                AssertNoOverlapRecursive(root);
            });
        }
    }
}

