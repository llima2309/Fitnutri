# ✅ Checklist Final - Videochamada MAUI

## 📋 Validação Pré-Publicação

### 1. Arquivos Criados
- [ ] `AppFitNutri/Services/VideoCallService.cs` existe
- [ ] `AppFitNutri/Views/VideoCallPage.xaml.cs` existe
- [ ] `AppFitNutri/Converters/StatusEqualsConverter.cs` existe
- [ ] `MAUI_VIDEOCALL_IMPLEMENTATION.md` criado

### 2. Arquivos Modificados
- [ ] `AgendamentosProfissionalViewModel.cs` atualizado
- [ ] `MeusAgendamentosViewModel.cs` atualizado
- [ ] `AgendamentosProfissionalPage.xaml` atualizado
- [ ] `MeusAgendamentosPage.xaml` atualizado
- [ ] `MauiProgram.cs` atualizado
- [ ] `App.xaml` atualizado

### 3. Compilação
- [ ] Projeto compila sem erros
- [ ] Apenas warnings (não impedem execução)
- [ ] Todas as dependências resolvidas

### 4. Configuração

#### MauiProgram.cs
- [ ] `VideoCallService` registrado no DI
- [ ] `VideoCallPage` registrada no DI
- [ ] HttpClient configurado com URL correta
- [ ] x-api-key configurada

#### App.xaml
- [ ] `StatusEqualsConverter` nos recursos
- [ ] Converters antigos mantidos

### 5. API Backend
- [ ] Endpoint `/api/videocall/initiate` funciona
- [ ] Endpoint `/api/videocall/status/{id}` funciona
- [ ] SignalR Hub `/videocall` configurado
- [ ] BypassPaths inclui "/videocall" no appsettings.json

---

## 🧪 Testes de Integração

### Teste 1: Profissional Inicia Chamada

**Dispositivo 1 (Profissional):**
1. [ ] Login como profissional
2. [ ] Navegar para "Meus Agendamentos"
3. [ ] Verificar se botão "🎥 Iniciar Videochamada" aparece APENAS em agendamentos confirmados
4. [ ] Clicar no botão
5. [ ] Verificar se abre página de videochamada
6. [ ] Permitir acesso à câmera/microfone
7. [ ] Verificar se vídeo local aparece (canto superior direito)
8. [ ] Verificar se status mostra "Conectado"

**Resultado Esperado:**
- ✅ Página de videochamada aberta
- ✅ Vídeo local funcionando
- ✅ Status "Conectado"
- ✅ Log no console sem erros

---

### Teste 2: Paciente se Junta à Chamada

**Dispositivo 2 (Paciente):**
1. [ ] Login como paciente
2. [ ] Navegar para "Meus Agendamentos"
3. [ ] Encontrar o MESMO agendamento do teste anterior
4. [ ] Verificar se botão "🎥" está habilitado
5. [ ] Clicar no botão
6. [ ] Permitir acesso à câmera/microfone
7. [ ] Verificar se vídeo local aparece
8. [ ] Verificar se vídeo remoto aparece (profissional)
9. [ ] Testar áudio bidirecional

**Resultado Esperado:**
- ✅ Conexão automática à chamada existente
- ✅ Vídeo local + remoto funcionando
- ✅ Áudio bidirecional funcionando
- ✅ Sem erro "chamada já iniciada"

---

### Teste 3: Verificação de Status (Evita Duplicação)

**Cenário: Ambos tentam iniciar ao mesmo tempo**

**Dispositivo 1:**
1. [ ] Profissional clica em "Iniciar Videochamada"
2. [ ] Aguardar conexão

**Dispositivo 2 (simultaneamente):**
1. [ ] Paciente clica em "🎥"
2. [ ] Verificar se detecta chamada já ativa
3. [ ] Conectar automaticamente

**Resultado Esperado:**
- ✅ Apenas UMA chamada criada no banco
- ✅ Segundo usuário conecta à existente
- ✅ Sem erro ou duplicação
- ✅ Log mostra "Chamada já ativa, conectando..."

---

### Teste 4: Controles Durante Chamada

**Com chamada ativa em ambos dispositivos:**

1. [ ] Clicar em "🎤" (Toggle Áudio)
   - ✅ Microfone desliga
   - ✅ Outro usuário para de ouvir
   - ✅ Botão muda de cor

2. [ ] Clicar em "📹" (Toggle Vídeo)
   - ✅ Câmera desliga
   - ✅ Vídeo local some
   - ✅ Outro usuário vê tela preta

3. [ ] Clicar em "📞" (Encerrar)
   - ✅ Confirma encerramento
   - ✅ Fecha página de videochamada
   - ✅ Volta para lista de agendamentos

---

