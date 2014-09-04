using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using mshtml;

namespace SoftFluent.Tools
{
    public partial class HtmlEditor : UserControl
    {
        public HtmlEditor()
        {
            InitializeComponent();
            Tray.Loaded += (sender, e) => AdjustStyle(Tray, Brushes.LightGray);
            Browser.LoadCompleted += Browser_LoadCompleted;
            Browser.NavigateToString("<html/>");
            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonBase_Click));
        }

        public Version BrowserVersion
        {
            get
            {
                FileVersionInfo fi = FileVersionInfo.GetVersionInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "mshtml.dll"));
                return new Version(fi.ProductMajorPart, fi.ProductMinorPart, fi.ProductBuildPart, fi.ProductPrivatePart);
            }
        }

        public string DocumentHtml
        {
            get
            {
                IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
                if (doc == null)
                    return null;

                return doc.body.parentElement.outerHTML;
            }
            set
            {
                if (value == DocumentHtml)
                    return;

                IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
                if (doc == null || doc.body == null)
                    return;

                if (value == null)
                {
                    value = string.Empty;
                }
                doc.body.parentElement.outerHTML = value;
            }
        }

        public string BodyHtml
        {
            get
            {
                IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
                if (doc == null)
                    return null;

                return doc.body != null ? doc.body.innerHTML : null;
            }
            set
            {
                if (value == BodyHtml)
                    return;

                IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
                if (doc == null || doc.body == null)
                    return;

                if (value == null)
                {
                    value = string.Empty;
                }
                doc.body.innerHTML = value;
            }
        }

        public bool ExecuteCommand(string name, bool throwOnError)
        {
            return ExecuteCommand(name, false, Type.Missing, throwOnError);
        }

        public bool ExecuteCommand(string name, bool showUi, object value, bool throwOnError)
        {
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            if (doc == null)
                return false;

            if (throwOnError)
                return doc.execCommand(name, showUi, value);

            try
            {
                return doc.execCommand(name, showUi, value);
            }
            catch
            {
                return false;
            }
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
                ExecuteCommand(command, false);
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
            IHTMLDocument2 doc = (IHTMLDocument2)Browser.Document;
            IHTMLBodyElement body = (IHTMLBodyElement)doc.body;
            doc.charset = "utf-8";
            ((IHTMLElement3)body).contentEditable = "true";
            var v = BrowserVersion;
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
    }
}
