using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AndroidWebView = Android.Webkit.WebView;

namespace AppFitNutri.Platforms.Android;

public class CustomWebViewHandler : WebViewHandler
{
    protected override void ConnectHandler(AndroidWebView platformView)
    {
        base.ConnectHandler(platformView);
        
        // Configurar WebView para suportar WebRTC
        var settings = platformView.Settings;
        
        // Habilitar JavaScript (já deve estar habilitado)
        settings.JavaScriptEnabled = true;
        
        // Habilitar acesso a mídia
        settings.MediaPlaybackRequiresUserGesture = false;
        
        // Habilitar DOM storage
        settings.DomStorageEnabled = true;
        
        // Habilitar database
        settings.DatabaseEnabled = true;
        
        // Permitir acesso a arquivos
        settings.AllowFileAccess = true;
        settings.AllowContentAccess = true;
        
        // Habilitar suporte a WebRTC
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Lollipop)
        {
            settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
        }
        
        // Criar e configurar WebChromeClient customizado para permissões
        platformView.SetWebChromeClient(new CustomWebChromeClient());
    }
}

public class CustomWebChromeClient : WebChromeClient
{
    public override void OnPermissionRequest(PermissionRequest? request)
    {
        if (request == null) return;
        
        // Conceder automaticamente permissões de câmera e microfone
        var requestedResources = request.GetResources();
        if (requestedResources != null)
        {
            var resources = new List<string>();
            
            foreach (var resource in requestedResources)
            {
                if (resource == PermissionRequest.ResourceVideoCapture ||
                    resource == PermissionRequest.ResourceAudioCapture)
                {
                    resources.Add(resource);
                }
            }
            
            if (resources.Any())
            {
                request.Grant(resources.ToArray());
            }
            else
            {
                request.Deny();
            }
        }
    }
    
    public override bool OnConsoleMessage(ConsoleMessage? consoleMessage)
    {
        if (consoleMessage != null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"WebView Console: [{typeof(ConsoleMessage.MessageLevel)}] {consoleMessage.Message()} " +
                $"-- From line {consoleMessage.LineNumber()} of {consoleMessage.SourceId()}");
        }
        return true;
    }
}

