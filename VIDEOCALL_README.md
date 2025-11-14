# Sistema de Videochamada - FitNutri

## Visão Geral

Sistema de videochamada integrado ao agendamento entre profissional e paciente, utilizando WebRTC para comunicação peer-to-peer e SignalR para sinalização.

## Arquitetura Backend

### Componentes Principais

1. **VideoCallHub** (`Application/VideoCallHub.cs`)
   - Hub SignalR para sinalização WebRTC
   - Gerencia conexões e salas de chamada
   - Transmite ofertas, respostas e ICE candidates

2. **VideoCallController** (`Application/VideoCallController.cs`)
   - API REST para gerenciar o ciclo de vida da videochamada
   - Autenticação via JWT
   - Vinculado ao sistema de agendamento

3. **Modelo Agendamento** (`Domain/Agendamento.cs`)
   - Campos adicionados:
     - `CallToken`: Token único da chamada
     - `CallStartedAt`: Data/hora de início
     - `CallEndedAt`: Data/hora de término

## Endpoints da API

### 1. Iniciar Videochamada

**POST** `/api/videocall/initiate`

Inicia uma nova videochamada para um agendamento confirmado.

**Request Body:**
```json
{
  "agendamentoId": "guid"
}
```

**Response (200 OK):**
```json
{
  "agendamentoId": "guid",
  "callToken": "string",
  "callStartedAt": "2025-11-14T10:30:00Z",
  "hubUrl": "/videocall"
}
```

**Requisitos:**
- Agendamento deve estar confirmado (`Status = Confirmado`)
- Usuário deve ser o profissional ou cliente do agendamento
- Autenticação obrigatória (JWT)

---

### 2. Encerrar Videochamada

**POST** `/api/videocall/end`

Encerra uma videochamada ativa.

**Request Body:**
```json
{
  "agendamentoId": "guid"
}
```

**Response (200 OK):**
```json
{
  "message": "Chamada encerrada com sucesso.",
  "durationMinutes": 45
}
```

**Efeito:**
- Notifica todos os participantes via SignalR
- Registra data/hora de término
- Calcula duração da chamada

---

### 3. Status da Videochamada

**GET** `/api/videocall/status/{agendamentoId}`

Verifica o status de uma videochamada.

**Response (200 OK):**
```json
{
  "agendamentoId": "guid",
  "isActive": true,
  "callStartedAt": "2025-11-14T10:30:00Z",
  "callEndedAt": null,
  "durationMinutes": null
}
```

---

### 4. Validar Acesso

**GET** `/api/videocall/validate/{agendamentoId}`

Valida se o usuário pode acessar uma chamada específica.

**Response (200 OK):**
```json
{
  "hasAccess": true,
  "userType": "profissional",
  "isActive": true,
  "callToken": "string",
  "agendamentoId": "guid",
  "profissionalId": "guid",
  "clienteId": "guid"
}
```

---

## SignalR Hub - Eventos

### Conexão: `/videocall`

O Hub SignalR requer autenticação JWT. O token pode ser enviado via:
- Query string: `?access_token=YOUR_JWT_TOKEN`
- Header: `Authorization: Bearer YOUR_JWT_TOKEN`

### Métodos do Cliente → Servidor

#### `JoinCall(appointmentId, userId, userType)`
Entra em uma sala de chamada.

**Parâmetros:**
- `appointmentId` (string): ID do agendamento
- `userId` (string): ID do usuário
- `userType` (string): "profissional" ou "cliente"

**Resposta:**
- `UserJoined`: Notifica outros participantes
- `ExistingParticipants`: Lista de participantes já presentes

---

#### `SendOffer(appointmentId, offer, targetConnectionId)`
Envia uma oferta WebRTC para outro participante.

**Parâmetros:**
- `appointmentId` (string): ID do agendamento
- `offer` (string): SDP offer (JSON stringified)
- `targetConnectionId` (string): ID da conexão de destino

---

#### `SendAnswer(appointmentId, answer, targetConnectionId)`
Envia uma resposta WebRTC.

**Parâmetros:**
- `appointmentId` (string): ID do agendamento
- `answer` (string): SDP answer (JSON stringified)
- `targetConnectionId` (string): ID da conexão de destino

---

#### `SendIceCandidate(appointmentId, candidate, targetConnectionId)`
Envia um ICE candidate.

**Parâmetros:**
- `appointmentId` (string): ID do agendamento
- `candidate` (string): ICE candidate (JSON stringified)
- `targetConnectionId` (string): ID da conexão de destino

---

#### `ToggleAudio(appointmentId, enabled)`
Notifica mudança no estado do áudio.

---

#### `ToggleVideo(appointmentId, enabled)`
Notifica mudança no estado do vídeo.

---

#### `LeaveCall(appointmentId)`
Sai da sala de chamada.

