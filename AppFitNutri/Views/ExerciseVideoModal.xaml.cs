namespace AppFitNutri.Views;

public partial class ExerciseVideoModal
{
    public ExerciseVideoModal(ViewModel.ExerciseVideoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Carregar o player HTML5 com o vídeo
        LoadVideoPlayer(viewModel.VideoUrl);
    }
    
    private void LoadVideoPlayer(string videoUrl)
    {
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            background-color: #000000;
            overflow: hidden;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            width: 100vw;
        }}
        video {{
            width: 100%;
            height: 100%;
            object-fit: contain;
            outline: none;
        }}
    </style>
</head>
<body>
    <video 
        controls 
        playsinline 
        preload='metadata'
        controlsList='nodownload'
        poster='data:image/svg+xml,%3Csvg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""%23ffffff""%3E%3Cpath d=""M8 5v14l11-7z""/%3E%3C/svg%3E'>
        <source src='{videoUrl}' type='video/mp4'>
        Seu navegador não suporta vídeo HTML5.
    </video>
    <script>
        // Previne zoom no iOS
        document.addEventListener('touchmove', function(event) {{
            if (event.scale !== 1) {{ event.preventDefault(); }}
        }}, {{ passive: false }});
        
        // Otimizações de performance
        const video = document.querySelector('video');
        video.addEventListener('loadedmetadata', function() {{
            console.log('Vídeo carregado');
        }});
        
        // Suporte a fullscreen
        video.addEventListener('fullscreenchange', function() {{
            if (document.fullscreenElement) {{
                screen.orientation?.lock('landscape').catch(() => {{}});
            }}
        }});
    </script>
</body>
</html>";

        VideoWebView.Source = new HtmlWebViewSource { Html = htmlContent };
    }
}