### Teste 5: Botão Condicional

**Status = Pendente:**
- [ ] Botão de videochamada NÃO aparece
- [ ] Apenas botões Confirmar/Cancelar

**Status = Confirmado:**
- [ ] Botão de videochamada APARECE
- [ ] Botão está habilitado

**Status = Cancelado:**
- [ ] Botão de videochamada NÃO aparece
- [ ] Nenhum botão de ação

---

## 🔧 Testes de Erro

### Erro 1: Token Expirado
**Passos:**
1. [ ] Usar token JWT expirado
2. [ ] Tentar iniciar videochamada

**Resultado Esperado:**
- ✅ Mensagem de erro clara
- ✅ Não trava o app
- ✅ Sugere fazer login novamente

### Erro 2: Agendamento Não Confirmado
**Passos:**
1. [ ] Agendamento com status "Pendente"
2. [ ] Tentar iniciar videochamada via código direto

**Resultado Esperado:**
- ✅ Botão não aparece (prevenção via UI)
- ✅ Se forçar, API retorna erro
- ✅ Mensagem amigável ao usuário

### Erro 3: Sem Permissão de Câmera/Microfone
**Passos:**
1. [ ] Negar permissões
2. [ ] Tentar iniciar videochamada

**Resultado Esperado:**
- ✅ Mensagem de erro no WebView
- ✅ Instrui usuário a habilitar permissões
- ✅ Não trava o app

### Erro 4: Sem Internet
**Passos:**
1. [ ] Desligar Wi-Fi/dados móveis
2. [ ] Tentar iniciar videochamada

**Resultado Esperado:**
- ✅ Timeout após 30 segundos
- ✅ Mensagem de erro de conexão
- ✅ Permite tentar novamente

---

## 📱 Testes de Dispositivos

### Android
- [ ] Testado em emulador Android
- [ ] Testado em dispositivo físico
- [ ] Permissões solicitadas corretamente
- [ ] Câmera frontal/traseira funciona
- [ ] Áudio funciona

### iOS
- [ ] Testado em simulador iOS
- [ ] Testado em dispositivo físico
- [ ] Permissões solicitadas corretamente
- [ ] Câmera frontal/traseira funciona
- [ ] Áudio funciona

---

## 🌐 Testes de Rede

### Wi-Fi
- [ ] Conexão estável
- [ ] Qualidade de vídeo boa
- [ ] Latência baixa

### 4G/5G
- [ ] Conexão funciona
- [ ] Qualidade ajustada
- [ ] Sem travamentos

### Mudança de Rede
- [ ] Wi-Fi → 4G durante chamada
- [ ] Reconexão automática funciona
- [ ] Chamada não cai

---

## 🚀 Preparação para Produção

### Configuração
- [ ] URL da API configurada para produção
- [ ] x-api-key de produção configurada
- [ ] Certificados SSL válidos
- [ ] Servidores TURN configurados (opcional)

### Segurança
- [ ] x-api-key não hardcoded
- [ ] JWT tokens validados
- [ ] Apenas participantes do agendamento acessam
- [ ] HTTPS obrigatório

### Performance
- [ ] Tempo de conexão < 3 segundos
- [ ] Qualidade de vídeo adaptável
- [ ] Uso de CPU/memória aceitável
- [ ] Bateria não drena excessivamente

---

## 📊 Métricas de Sucesso

### Funcionalidade
- ✅ 100% das chamadas iniciadas conectam
- ✅ < 1% de taxa de erro
- ✅ Reconexão automática funciona em 90% dos casos

### Performance
- ✅ Latência média < 200ms
- ✅ Qualidade de vídeo HD quando possível
- ✅ Áudio sincronizado com vídeo

### UX
- ✅ Interface intuitiva
- ✅ Mensagens de erro claras
- ✅ Feedback visual imediato

---

## ✅ Aprovação Final

Após completar todos os testes acima:

- [ ] Todos os testes passaram
- [ ] Nenhum bug crítico encontrado
- [ ] Performance aceitável
- [ ] Documentação completa
- [ ] Pronto para publicar

**Responsável pelo Teste:** _________________

**Data:** ___/___/2025

**Assinatura:** _________________

---

## 🐛 Bugs Encontrados

Use esta seção para documentar bugs durante os testes:

| ID | Descrição | Severidade | Status |
|----|-----------|------------|--------|
| 1  |           | Baixa/Média/Alta | Aberto/Corrigido |
| 2  |           |            |        |
| 3  |           |            |        |

---

## 📝 Notas Adicionais

_Use este espaço para notas importantes encontradas durante os testes:_

---

**Status Geral:** 🟡 Aguardando Testes
- 🔴 Falhou
- 🟡 Em Teste
- 🟢 Aprovado