---

### Eventos Servidor → Cliente

#### `UserJoined(userId, userType, connectionId)`
Novo usuário entrou na chamada.

#### `ExistingParticipants(participants)`
Lista de participantes já na chamada quando você entra.

#### `ReceiveOffer(offer, connectionId)`
Recebe uma oferta WebRTC de outro participante.

#### `ReceiveAnswer(answer, connectionId)`
Recebe uma resposta WebRTC.

#### `ReceiveIceCandidate(candidate)`
Recebe um ICE candidate.

#### `UserToggledAudio(connectionId, enabled)`
Usuário ligou/desligou o áudio.

#### `UserToggledVideo(connectionId, enabled)`
Usuário ligou/desligou o vídeo.

#### `UserLeft(connectionId)`
Usuário saiu da chamada.

#### `CallEnded()`
A chamada foi encerrada.

---

## Fluxo de Uso

### 1. Iniciar Chamada

```
Cliente/Profissional → POST /api/videocall/initiate
                    ← CallToken + HubUrl
```

### 2. Conectar ao SignalR Hub

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/videocall", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

await connection.start();
```

### 3. Entrar na Sala

```javascript
await connection.invoke("JoinCall", agendamentoId, userId, userType);
```

### 4. Estabelecer Conexão WebRTC

```javascript
// Quando receber UserJoined, criar oferta
connection.on("UserJoined", async (userId, userType, connectionId) => {
    const offer = await peerConnection.createOffer();
    await peerConnection.setLocalDescription(offer);
    await connection.invoke("SendOffer", agendamentoId, 
        JSON.stringify(offer), connectionId);
});

// Quando receber oferta, criar resposta
connection.on("ReceiveOffer", async (offer, connectionId) => {
    await peerConnection.setRemoteDescription(JSON.parse(offer));
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);
    await connection.invoke("SendAnswer", agendamentoId, 
        JSON.stringify(answer), connectionId);
});

// Quando receber resposta
connection.on("ReceiveAnswer", async (answer) => {
    await peerConnection.setRemoteDescription(JSON.parse(answer));
});

// ICE candidates
peerConnection.onicecandidate = (event) => {
    if (event.candidate) {
        connection.invoke("SendIceCandidate", agendamentoId, 
            JSON.stringify(event.candidate), targetConnectionId);
    }
};

connection.on("ReceiveIceCandidate", async (candidate) => {
    await peerConnection.addIceCandidate(JSON.parse(candidate));
});
```

### 5. Encerrar Chamada

```javascript
await fetch('/api/videocall/end', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ agendamentoId })
});

await connection.invoke("LeaveCall", agendamentoId);
await connection.stop();
```

---

## Configuração CORS

Para permitir acesso de diferentes origens, configure o CORS no `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("videocall", p =>
        p.WithOrigins("https://seu-dominio.com")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()); // Necessário para SignalR
});

app.UseCors("videocall");
```

---

## Segurança

1. **Autenticação JWT obrigatória** em todos os endpoints e no Hub
2. **Validação de acesso**: Apenas profissional e cliente do agendamento podem participar
3. **Status do agendamento**: Apenas agendamentos confirmados podem iniciar chamadas
4. **Tokens únicos**: Cada chamada recebe um token único
5. **Logs**: Todas as ações são registradas via Serilog

---

## Próximos Passos (Frontend)

1. Implementar cliente JavaScript/TypeScript com SignalR
2. Configurar WebRTC com ICE servers (STUN/TURN)
3. Criar interface de usuário para videochamada
4. Adicionar controles de áudio/vídeo
5. Implementar indicadores de conexão
6. Adicionar suporte a reconexão automática

---

## Dependências

- ASP.NET Core 9.0
- Microsoft.AspNetCore.SignalR
- Entity Framework Core 9.0
- JWT Bearer Authentication

---

## Testes

Para testar a API, você pode usar o Swagger UI disponível em `/swagger` quando em desenvolvimento.

Para testar o SignalR Hub, recomenda-se usar uma ferramenta como [SignalR Client](https://www.npmjs.com/package/@microsoft/signalr) ou implementar um cliente de teste.

---

## Limitações Atuais

- Suporte apenas para chamadas 1:1 (profissional ↔ cliente)
- Sem gravação de chamadas
- Sem controle de qualidade adaptativa
- Requer configuração de TURN server para funcionar atrás de NATs restritivos

---

## Melhorias Futuras

- [ ] Adicionar gravação de chamadas
- [ ] Implementar qualidade adaptativa de vídeo
- [ ] Adicionar chat durante a chamada
- [ ] Suporte a compartilhamento de tela
- [ ] Estatísticas de qualidade da chamada
- [ ] Notificações push para início de chamada
- [ ] Sala de espera
- [ ] Histórico de chamadas

