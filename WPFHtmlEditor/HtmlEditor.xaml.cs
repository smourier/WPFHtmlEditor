using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using mshtml;

namespace SoftFluent.Tools
{
    public partial class HtmlEditor : UserControl
    {
        public static readonly DependencyProperty BodyHtmlProperty =
            DependencyProperty.Register("BodyHtml", typeof(string), typeof(HtmlEditor),
            new FrameworkPropertyMetadata(null));

        private HTMLBody _body;
        private EventListener _listener = new EventListener();

        public HtmlEditor()
        {
            _listener.Event += OnBodyChanged;
            InitializeComponent();
            Tray.Loaded += (sender, e) => AdjustStyle(Tray, Brushes.LightGray);
            Browser.LoadCompleted += Browser_LoadCompleted;
            Browser.NavigateToString("<html/>");
            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonBase_Click));
        }

        private void OnBodyChanged(object sender, EventArgs e)
        {
            Trace.WriteLine("body changed");
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            if (doc == null)
                return;

            string bodyHtml = doc.body != null ? doc.body.innerHTML : null;
            if (bodyHtml != BodyHtml)
            {
                Trace.WriteLine("body really changed new:" + bodyHtml);
                BodyHtml = bodyHtml;
            }
        }

        public bool IsIE9OrHigher
        {
            get
            {
                return BrowserVersion.Major >= 9;
            }
        }

        public bool IsIE10OrHigher
        {
            get
            {
                return BrowserVersion.Major >= 10;
            }
        }

        public bool IsIE11OrHigher
        {
            get
            {
                return BrowserVersion.Major >= 11;
            }
        }

        public Version BrowserVersion
        {
            get
            {
                FileVersionInfo fi = FileVersionInfo.GetVersionInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "mshtml.dll"));
                return new Version(fi.ProductMajorPart, fi.ProductMinorPart, fi.ProductBuildPart, fi.ProductPrivatePart);
            }
        }

        public string BodyHtml
        {
            get { return (string)GetValue(BodyHtmlProperty); }
            set { SetValue(BodyHtmlProperty, value); }
        }

        public bool ExecuteCommand(string name, bool throwOnError, bool raiseChanged)
        {
            return ExecuteCommand(name, false, Type.Missing, throwOnError, raiseChanged);
        }

        public bool ExecuteCommand(string name, bool showUi, object value, bool throwOnError, bool raiseChanged)
        {
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            if (doc == null)
                return false;

            bool b;
            if (throwOnError)
            {
                b = doc.execCommand(name, showUi, value);
            }
            try
            {
                b = doc.execCommand(name, showUi, value);
            }
            catch
            {
                b = false;
            }
            if (b)
            {
                OnBodyChanged(this, EventArgs.Empty);
            }
            return b;
        }

        public bool IsCommandEnabled(string name)
        {
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            if (doc == null)
                return false;

            return doc.queryCommandEnabled(name);
        }

        public bool IsCommandIndeterminate(string name)
        {
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            if (doc == null)
                return false;

            return doc.queryCommandIndeterm(name);
        }

        public bool IsCommandSupported(string name)
        {
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            if (doc == null)
                return false;

            return doc.queryCommandSupported(name);
        }

        private void ButtonBase_Click(object sender, RoutedEventArgs e)
        {
            HandleButtonClick(e.OriginalSource as ButtonBase);
        }

        protected virtual void HandleButtonClick(ButtonBase bb)
        {
            if (bb == null || bb.Name == null)
                return;

            const string commandPrefix = "Cmd";
            if (bb.Name.StartsWith(commandPrefix))
            {
                string command = bb.Name.StartsWith(commandPrefix) ? bb.Name.Substring(commandPrefix.Length) : bb.Name;
                command = command.Replace("_", "-"); // handle ms- commands
                ExecuteCommand(command, false, true);
                return;
            }

            switch (bb.Name)
            {
                case "ButtonCode":
                    SwitchSourceCode();
                    break;
            }
        }

        protected virtual void SwitchSourceCode()
        {
            if (Browser.Visibility == Visibility.Hidden)
            {
                BodyHtml = Code.Text;
                Browser.Visibility = Visibility.Visible;
                Code.Visibility = Visibility.Hidden;
            }
            else
            {
                Code.Text = BodyHtml;
                Browser.Visibility = Visibility.Hidden;
                Code.Visibility = Visibility.Visible;
            }

            StyleBar.IsEnabled = Browser.Visibility == Visibility.Visible;
            IndentBar.IsEnabled = Browser.Visibility == Visibility.Visible;
            AlignBar.IsEnabled = Browser.Visibility == Visibility.Visible;
            CmdUndo.IsEnabled = Browser.Visibility == Visibility.Visible;
            CmdRedo.IsEnabled = Browser.Visibility == Visibility.Visible;
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            try
            {
                Browser_LoadCompleted(e);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error:" + ex);
            }
        }

        private void Browser_LoadCompleted(NavigationEventArgs e)
        {
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            IHTMLBodyElement body = (IHTMLBodyElement)doc.body;
            doc.charset = "utf-8";
            ((IHTMLElement3)body).contentEditable = "true";
            _body = (HTMLBody)doc.body;

            dynamic d = doc;
            d.attachEvent("onkeyup", _listener);
            d.attachEvent("onblur", _listener);
            d.attachEvent("onchange", _listener);
            d.attachEvent("oninput", _listener);
            d.attachEvent("onpaste", _listener);
            d.attachEvent("oncut", _listener);
        }

        private static void AdjustStyle(ToolBarTray tray, Brush background)
        {
            foreach (var tb in tray.ToolBars)
            {
                AdjustStyle(tb, background);
            }
        }

        private static void AdjustStyle(ToolBar tb, Brush background)
        {
            var border = tb.FindVisualChild<Border>("MainPanelBorder");
            if (border != null)
            {
                border.CornerRadius = new CornerRadius(0);
                border.Background = background;
            }

            border = tb.FindVisualChild<Border>("Bd");
            if (border != null)
            {
                border.CornerRadius = new CornerRadius(0);
                border.Background = background;
            }
        }

        [ComVisible(true)]
        public class EventListener : ICustomQueryInterface
        {
            public event EventHandler Event;

            // some kind of a hack... we don't implement anything, we just fire when someone wants to call us...
            // so we can have only one catch-all class 
            public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
            {
                Trace.WriteLine("iid:" + iid);
                ppv = IntPtr.Zero;
                var handler = Event;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
                return CustomQueryInterfaceResult.NotHandled;
            }
        }
    }
}
