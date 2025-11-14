# 🔧 Correção: Vídeo Não Aparece (Erro de Estado WebRTC)

## 🐛 Problema Identificado

**Sintomas:**
- ✅ Ambos conectaram ao SignalR
- ❌ Vídeo não aparece
- ❌ Erro: "Called in wrong state: stable (INVALID_STATE)"
- ❌ Erro: "Failed to set remote answer sdp: Called in wrong state: stable"

**Causa Raiz:**
A lógica de negociação WebRTC estava criando **ofertas duplicadas** e tentando definir respostas quando a conexão já estava em estado "stable".

---

## ✅ Correções Aplicadas

### 1. **Frontend: Lógica de Negociação WebRTC** ⭐

**Arquivo**: `AppFitNutri/Views/VideoCallPage.xaml.cs`

#### Mudança 1: `setupHubHandlers()` - Evitar Ofertas Duplicadas

**Antes (ERRADO):**
```javascript
hubConnection.on('UserJoined', async (userId, userType, connectionId) => {
    await createPeerConnection(connectionId, true); // Ambos criavam oferta!
});

hubConnection.on('ExistingParticipants', async (participants) => {
    for (const p of participants) {
        await createPeerConnection(p.connectionId, true); // Mais ofertas!
    }
});
```

**Depois (CORRETO):**
```javascript
hubConnection.on('UserJoined', async (userId, userType, connectionId) => {
    // Apenas quem JÁ estava cria oferta
    if (hubConnection.connectionId < connectionId) {
        await createPeerConnection(connectionId, true);
    } else {
        console.log('Aguardando oferta do outro peer');
    }
});

hubConnection.on('ExistingParticipants', async (participants) => {
    // NÃO cria ofertas aqui - aguarda UserJoined
    console.log('Lista de participantes:', participants);
});
```

#### Mudança 2: `handleAnswer()` - Verificar Estado

**Adicionado:**
```javascript
async function handleAnswer(answerJson, fromConnectionId) {
    const pc = peerConnections.get(fromConnectionId);
    
    // Verificar estado antes de processar
    if (pc.signalingState === 'stable') {
        console.warn('PC já está em stable, ignorando resposta duplicada');
        return; // ← IMPORTANTE!
    }
    
    if (pc.signalingState !== 'have-local-offer') {
        console.error('Estado inesperado:', pc.signalingState);
        return;
    }
    
    await pc.setRemoteDescription(new RTCSessionDescription(answer));
}
```

#### Mudança 3: `handleIceCandidate()` - Usar fromConnectionId

**Antes:**
```javascript
async function handleIceCandidate(candidateJson) {
    for (const [, pc] of peerConnections) {
        await pc.addIceCandidate(...); // Para TODOS os peers!
    }
}
```

**Depois:**
```javascript
async function handleIceCandidate(candidateJson, fromConnectionId) {
    const pc = peerConnections.get(fromConnectionId); // Apenas o peer correto
    
    if (!pc) {
        console.warn('Peer connection não encontrado');
        return;
    }
    
    if (pc.remoteDescription) {
        await pc.addIceCandidate(new RTCIceCandidate(candidate));
    }
}
```

#### Mudança 4: Logs Detalhados

Adicionados console.logs em cada etapa para debug:
- ✅ "Criando peer connection..."
- ✅ "Oferta enviada!"
- ✅ "Resposta recebida"
- ✅ "Connection state: connected"

---

### 2. **Backend: VideoCallHub** ⭐

**Arquivo**: `Fitnutri/Application/VideoCallHub.cs`

**Antes:**
```csharp
public async Task SendIceCandidate(string appointmentId, string candidate, string targetConnectionId)
{
    await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", candidate);
    // ❌ Faltava Context.ConnectionId
}
```

**Depois:**
```csharp
public async Task SendIceCandidate(string appointmentId, string candidate, string targetConnectionId)
{
    await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", candidate, Context.ConnectionId);
    // ✅ Agora envia de quem veio
}
```

---

## 🔄 Fluxo Correto Agora

### Cenário: Profissional (iOS) + Paciente (Android)

1. **Profissional entra primeiro:**
   - Conecta ao SignalR
   - Aguarda outro usuário

2. **Paciente entra depois:**
   - Conecta ao SignalR
   - SignalR notifica Profissional: `UserJoined`

3. **Profissional (connectionId menor):**
   - Cria PeerConnection
   - Cria Oferta
   - Envia Oferta → Paciente

4. **Paciente recebe Oferta:**
   - Cria PeerConnection
   - Define Remote Description (oferta)
   - Cria Resposta
   - Envia Resposta → Profissional

