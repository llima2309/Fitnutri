# HomePage para Nutricionistas - FitNutri

## Descrição
HomePage personalizada para profissionais nutricionistas, exibindo:
- Dashboard com estatísticas (agendamentos hoje, semana, total de pacientes, dietas ativas)
- Ações rápidas (novo agendamento, criar dieta)
- Lista de próximos agendamentos

## Arquivos Criados

### Models
- `PacienteDto.cs` - DTO para representar pacientes
- `DashboardNutricionistaDto.cs` - DTO para dados do dashboard

### Services
- `ProfissionalDashboardService.cs` - Serviço para buscar dados do dashboard do profissional
  - `GetDashboardAsync()` - Busca estatísticas completas
  - `GetAgendamentosProfissionalAsync()` - Busca agendamentos do profissional
  - `GetPacientesAsync()` - Busca lista de pacientes

### ViewModel
- `HomeNutricionistaViewModel.cs` - ViewModel com lógica de apresentação
  - Commands: VerTodosAgendamentos, VerPacientes, VerDietas, NovoAgendamento, NovaDieta
  - Propriedades observáveis para binding

### Views
- `HomeNutricionistaPage.xaml` - Interface visual da página
- `HomeNutricionistaPage.xaml.cs` - Code-behind

## Como Usar

### Navegação
Para navegar até a página do nutricionista:

```csharp
await Shell.Current.GoToAsync(nameof(HomeNutricionistaPage));
```

### Exemplo de Uso no Login
Após login, verificar o tipo de perfil do usuário e navegar:

```csharp
// No LoginViewModel ou após autenticação bem-sucedida
var userProfile = await _profileService.GetProfileAsync();

if (userProfile?.TipoProfissional == 2) // Nutricionista
{
    await Shell.Current.GoToAsync($"//{nameof(HomeNutricionistaPage)}");
}
else
{
    await Shell.Current.GoToAsync($"//{nameof(HomePage)}"); // HomePage padrão
}
```

## Endpoints da API (a serem implementados)

Os seguintes endpoints precisam ser criados na API para funcionalidade completa:

1. **GET /agendamentos/profissional/me** - Retorna agendamentos do profissional autenticado
2. **GET /profissional/pacientes** - Retorna lista de pacientes do profissional
3. **GET /profissional/dashboard** - Retorna estatísticas consolidadas (opcional)

## Funcionalidades Futuras

### A implementar:
- [ ] Página de lista completa de agendamentos do profissional
- [ ] Página de lista de pacientes com detalhes
- [ ] Página de gerenciamento de dietas
- [ ] Criar/editar dietas personalizadas
- [ ] Visualizar histórico de consultas por paciente
- [ ] Notificações de novos agendamentos
- [ ] Chat/mensagens com pacientes

## Customização

### Cores do Tema
As cores podem ser ajustadas no arquivo XAML:
- Agendamentos Hoje: `#2196F3` (azul)
- Agendamentos Semana: `#FF9800` (laranja)
- Pacientes: `#4CAF50` (verde)
- Dietas: `#9C27B0` (roxo)

### Layout
O layout usa um Grid 2x2 para os cards de estatísticas, responsivo para diferentes tamanhos de tela.

## Observações

- O serviço atualmente retorna listas vazias quando os endpoints não existem (graceful degradation)
- A autenticação é obrigatória via JWT token
- O RefreshView permite pull-to-refresh para atualizar os dados
- Todos os commands têm tratamento de exceções para evitar crashes

