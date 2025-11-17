# Solução de Player de Vídeo - FitNutri

## ✅ Decisão: WebView com HTML5 Player

### Por que WebView ao invés de MediaElement?

#### Problemas encontrados com MediaElement:
1. **Android**: Vídeo não renderiza (apenas áudio)
2. **Android**: Fullscreen vai para trás do modal
3. **Dependências**: Requer pacotes AndroidX extras com conflitos
4. **Handlers**: Necessita de handlers customizados (workarounds)
5. **Manutenção**: Lib com bugs conhecidos no Android

#### Vantagens do WebView + HTML5:
✅ **Funciona nativamente** em Android e iOS  
✅ **Controles nativos** do navegador (play, pause, fullscreen)  
✅ **Suporte a fullscreen** sem problemas de z-index  
✅ **Sem dependências extras** - usa recursos nativos do MAUI  
✅ **Código simples** - menos linhas, mais manutenível  
✅ **S3 compatível** - headers corretos para streaming HTTP  
✅ **Performance** - hardware acceleration automático  
✅ **Padrão da indústria** - Netflix, YouTube usam HTML5 em embeds  

## 🎯 Implementação

### 1. Estrutura
```
Views/
  ├── ExerciseVideoModal.xaml          # UI do modal
  └── ExerciseVideoModal.xaml.cs       # Player HTML5
ViewModel/
  └── ExerciseVideoViewModel.cs        # Lógica do vídeo
Models/
  └── Exercise.cs                       # URL do vídeo padrão
```

### 2. Funcionalidades
- ✅ Player HTML5 responsivo
- ✅ Controles nativos (play, pause, volume, fullscreen)
- ✅ Preload de metadata para preview rápido
- ✅ Object-fit contain (mantém proporção)
- ✅ Suporte a orientação landscape no fullscreen
- ✅ Previne zoom acidental no iOS
- ✅ Poster com ícone de play

### 3. Hospedagem
**AWS S3** - `https://fitnutri-videos.s3.us-east-1.amazonaws.com/`

#### Configurações necessárias no S3:
```json
{
  "CORSRules": [
    {
      "AllowedOrigins": ["*"],
      "AllowedMethods": ["GET", "HEAD"],
      "AllowedHeaders": ["*"],
      "ExposeHeaders": ["Content-Length", "Content-Range"],
      "MaxAgeSeconds": 3000
    }
  ]
}
```

#### Content-Type correto:
```
Content-Type: video/mp4
```

### 4. URL Padrão de Teste
```csharp
public string VideoUrl { get; set; } = 
    "https://fitnutri-videos.s3.us-east-1.amazonaws.com/video.mp4";
```

## 🔄 Como Usar

### No código:
```csharp
var exercise = new Exercise 
{
    Name = "Supino Reto",
    Sets = "4",
    Reps = "12",
    VideoUrl = "https://fitnutri-videos.s3.us-east-1.amazonaws.com/supino.mp4"
};

var viewModel = new ExerciseVideoViewModel(exercise, null);
var modal = new ExerciseVideoModal(viewModel);
await Navigation.PushModalAsync(modal);
```

### Upload de novos vídeos:
1. Fazer upload no S3 bucket `fitnutri-videos`
2. Definir Content-Type como `video/mp4`
3. Configurar ACL como público ou usar URLs assinadas
4. Usar a URL do S3 no objeto Exercise

## 📱 Compatibilidade

| Plataforma | Status | Observações |
|-----------|--------|-------------|
| iOS       | ✅ Perfeito | Suporte nativo completo |
| Android   | ✅ Perfeito | WebView usa ExoPlayer internamente |

## 🎨 UI/UX

- Modal com fundo semi-transparente
- Header azul com nome do exercício
- Player de vídeo em container preto arredondado
- Detalhes do exercício (séries, repetições)
- Dicas de execução
- Botão de fechar no header

## 🚀 Performance

- Preload metadata apenas (não o vídeo inteiro)
- Hardware acceleration habilitado
- Streaming progressivo do S3
- Lazy loading do vídeo

## 🔧 Manutenção

### Adicionar novo vídeo a um exercício:
```csharp
exercise.VideoUrl = "https://fitnutri-videos.s3.us-east-1.amazonaws.com/novo-video.mp4";
```

### Trocar provider de vídeos:
Apenas atualizar a propriedade `VideoUrl` - funciona com qualquer URL HTTP(S) de vídeo MP4.

## 📝 Boas Práticas Aplicadas

1. ✅ **Separation of Concerns**: ViewModel separado
2. ✅ **Cross-platform**: Código único para iOS/Android
3. ✅ **MVVM Pattern**: Binding limpo com ViewModel
4. ✅ **Responsive**: Adapta a diferentes tamanhos de tela
5. ✅ **Accessibility**: Controles nativos acessíveis
6. ✅ **Performance**: Hardware acceleration
7. ✅ **Maintainability**: Código simples e direto
8. ✅ **Scalability**: Fácil adicionar novos vídeos

## 🎓 Referências

- [MDN: HTML5 Video](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/video)
- [MAUI WebView](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/webview)
- [AWS S3 CORS](https://docs.aws.amazon.com/AmazonS3/latest/userguide/cors.html)

