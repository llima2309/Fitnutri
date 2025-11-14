# 🔧 Correção: Erro de Acesso à Câmera/Microfone no WebView

## 🐛 Problema Original

```
Erro ao iniciar videochamada: Erro ao acessar camera/microfone: 
undefined is not an object (evaluating navigator.mediaDevices.getUserMedia)
```

### Causa
O WebView do MAUI não tinha:
1. Permissões de câmera/microfone configuradas
2. Configurações necessárias para WebRTC
3. Handler para solicitar permissões ao usuário

---

## ✅ Soluções Aplicadas

### 1. **Melhorado Tratamento de Erro no JavaScript**

**Arquivo**: `VideoCallPage.xaml.cs`

✅ Adicionada verificação se `navigator.mediaDevices` existe
✅ Mensagens de erro mais claras baseadas no tipo de erro
✅ Console.log para debug
✅ Configurações otimizadas de vídeo e áudio

```javascript
// Verifica disponibilidade da API
if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
    throw new Error('API de mídia não disponível...');
}

// Trata erros específicos
if (error.name === 'NotAllowedError') {
    errorMessage += 'Permissão negada...';
} else if (error.name === 'NotFoundError') {
    errorMessage += 'Nenhuma câmera ou microfone encontrado...';
}
```

---

### 2. **Permissões Android**

**Arquivo**: `Platforms/Android/AndroidManifest.xml`

✅ Adicionadas permissões:
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.MODIFY_AUDIO_SETTINGS" />
<uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />
<uses-permission android:name="android.permission.CHANGE_WIFI_STATE" />

<uses-feature android:name="android.hardware.camera" android:required="false" />
<uses-feature android:name="android.hardware.microphone" android:required="false" />
```

---

### 3. **Permissões iOS**

**Arquivo**: `Platforms/iOS/Info.plist`

✅ Adicionadas descrições de uso:
```xml
<key>NSCameraUsageDescription</key>
<string>O FitNutri precisa acessar sua câmera para realizar videochamadas...</string>