5. **Profissional recebe Resposta:**
   - ✅ Verifica estado: `have-local-offer`
   - Define Remote Description (resposta)
   - Estado vira: `stable`

6. **ICE Candidates:**
   - Cada um envia candidates para o outro
   - Apenas o peer correto recebe cada candidate

7. **✅ Vídeo Conectado!**
   - Connection state: `connected`
   - Vídeo remoto aparece

---

## 🚀 Como Testar

### 1. Deploy do Backend

```bash
cd Fitnutri
dotnet clean
dotnet build
# Fazer deploy para produção
```

### 2. Rebuild do App

```bash
cd AppFitNutri
dotnet clean
dotnet build -f net8.0-ios
dotnet build -f net8.0-android
```

### 3. Testar Videochamada

**Dispositivo 1 (iOS Simulator - Profissional):**
1. Login como profissional
2. Abrir agendamento confirmado
3. Clicar "🎥 Iniciar Videochamada"
4. Permitir câmera/microfone
5. **Aguardar** outro usuário

**Dispositivo 2 (Android - Paciente):**
1. Login como paciente
2. Abrir mesmo agendamento
3. Clicar "🎥"
4. Permitir câmera/microfone
5. **Vídeo deve aparecer em ambos!**

---

## 📊 Logs Esperados (Sucesso)

### No Profissional (iOS):
```
✅ Conectando ao hub: https://api.fit-nutri.com/videocall
✅ SignalR conectado! Estado: Connected
✅ Entrou na sala com sucesso
✅ Usuário entrou: [paciente-id] connectionId: abc123
✅ Criando peer connection como iniciador
✅ Criando oferta...
✅ Oferta enviada!
✅ Recebeu resposta de: abc123
✅ Definindo remote description (resposta)
✅ Remote description definido! Estado: stable
✅ Connection state: connected
✅ ICE connection state: connected
✅ ✅ Peer conectado com sucesso!
```

### No Paciente (Android):
```
✅ Conectando ao hub: https://api.fit-nutri.com/videocall
✅ SignalR conectado! Estado: Connected
✅ Entrou na sala com sucesso
✅ Lista de participantes: 1
✅ Aguardando oferta do outro peer
✅ Recebeu oferta de: xyz789
✅ Criando nova peer connection para processar oferta
✅ Definindo remote description (oferta)
✅ Criando resposta...
✅ Resposta enviada!
✅ Track remoto recebido: video
✅ Track remoto recebido: audio
✅ Conectando stream remoto ao vídeo
✅ Connection state: connected
✅ ✅ Peer conectado com sucesso!
```

---

## 🔍 Se o Vídeo Ainda Não Aparecer

### Verificar no Console do Chrome (Android)

1. `chrome://inspect`
2. Selecionar WebView do app
3. Ver logs do console

**Se ver:**
- ❌ "Estado inesperado: stable" → Ainda há ofertas duplicadas
- ❌ "Peer connection não encontrado" → fromConnectionId não está correto
- ✅ "✅ Peer conectado com sucesso!" → WebRTC funcionou!
- ❌ Vídeo não aparece mesmo conectado → Problema de tracks/stream

### Verificar Tracks

Se conectou mas vídeo não aparece:

```javascript
// No console do Chrome DevTools
const remoteVideo = document.getElementById('remoteVideo');
console.log('Remote video srcObject:', remoteVideo.srcObject);
console.log('Remote tracks:', remoteVideo.srcObject?.getTracks());
```

**Esperado:**
```
Remote video srcObject: MediaStream
Remote tracks: [VideoTrack, AudioTrack]
```

---

## 📁 Arquivos Modificados

### Frontend (App Mobile):
✅ `AppFitNutri/Views/VideoCallPage.xaml.cs`
- setupHubHandlers() - lógica de ofertas
- handleAnswer() - verificação de estado
- handleIceCandidate() - usar fromConnectionId correto
- Logs detalhados em todas as etapas

### Backend (API):
✅ `Fitnutri/Application/VideoCallHub.cs`
- SendIceCandidate() - adicionar Context.ConnectionId

---

## ✅ Status

- ✅ Lógica de negociação WebRTC corrigida
- ✅ Ofertas duplicadas eliminadas
- ✅ Estados verificados antes de processar
- ✅ ICE candidates enviados para peer correto
- ✅ Logs detalhados para debug
- ⏳ **Aguardando deploy e teste**

---

## 🎯 Expectativa

Após deploy e rebuild:
1. ✅ Ambos conectam ao SignalR
2. ✅ Um cria oferta, outro cria resposta
3. ✅ Estados corretos durante negociação
4. ✅ ICE candidates trocados
5. ✅ **Vídeo aparece em ambos os lados!** 🎥

---

**O vídeo deve funcionar agora!** 🚀

