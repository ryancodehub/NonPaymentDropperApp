using Microsoft.Web.WebView2.Wpf;
using SmallApp.Library.Interface;

namespace NonPaymentDropper.FunctionWindows
{
    public class WpfEditorHost : IEditorHost
    {
        private readonly WebView2 _webView;

        public WpfEditorHost(WebView2 webView)
        {
            _webView = webView;
        }

        public void ExecuteScript(string script)
        {
            _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