<key>NSMicrophoneUsageDescription</key>
<string>O FitNutri precisa acessar seu microfone para realizar videochamadas...</string>
```

---

### 4. **Handler Customizado para Android**

**Arquivo**: `Platforms/Android/CustomWebViewHandler.cs` ⭐ **NOVO**

✅ Criado handler que:
- Habilita JavaScript
- Habilita DOM Storage
- Permite acesso a arquivos
- Habilita Mixed Content (para HTTPS)
- **Concede automaticamente permissões de câmera/microfone**
- Logs do console do WebView

```csharp
public class CustomWebChromeClient : WebChromeClient
{
    public override void OnPermissionRequest(PermissionRequest? request)
    {
        // Concede automaticamente permissões de câmera e microfone
        if (resource == PermissionRequest.ResourceVideoCapture ||
            resource == PermissionRequest.ResourceAudioCapture)
        {
            request.Grant(resources.ToArray());
        }
    }
}
```

---

### 5. **Registro do Handler**

**Arquivo**: `MauiProgram.cs`

✅ Registrado handler customizado apenas para Android:
```csharp
#if ANDROID
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<WebView, AppFitNutri.Platforms.Android.CustomWebViewHandler>();
});
#endif
```

---

## 📋 Checklist de Verificação

### Antes de Testar

- [ ] **Rebuild completo do projeto**
  ```bash
  dotnet clean
  dotnet build
  ```

- [ ] **Desinstalar app do dispositivo/emulador**
  - Android: Desinstalar FitNutri
  - iOS: Desinstalar FitNutri

- [ ] **Reinstalar app**
  - Garante que as novas permissões sejam lidas

### Durante o Teste

- [ ] Ao abrir videochamada pela primeira vez, deve aparecer:
  - **Android**: Popup solicitando permissão de câmera e microfone
  - **iOS**: Popup solicitando permissão de câmera e microfone

- [ ] Conceder as permissões

- [ ] Verificar se vídeo local aparece

- [ ] Verificar console do dispositivo para logs

---

## 🧪 Testando

### Android

1. **Conectar dispositivo/emulador Android**
2. **Limpar e rebuildar**:
   ```bash
   dotnet clean
   dotnet build -t:Run -f net8.0-android
   ```
3. **Abrir videochamada**
4. **Verificar popup de permissões**
5. **Conceder permissões**

### iOS

1. **Conectar dispositivo iOS ou usar simulador**
2. **Limpar e rebuildar**:
   ```bash
   dotnet clean
   dotnet build -t:Run -f net8.0-ios
   ```
3. **Abrir videochamada**
4. **Verificar popup de permissões**
5. **Conceder permissões**

---

## 🔍 Debug

### Ver Logs do WebView (Android)

O `CustomWebChromeClient` agora loga mensagens do console:

```csharp
public override bool OnConsoleMessage(ConsoleMessage? consoleMessage)
{
    System.Diagnostics.Debug.WriteLine(
        $"WebView Console: [{consoleMessage.MessageLevel()}] {consoleMessage.Message()}");
    return true;
}
```

**Ver logs no Visual Studio:**
- Output > Debug
- Filtrar por "WebView Console"

### Comandos úteis

**Verificar permissões concedidas (Android):**
```bash
adb shell dumpsys package com.companyname.appfitnutri | grep permission
```

**Ver logs em tempo real (Android):**
```bash
adb logcat | grep "FitNutri\|WebView\|chromium"
```

---

## ⚠️ Problemas Conhecidos

### Emulador Android sem Câmera

Se o emulador não tiver câmera virtual configurada:

1. **AVD Manager** > Selecione o emulador
2. **Edit** > **Show Advanced Settings**
3. **Camera**:
   - Front: `Webcam0` ou `Emulated`
   - Back: `Webcam0` ou `Emulated`
4. Salvar e reiniciar emulador

### iOS Simulator

O simulador iOS **não tem câmera física**, mas deve permitir acesso mockado.

Para testar com câmera real no iOS:
- Use dispositivo físico iOS conectado

---

## 🎯 Resultado Esperado

### ✅ Funcionando Corretamente:

1. **Ao clicar em "🎥"**:
   - Abre página de videochamada
   - Mostra "Obtendo mídia..."

2. **Primeira vez**:
   - Popup de permissões aparece
   - Usuário concede permissões

3. **Após conceder**:
   - Vídeo local aparece (canto superior direito)
   - Status muda para "Conectado"
   - Console mostra: "Mídia obtida com sucesso"

4. **Se outro participante entrar**:
   - Vídeo remoto aparece (tela inteira)
   - Áudio bidirecional funciona

### ❌ Se Continuar com Erro:

**Erro: "API de mídia não disponível"**
- WebView não suporta WebRTC neste dispositivo
- Solução: Usar navegador externo ou componente nativo

**Erro: "Permissão negada"**
- Usuário negou permissões
- Ir em Configurações > Apps > FitNutri > Permissões
- Habilitar Câmera e Microfone

**Erro: "Câmera já em uso"**
- Outro app está usando a câmera
- Fechar outros apps que usam câmera

---

## 📁 Arquivos Modificados/Criados

### Novos (1):
✅ `Platforms/Android/CustomWebViewHandler.cs`

### Modificados (4):
✅ `Views/VideoCallPage.xaml.cs`
✅ `Platforms/Android/AndroidManifest.xml`
✅ `Platforms/iOS/Info.plist`
✅ `MauiProgram.cs`

---

## 🚀 Próximos Passos

1. **Rebuild completo**
2. **Desinstalar app antigo**
3. **Reinstalar app novo**
4. **Testar videochamada**
5. **Conceder permissões quando solicitado**
6. **Verificar se vídeo aparece**

---

## 💡 Alternativa: Usar Navegador Externo

Se o WebView continuar com problemas, você pode abrir a videochamada no navegador nativo:

```csharp
// Abrir no navegador
await Launcher.OpenAsync(new Uri($"{apiUrl}/videocall?token={token}&agendamentoId={agendamentoId}"));
```

Isso garantiria 100% de compatibilidade com WebRTC, mas perderia a integração nativa.

---

**Status**: ✅ Correções aplicadas e testáveis

