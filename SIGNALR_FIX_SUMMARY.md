# 🔧 Correções Aplicadas - SignalR + x-api-key

## Problema Original

O SignalR Hub estava retornando **401 Unauthorized** porque:
1. O `ApiKeyMiddleware` estava bloqueando a rota `/videocall/negotiate`
2. O SignalR não consegue enviar headers customizados como `x-api-key` durante a negociação
3. O token JWT precisava ser configurado para funcionar via query string

## ✅ Soluções Implementadas

### 1. **Bypass do x-api-key para SignalR** (`appsettings.json`)

```json
"ApiKey": {
  "Enabled": true,
  "Header": "x-api-key",
  "Key": "sua-chave-aqui",
  "BypassPaths": [ "/videocall" ]
}
```

**Resultado**: O SignalR Hub agora funciona **sem precisar** do header `x-api-key`.

### 2. **Suporte a Token JWT via Query String** (`Program.cs`)

Atualizado o `JwtBearerEvents.OnMessageReceived` para aceitar token via:
- Cookie HttpOnly (para o site)
- Query string `access_token` (para o SignalR)

```csharp
// Para SignalR: lê o token da query string (access_token)
var path = context.HttpContext.Request.Path;
if (string.IsNullOrEmpty(context.Token) &&
    path.StartsWithSegments("/videocall"))
{
    var accessToken = context.Request.Query["access_token"];
    if (!string.IsNullOrEmpty(accessToken))
    {
        context.Token = accessToken;
    }
}
```

### 3. **Página de Teste Atualizada** (`videocall-test.html`)

- Removida a exigência de `x-api-key` para conectar ao SignalR Hub
- Mantida a exigência de `x-api-key` apenas para chamar a API REST `/api/videocall/initiate`
- Adicionada mensagem informativa no log

### 4. **Documentação Atualizada**

- `VIDEOCALL_TEST_INSTRUCTIONS.md`: Esclarecido quando o x-api-key é necessário
- `API_KEY_SETUP.md`: Guia completo para configurar a API Key

## 🎯 Como Funciona Agora

### Fluxo Completo:

1. **Iniciar Videochamada** (botão "Iniciar Videochamada")
   - ✅ Requer: `x-api-key` + Token JWT
   - Endpoint: `POST /api/videocall/initiate`
   - Cria o CallToken no banco de dados

2. **Conectar ao SignalR Hub** (automático após iniciar)
   - ✅ Requer: Token JWT (via `accessTokenFactory`)
   - ❌ **NÃO** requer: `x-api-key` (está no bypass)
   - URL: `wss://localhost:7001/videocall`

3. **Entrar na Sala** (`JoinCall`)
   - Autenticado via JWT Bearer Token
   - Autorização verificada pelo `[Authorize]` do Hub

## 🧪 Como Testar

### Teste Simples (1 participante):
1. Preencha: x-api-key, Token JWT, ID do Agendamento, User ID
2. Clique em "Iniciar Videochamada"
3. Permita acesso à câmera/microfone
4. Verifique se o status muda para "Conectado"

### Teste Completo (2 participantes):
1. **Aba 1 (Profissional)**:
   - Preencha todos os campos com token do profissional
   - Clique em "Iniciar Videochamada"

2. **Aba 2 (Paciente)**:
   - Preencha Token JWT do paciente e mesmos IDs
   - Clique apenas em "Conectar ao SignalR"
   - O vídeo deve aparecer em ambas as abas

## ⚠️ Importante

### Para o x-api-key funcionar:
- Configure a chave no `appsettings.json` (campo `ApiKey.Key`)
- Use a mesma chave na página de teste

### Para o JWT funcionar:
- O token deve ser válido e não expirado
- O usuário deve ter permissão para acessar o agendamento
- O agendamento deve estar com status `Confirmado`

## 🔐 Segurança

- **API REST** (`/api/videocall/*`): Protegida por `x-api-key` + JWT
- **SignalR Hub** (`/videocall`): Protegida **apenas** por JWT + `[Authorize]`
- O bypass do x-api-key é **intencional** pois o SignalR tem suas próprias restrições de autenticação

## 📝 Arquivos Modificados

1. ✅ `/Fitnutri/appsettings.json` - Adicionado BypassPaths
2. ✅ `/Fitnutri/Program.cs` - Configurado JWT para query string
3. ✅ `/Fitnutri/wwwroot/videocall-test.html` - Ajustada validação
4. ✅ `/VIDEOCALL_TEST_INSTRUCTIONS.md` - Atualizada documentação

## ✨ Resultado Final

Agora a videochamada deve funcionar perfeitamente! O erro 401 no `/videocall/negotiate` foi resolvido.

---

**Próximos Passos Sugeridos**:
1. Testar com 2 participantes reais
2. Verificar qualidade do vídeo/áudio
3. Testar reconexão em caso de queda
4. Implementar UI/UX final para produção

