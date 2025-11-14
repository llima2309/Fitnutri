# 🔧 Solução: Erro de Conexão SignalR em Produção

## 🐛 Erro
```
Erro ao iniciar videochamada: Failed to complete negotiation with the server: TypeError: Load Failed
```

## ✅ Correção Aplicada

A URL da API foi configurada para **sempre usar produção**:

```csharp
private string GetApiBaseUrl()
{
    // API está em produção
    return "https://api.fit-nutri.com";
}
```

---

## 🔍 Possíveis Causas Restantes

Se o erro persistir, pode ser:

### 1. **Problema de CORS**
O SignalR precisa que o CORS esteja configurado corretamente no backend.

**Verifique no backend (`Fitnutri/Program.cs`):**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("app", p =>
        p.WithOrigins(
            "https://fit-nutri.com",
            "https://api.fit-nutri.com",
            "capacitor://localhost",  // Para apps mobile
            "ionic://localhost",
            "http://localhost"
        )
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// E use o CORS ANTES do SignalR
app.UseCors("app");
app.MapHub<VideoCallHub>("/videocall");
```

### 2. **SignalR não está respondendo**
Teste o endpoint diretamente:

```bash
# Testar se o SignalR responde
curl https://api.fit-nutri.com/videocall/negotiate \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "x-api-key: SUA_API_KEY"
```

**Resposta esperada**: JSON com `connectionId` e `availableTransports`

### 3. **Token JWT inválido ou expirado**
O token pode ter expirado durante o teste.

**Solução**: Fazer logout e login novamente no app para obter token novo.

### 4. **x-api-key está bloqueando o SignalR**
Verifique se o SignalR está no `BypassPaths`:

```json
// appsettings.json
"ApiKey": {
  "Enabled": true,
  "Header": "x-api-key",
  "Key": "sua-chave",
  "BypassPaths": [ "/videocall" ]  // ← IMPORTANTE
}
```

### 5. **Certificado SSL do servidor**
Se estiver usando certificado autoassinado, o WebView pode bloquear.

**No Android**, você pode adicionar configuração de rede:

```xml
<!-- res/xml/network_security_config.xml -->
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
    <domain-config cleartextTrafficPermitted="true">
        <domain includeSubdomains="true">api.fit-nutri.com</domain>
    </domain-config>
</network-security-config>
```

E no `AndroidManifest.xml`:
```xml
<application 
    android:networkSecurityConfig="@xml/network_security_config"
    ...>
```

---

## 🧪 Como Testar/Debug

### 1. **Ver logs do WebView no Android**

Os logs do JavaScript agora aparecem no console do Android Studio/Visual Studio:

```
WebView Console: [DEBUG] Conectando ao hub: https://api.fit-nutri.com/videocall
WebView Console: [DEBUG] Token JWT: Presente
WebView Console: [DEBUG] Iniciando conexão SignalR...
```

### 2. **Verificar no Chrome Remote Debugging**

Para Android:
1. Conecte o dispositivo
2. Abra Chrome: `chrome://inspect`
3. Selecione o WebView do app
4. Abra o console DevTools
5. Veja os logs em tempo real

### 3. **Teste direto no navegador**

Para isolar se é problema do app ou da API:

1. Abra o Chrome no PC
2. Acesse: `https://api.fit-nutri.com/videocall-test.html`
3. Teste a conexão SignalR
4. Se funcionar no PC mas não no app = problema do WebView
5. Se não funcionar em nenhum = problema do backend

---

## 🔧 Checklist de Verificação Backend

No servidor de produção, verifique:

- [ ] SignalR Hub está rodando: `https://api.fit-nutri.com/videocall`
- [ ] CORS inclui origens mobile
- [ ] SignalR está no `BypassPaths` do x-api-key
- [ ] JWT tokens são válidos
- [ ] Porta 443 (HTTPS) está aberta
- [ ] Certificado SSL é válido
- [ ] Logs do servidor mostram tentativas de conexão

---

## 📱 Próximos Passos

### Se o erro continuar:

1. **Capture os logs completos do WebView**
   - Use `chrome://inspect` (Android)
   - Veja exatamente qual URL está falhando

2. **Teste o endpoint `/videocall/negotiate`**
   - Usando Postman ou curl
   - Com o mesmo token JWT do app
   - Veja se responde corretamente

3. **Verifique os logs do servidor**
   - Veja se a requisição está chegando
   - Veja se há erros de CORS ou autenticação

4. **Se necessário, remova temporariamente o ApiKeyMiddleware do SignalR**
   ```csharp
   // No Program.cs, teste sem o middleware
   app.MapHub<VideoCallHub>("/videocall");
   ```

---

## ✅ Status Atual

- ✅ URL configurada para produção
- ✅ Permissões de câmera/microfone OK
- ✅ CustomWebViewHandler configurado
- ⏳ Aguardando teste de conexão SignalR

**Próximo teste**: Rebuild e verificar se conecta ao SignalR em produção.

