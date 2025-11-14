# ✅ Checklist - Testar Videochamada

## Antes de Iniciar

- [ ] A API está rodando (`dotnet run` na pasta Fitnutri)
- [ ] Você tem um agendamento com status `Confirmado`
- [ ] Você tem tokens JWT válidos (profissional e paciente)
- [ ] Você configurou o `x-api-key` no `appsettings.json`

## Configuração Inicial

### 1. Verifique o appsettings.json

```json
"ApiKey": {
  "Enabled": true,
  "Header": "x-api-key",
  "Key": "COLOQUE_SUA_CHAVE_AQUI",
  "BypassPaths": [ "/videocall" ]
}
```

- [ ] Campo `Key` está preenchido
- [ ] Campo `BypassPaths` contém `"/videocall"`

### 2. Acesse a Página de Teste

- [ ] Abra: `https://localhost:7001/videocall-test.html`
- [ ] Ou: `https://localhost:7065/videocall-test.html` (dependendo da porta)

## Teste Básico (1 Participante)

### 3. Preencha os Campos

- [ ] **URL da API**: `https://localhost:7001`
- [ ] **x-api-key**: Cole a chave do appsettings.json
- [ ] **Token JWT**: Cole o token obtido no login
- [ ] **ID do Agendamento**: GUID do agendamento confirmado
- [ ] **User ID**: ID do usuário autenticado
- [ ] **Tipo de Usuário**: Profissional ou Paciente

### 4. Inicie a Chamada

- [ ] Clique em "Iniciar Videochamada"
- [ ] Verifique o log: deve mostrar "Iniciando videochamada via API..."
- [ ] Verifique o log: deve mostrar "Chamada iniciada! Token: ..."
- [ ] Verifique o log: deve mostrar "Conectando ao SignalR Hub..."
- [ ] Permita acesso à câmera/microfone quando solicitado
- [ ] Verifique o log: deve mostrar "Conectado ao SignalR Hub!"
- [ ] Verifique o log: deve mostrar "Stream local obtido com sucesso!"
- [ ] Verifique o status: deve mostrar "Conectado" (bolinha verde)
- [ ] Verifique o vídeo local: deve mostrar sua câmera

## Teste Completo (2 Participantes)

### 5. Abra Segunda Aba/Navegador

- [ ] Abra a mesma URL em outra aba ou navegador diferente
- [ ] Use um token JWT **diferente** (outro usuário)
- [ ] Use o **mesmo ID de agendamento**

### 6. Na Primeira Aba (Profissional)

- [ ] Preencha com token do profissional
- [ ] Clique em "Iniciar Videochamada"
- [ ] Aguarde conexão e acesso à câmera

### 7. Na Segunda Aba (Paciente)

- [ ] Preencha com token do paciente
- [ ] **NÃO** precisa preencher x-api-key para "Conectar ao SignalR"
- [ ] Clique em "Conectar ao SignalR"
- [ ] Aguarde conexão e acesso à câmera

### 8. Verifique a Comunicação

- [ ] Log mostra: "Usuário entrou: [userId]"
- [ ] Log mostra: "Oferta enviada!"
- [ ] Log mostra: "Resposta recebida de [connectionId]"
- [ ] Log mostra: "Track remoto recebido"
- [ ] Log mostra: "Stream remoto conectado ao vídeo!"
- [ ] **Vídeo remoto aparece** em ambas as abas
- [ ] **Áudio funciona** em ambas as direções
- [ ] Contador de participantes mostra "2 participante(s)"

## Teste de Controles

### 9. Teste os Botões

- [ ] Clique em "Ativar/Desativar Áudio"
  - [ ] Seu microfone deve mutar/desmutar
  - [ ] Log mostra: "Áudio ativado/desativado"
  
- [ ] Clique em "Ativar/Desativar Vídeo"
  - [ ] Sua câmera deve ligar/desligar
  - [ ] Log mostra: "Vídeo ativado/desativado"
  
- [ ] Clique em "Encerrar Chamada"
  - [ ] Conexão deve fechar
  - [ ] Vídeos devem parar
  - [ ] Status deve mudar para "Desconectado"
  - [ ] Log mostra: "Chamada encerrada!"

## Troubleshooting

### ❌ Erro: "API key ausente" no /videocall/negotiate

**Solução**: 
- Verifique se o `appsettings.json` tem `"BypassPaths": [ "/videocall" ]`
- Reinicie a API após alterar o appsettings.json

### ❌ Erro 401 no SignalR

**Solução**:
- Verifique se o token JWT é válido
- Verifique se o token não expirou
- Verifique se o usuário tem permissão para o agendamento

### ❌ Vídeo não aparece

**Possíveis causas**:
- Câmera bloqueada pelo navegador
- Outro aplicativo usando a câmera
- Firewall bloqueando WebRTC
- Verifique o console do navegador (F12)

### ❌ "Horário indisponível" ao iniciar

**Solução**:
- Verifique se o agendamento está com status `Confirmado`
- Verifique se você é o profissional ou paciente daquele agendamento

### ❌ ICE connection state: failed

**Possíveis causas**:
- Problema de NAT/Firewall
- Adicione servidores TURN para produção
- Verifique se ambos estão na mesma rede (teste local)

## Logs Importantes

### ✅ Sucesso - Você deve ver:

```
[HH:mm:ss] Chamada iniciada! Token: abc123...
[HH:mm:ss] Conectado ao SignalR Hub!
[HH:mm:ss] Stream local obtido com sucesso!
[HH:mm:ss] Entrou na sala de chamada: [guid]
[HH:mm:ss] Usuário entrou: [userId] (profissional/paciente)
[HH:mm:ss] Oferta enviada!
[HH:mm:ss] Resposta recebida de [connectionId]
[HH:mm:ss] Track remoto recebido de [connectionId]
[HH:mm:ss] Stream remoto conectado ao vídeo!
[HH:mm:ss] Connection state: connected
[HH:mm:ss] ICE connection state: connected
```

### ❌ Erro - Se ver isso, há problema:

```
[HH:mm:ss] Erro: API key ausente
[HH:mm:ss] Erro ao conectar: ...
[HH:mm:ss] ICE connection state: failed
[HH:mm:ss] Connection state: failed
```

## 🎉 Teste Concluído!

Se todos os itens acima foram marcados, sua videochamada está funcionando perfeitamente!

---

**Próximos Passos**:
1. Integrar com a aplicação mobile (MAUI)
2. Melhorar UI/UX
3. Adicionar recursos extras (chat, compartilhamento de tela, etc.)
4. Configurar servidores TURN para produção
5. Implementar gravação de chamadas (opcional)

