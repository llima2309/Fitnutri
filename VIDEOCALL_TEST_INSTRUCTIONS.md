# 📹 Instruções - Teste de Videochamada

## Como Testar a Videochamada

### 1. **Inicie a API**

Certifique-se de que o projeto `Fitnutri` está rodando:

```bash
cd Fitnutri
dotnet run
```

A API estará disponível em: `https://localhost:7001`

### 2. **Acesse a Página de Teste**

Abra seu navegador e acesse:

```
https://localhost:7001/videocall-test.html
```

### 3. **Prepare os Dados Necessários**

Você precisará de:

#### a) **x-api-key**
- Obtenha a API Key configurada no `appsettings.json`
- Por padrão está em: `"ApiKey": { "Key": "..." }`
- Esta chave é obrigatória para todas as requisições

#### b) **Token JWT**
- Faça login na aplicação para obter um token
- Ou use um endpoint de autenticação diretamente:

```bash
POST https://localhost:7001/auth/login
Content-Type: application/json
x-api-key: sua-api-key-aqui

{
  "email": "seu-email@exemplo.com",
  "password": "sua-senha"
}
```

#### c) **ID do Agendamento**
- Crie um agendamento confirmado primeiro
- Ou use um ID de agendamento existente que esteja com status `Confirmado`

#### c) **User ID**
- O ID do usuário autenticado (profissional ou paciente)
- Este ID deve estar vinculado ao agendamento

### 4. **Configure a Página de Teste**

Na página de teste, preencha os campos:

1. **URL da API**: `https://localhost:7001` (já preenchido por padrão)
2. **x-api-key**: Cole a API Key do appsettings.json
3. **Token JWT**: Cole o token obtido no login
4. **ID do Agendamento**: Cole o GUID do agendamento
5. **User ID**: Cole o ID do usuário
6. **Tipo de Usuário**: Selecione `Profissional` ou `Paciente`

### 5. **Inicie a Videochamada**

Clique em **"Iniciar Videochamada"** para:
- Chamar o endpoint `/api/videocall/initiate` (requer **x-api-key**)
- Conectar automaticamente ao SignalR Hub (não requer x-api-key - está no bypass)
- Solicitar permissões de câmera e microfone

**Nota**: O SignalR Hub (`/videocall`) está configurado no `BypassPaths` do `ApiKeyMiddleware`, então não precisa do header `x-api-key`.

### 6. **Teste com Múltiplos Participantes**

Para testar a comunicação entre dois participantes:

1. **Abra a página em duas abas diferentes** (ou dois navegadores)
2. Na **primeira aba**:
   - Configure com o token do **profissional**
   - Use o mesmo ID de agendamento
   - Clique em "Iniciar Videochamada"

3. Na **segunda aba**:
   - Configure com o token do **paciente**
   - Use o **mesmo ID de agendamento**
   - Clique apenas em "Conectar ao SignalR" (a chamada já estará iniciada)
   - **Nota**: Não precisa do x-api-key para conectar ao SignalR

### 7. **Controles Disponíveis**

Durante a chamada, você pode:

- 🎤 **Ativar/Desativar Áudio**: Liga/desliga o microfone
- 📹 **Ativar/Desativar Vídeo**: Liga/desliga a câmera
- 📞 **Encerrar Chamada**: Finaliza a videochamada e desconecta

### 8. **Monitoramento**

A página oferece:

- **Indicador de Status**: Mostra se está conectado ao SignalR
- **Contador de Participantes**: Exibe quantos usuários estão na chamada
- **Log de Eventos**: Histórico detalhado de todas as ações e eventos WebRTC

## 🔧 Troubleshooting

### Erro: "x-api-key, Token JWT e ID do Agendamento são obrigatórios"
- Verifique se preencheu todos os campos obrigatórios
- Confirme que está usando a API Key correta do appsettings.json

### Erro ao iniciar chamada
- Certifique-se de que o agendamento existe e está com status `Confirmado`
- Verifique se o token JWT é válido e não expirou
- Confirme que o usuário tem permissão para acessar o agendamento

### Câmera/Microfone não funcionam
- Permita o acesso quando o navegador solicitar
- Verifique se outro aplicativo não está usando a câmera
- No Chrome: `chrome://settings/content/camera` e `chrome://settings/content/microphone`

### Vídeo não aparece
- Abra o console do navegador (F12) para ver erros
- Verifique o log de eventos na página
- Certifique-se de que ambos os participantes estão conectados

### Erro de certificado HTTPS em desenvolvimento
- No Chrome: digite `thisisunsafe` quando aparecer o aviso
- Ou configure certificados de desenvolvimento válidos

## 📊 Fluxo da Videochamada

```
1. Profissional/Paciente → Inicia a chamada via API
2. API → Cria CallToken e marca CallStartedAt
3. Cliente → Conecta ao SignalR Hub
4. Cliente → Entra na sala (JoinCall)
5. SignalR → Notifica outros participantes (UserJoined)
6. WebRTC → Troca de ofertas/respostas/ICE candidates
7. WebRTC → Estabelece conexão P2P
8. Vídeo/Áudio → Streaming entre participantes
9. Cliente → Encerra a chamada (LeaveCall)
10. SignalR → Notifica desconexão (UserLeft)
```

## 🔐 Segurança

- A página de teste usa **autenticação JWT**
- O SignalR Hub requer **autorização**
- Apenas participantes do agendamento podem entrar na chamada
- O CallToken é único por agendamento

## 💡 Dicas

- Use dois navegadores diferentes (ex: Chrome e Firefox) para evitar problemas de compartilhamento de mídia
- Mantenha o console do navegador aberto para debug
- O log de eventos na página mostra todos os passos da conexão WebRTC
- Para produção, configure servidores TURN para NAT traversal

## 🎯 Próximos Passos

Após validar o funcionamento:

1. Integrar com a aplicação mobile (MAUI)
2. Adicionar UI/UX profissional
3. Implementar gravação de chamadas (opcional)
4. Adicionar chat durante a videochamada
5. Implementar compartilhamento de tela
6. Configurar servidores TURN para produção

---

**Observação**: Esta é uma página de **teste/desenvolvimento**. Para produção, implemente uma interface adequada com melhor UX e tratamento de erros.

