using Microsoft.Extensions.DependencyInjection;
using SmallApp.Library.Authentication;
using SmallApp.Library.GlobalFeatures.EmailTemplate;
using System.IO;
using System.Windows;

namespace NonPaymentDropper.FunctionWindows
{
    public partial class EmailTemplateEditor : Window
    {
        private readonly IUserSession _userSession;

        public EmailTemplateEditor(IUserSession userSession)
        {
            _userSession = userSession;
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var user = _userSession.GetUsername();
            await HtmlEditor.EnsureCoreWebView2Async();

            var windowTitleBaseString = "Email Template Editor - ";
            HtmlEditor.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                var templateName = e.TryGetWebMessageAsString();
                this.Title = windowTitleBaseString + (string.IsNullOrEmpty(templateName) ? "New Template" : templateName);
            };

            // Paths relative to the output folder
            var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "editor.html");
            var jsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "js", "tinymce", "tinymce.min.js");
            var cssPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "editor.css");

            if (!File.Exists(htmlPath) || !File.Exists(jsFile))
            {
                MessageBox.Show("editor.html or tinymce.min.js not found.");
                return;
            }

            var html = File.ReadAllText(htmlPath);
            html = html.Replace("{{TINYMCE_JS}}", new Uri(jsFile).AbsoluteUri);
            html = html.Replace("{{CONTENT_CSS}}", new Uri(cssPath).AbsoluteUri);
            // Important for loading template for this specific app
            html = html.Replace("LoadTemplate()", "LoadTemplateForApp()");
            html = html.Replace("LoadCategories()", "LoadCategoriesForApp()");

            // Write temp HTML for WebView2
            var tempFile = Path.Combine(Path.GetTempPath(), "editor_temp.html");
            File.WriteAllText(tempFile, html);

            HtmlEditor.Source = new Uri(tempFile);

            var host = new WpfEditorHost(HtmlEditor);
            var dataAccess = ActivatorUtilities.CreateInstance<EmailTemplateDataAccess>(
                App.serviceProvider,
                host,
                user
            );
            HtmlEditor.CoreWebView2.AddHostObjectToScript("csHandler", dataAccess);
        }
    }
}
