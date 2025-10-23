using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppFitNutri.Core.Models;
using AppFitNutri.Services;

namespace AppFitNutri.ViewModel;

public class UserProfileRegistrationViewModel : INotifyPropertyChanged
{
    private readonly IUserProfileService _userProfileService;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    // Dados pessoais
    private string _nomeCompleto = string.Empty;
    private string _cpf = string.Empty;
    private string _rg = string.Empty;
    private string _telefone = string.Empty;
    private GeneroOption? _selectedGenero;
    private DateTime _dataNascimento = DateTime.Now.AddYears(-18);
    private string _crn = string.Empty;

    // Endereço
    private string _cep = string.Empty;
    private EstadoOption? _selectedEstado;
    private string _endereco = string.Empty;
    private string _numero = string.Empty;
    private string _cidade = string.Empty;
    private string _complemento = string.Empty;
    private string _bairro = string.Empty;

    // Modo de edição
    private bool _isEditMode = false;
    private UserProfileResponse? _existingProfile;

    // Collections
    public ObservableCollection<GeneroOption> GeneroOptions { get; } = new();
    public ObservableCollection<EstadoOption> EstadoOptions { get; } = new();

    public UserProfileRegistrationViewModel(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
        
        SaveCommand = new Command(async () => await SaveProfile(), () => !IsLoading);
        SearchCepCommand = new Command<string>(async (cep) => await SearchCep(cep));
        
        _ = LoadOptionsAndProfileAsync();
    }

    private async Task LoadOptionsAndProfileAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine("==> Carregando opções e verificando perfil existente");

            // Carregar opções e verificar se já existe perfil em paralelo
            var generoTask = _userProfileService.GetGeneroOptionsAsync();
            var estadoTask = _userProfileService.GetEstadoOptionsAsync();
            var profileTask = _userProfileService.GetProfileAsync();

            var generoOptions = await generoTask;
            var estadoOptions = await estadoTask;
            var existingProfile = await profileTask;

            // Carregar opções
            GeneroOptions.Clear();
            foreach (var option in generoOptions)
            {
                GeneroOptions.Add(option);
            }

            EstadoOptions.Clear();
            foreach (var option in estadoOptions)
            {
                EstadoOptions.Add(option);
            }

            // Verificar se já existe perfil e entrar no modo de edição
            if (existingProfile != null)
            {
                _isEditMode = true;
                _existingProfile = existingProfile;
                LoadExistingProfileData();
                System.Diagnostics.Debug.WriteLine("==> Modo de edição ativado - perfil existente encontrado");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("==> Modo de criação ativado - nenhum perfil existente");
            }

            // Notificar mudanças nas propriedades dinâmicas
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(ButtonText));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar dados: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"==> ERRO no LoadOptionsAndProfileAsync: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand SearchCepCommand { get; }
    public ICommand ValidateCpfCommand => new Command(() => { CPF = FormatCpf(CPF); });
    public ICommand ValidateCepCommand => new Command(() => { CEP = FormatCep(CEP); });
    public ICommand ValidateRgCommand => new Command(() => { RG = FormatRg(RG); });
    public ICommand ValidateTelefoneCommand => new Command(() => { Telefone = FormatTelefone(Telefone); });
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    // Propriedades dos dados pessoais
    public string NomeCompleto
    {
        get => _nomeCompleto;
        set
        {
            _nomeCompleto = value;
            OnPropertyChanged();
        }
    }

    public string CPF
    {
        get => _cpf;
        set
        {
            _cpf =  value;
            OnPropertyChanged();
        }
    }

    public string RG
    {
        get => _rg;
        set
        {
            _rg = value;
            OnPropertyChanged();
        }
    }

    public string Telefone
    {
        get => _telefone;
        set
        {
            _telefone = value;
            OnPropertyChanged();
        }
    }

    public GeneroOption? SelectedGenero
    {
        get => _selectedGenero;
        set
        {
            _selectedGenero = value;
            OnPropertyChanged();
        }
    }

    public DateTime DataNascimento
    {
        get => _dataNascimento;
        set
        {
            _dataNascimento = value;
            OnPropertyChanged();
        }
    }

    public string CRN
    {
        get => _crn;
        set
        {
            _crn = value;
            OnPropertyChanged();
        }
    }

    // Propriedades de endereço
    public string CEP
    {
        get => _cep;
        set
        {
            _cep = value;
            OnPropertyChanged();
            
         
        }
    }

    public EstadoOption? SelectedEstado
    {
        get => _selectedEstado;
        set
        {
            _selectedEstado = value;
            OnPropertyChanged();
        }
    }

    public string Endereco
    {
        get => _endereco;
        set
        {
            _endereco = value;
            OnPropertyChanged();
        }
    }

    public string Numero
    {
        get => _numero;
        set
        {
            _numero = value;
            OnPropertyChanged();
        }
    }

    public string Cidade
    {
        get => _cidade;
        set
        {
            _cidade = value;
            OnPropertyChanged();
        }
    }

    public string Complemento
    {
        get => _complemento;
        set
        {
            _complemento = value;
            OnPropertyChanged();
        }
    }

    public string Bairro
    {
        get => _bairro;
        set
        {
            _bairro = value;
            OnPropertyChanged();
        }
    }

    private async Task LoadOptionsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine("==> Iniciando carregamento de opções");

            var generoTask = _userProfileService.GetGeneroOptionsAsync();
            var estadoTask = _userProfileService.GetEstadoOptionsAsync();

            var generoOptions = await generoTask;
            var estadoOptions = await estadoTask;

            System.Diagnostics.Debug.WriteLine($"==> Recebido {generoOptions?.Count ?? 0} opções de gênero");
            System.Diagnostics.Debug.WriteLine($"==> Recebido {estadoOptions?.Count ?? 0} opções de estado");

            GeneroOptions.Clear();
            foreach (var option in generoOptions)
            {
                System.Diagnostics.Debug.WriteLine($"==> Adicionando gênero: {option.Value} - {option.Display}");
                GeneroOptions.Add(option);
            }

            EstadoOptions.Clear();
            foreach (var option in estadoOptions)
            {
                EstadoOptions.Add(option);
            }

            System.Diagnostics.Debug.WriteLine($"==> GeneroOptions.Count final: {GeneroOptions.Count}");
            System.Diagnostics.Debug.WriteLine($"==> EstadoOptions.Count final: {EstadoOptions.Count}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar opções: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"==> ERRO no LoadOptionsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"==> Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchCep(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep) || cep.Length != 9)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine($"==> Buscando CEP: {cep}");
            var address = await _userProfileService.GetAddressByCepAsync(cep.Replace("-", ""));
            
            if (address != null)
            {
                System.Diagnostics.Debug.WriteLine($"==> Endereço retornado:");
                System.Diagnostics.Debug.WriteLine($"    Logradouro: '{address.Logradouro}'");
                System.Diagnostics.Debug.WriteLine($"    Cidade: '{address.Cidade}'");
                System.Diagnostics.Debug.WriteLine($"    Bairro: '{address.Bairro}'");
                System.Diagnostics.Debug.WriteLine($"    UF: '{address.UF}'");
                System.Diagnostics.Debug.WriteLine($"    Estado: '{address.Estado}'");
                System.Diagnostics.Debug.WriteLine($"    DDD: '{address.DDD}'");
                
                Endereco = address.Logradouro;
                Cidade = address.Cidade;  // Agora usando o campo correto
                Bairro = address.Bairro;
                
                System.Diagnostics.Debug.WriteLine($"==> Município definido como: '{address.Cidade}'");
                
                // Buscar o estado correspondente pela UF
                var estado = EstadoOptions.FirstOrDefault(e => e.Sigla == address.UF);
                if (estado != null)
                {
                    SelectedEstado = estado;
                    System.Diagnostics.Debug.WriteLine($"==> Estado selecionado: {estado.Display} ({estado.Sigla})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"==> Estado não encontrado para UF: '{address.UF}'");
                }
            }
            else
            {
                ErrorMessage = "CEP não encontrado";
                System.Diagnostics.Debug.WriteLine("==> CEP não encontrado na API");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao buscar CEP: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"==> Erro ao buscar CEP: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveProfile()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (!ValidateFields())
                return;

            if (_isEditMode)
            {
                // Modo de edição - usar UpdateProfileAsync
                var updateRequest = new UpdateUserProfileRequest
                {
                    NomeCompleto = NomeCompleto,
                    RG = string.IsNullOrWhiteSpace(RG) ? null : RG,
                    Telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone,
                    Genero = (Genero)SelectedGenero!.Value,
                    DataNascimento = DataNascimento,
                    CRN = string.IsNullOrWhiteSpace(CRN) ? null : CRN,
                    CEP = CEP,
                    Estado = (Estado)SelectedEstado!.Value,
                    Endereco = Endereco,
                    Numero = Numero,
                    Cidade = Cidade,
                    Complemento = string.IsNullOrWhiteSpace(Complemento) ? null : Complemento,
                    Bairro = Bairro
                };

                System.Diagnostics.Debug.WriteLine("==> Atualizando perfil existente");
                var result = await _userProfileService.UpdateProfileAsync(updateRequest);
                
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine("==> Perfil atualizado com sucesso!");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                // Modo de criação - usar CreateProfileAsync
                var createRequest = new CreateUserProfileRequest
                {
                    NomeCompleto = NomeCompleto,
                    CPF = CPF,
                    RG = string.IsNullOrWhiteSpace(RG) ? null : RG,
                    Telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone,
                    Genero = (Genero)SelectedGenero!.Value,
                    DataNascimento = DataNascimento,
                    CRN = string.IsNullOrWhiteSpace(CRN) ? null : CRN,
                    CEP = CEP,
                    Estado = (Estado)SelectedEstado!.Value,
                    Endereco = Endereco,
                    Numero = Numero,
                    Cidade = Cidade,
                    Complemento = string.IsNullOrWhiteSpace(Complemento) ? null : Complemento,
                    Bairro = Bairro
                };

                System.Diagnostics.Debug.WriteLine("==> Criando novo perfil");
                var result = await _userProfileService.CreateProfileAsync(createRequest);
                
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine("==> Perfil criado com sucesso!");
                    await Shell.Current.GoToAsync("//HomePage");
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao salvar perfil: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"==> ERRO ao salvar perfil: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"==> Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public bool IsEditMode => _isEditMode;
    public string PageTitle => _isEditMode ? "Editar Perfil" : "Cadastro de Perfil";
    public string ButtonText => _isEditMode ? "Salvar Alterações" : "Finalizar Cadastro";

    private bool ValidateFields()
    {
        if (string.IsNullOrWhiteSpace(NomeCompleto))
        {
            ErrorMessage = "Nome completo é obrigatório";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CPF) || CPF.Length != 14)
        {
            ErrorMessage = "CPF deve estar no formato 000.000.000-00";
            return false;
        }

        if (SelectedGenero == null)
        {
            ErrorMessage = "Selecione um gênero";
            return false;
        }

        if (DataNascimento >= DateTime.Now)
        {
            ErrorMessage = "Data de nascimento deve ser anterior à data atual";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CEP) || CEP.Length != 9)
        {
            ErrorMessage = "CEP deve estar no formato 00000-000";
            return false;
        }

        if (SelectedEstado == null)
        {
            ErrorMessage = "Selecione um estado";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Endereco))
        {
            ErrorMessage = "Endereço é obrigatório";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = "Número é obrigatório";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Cidade))
        {
            ErrorMessage = "Cidade é obrigatória";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Bairro))
        {
            ErrorMessage = "Bairro é obrigatório";
            return false;
        }

        return true;
    }

    private static string FormatCpf(string cpf)
    {
        // Remove tudo que não for dígito e valida entrada
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        
        // Limita a 11 dígitos
        if (digits.Length > 11)
            digits = digits.Substring(0, 11);
        
        if (digits.Length <= 3)
            return digits;
        if (digits.Length <= 6)
            return $"{digits[..3]}.{digits[3..]}";
        if (digits.Length <= 9)
            return $"{digits[..3]}.{digits[3..6]}.{digits[6..]}";
        
        return $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[9..]}";
    }

    private static string FormatCep(string cep)
    {
        // Remove tudo que não for dígito e valida entrada
        var digits = new string(cep.Where(char.IsDigit).ToArray());
        
        // Limita a 8 dígitos
        if (digits.Length > 8)
            digits = digits.Substring(0, 8);
        
        if (digits.Length <= 5)
            return digits;
        
        return $"{digits[..5]}-{digits[5..]}";
    }

    private static string FormatRg(string rg)
    {
        if (string.IsNullOrWhiteSpace(rg))
            return string.Empty;

        // Remove tudo que não for dígito, X ou x
        var cleaned = new string(rg.Where(c => char.IsDigit(c) || char.ToUpper(c) == 'X').ToArray());
        
        if (cleaned.Length == 0)
            return string.Empty;
            
        // Converte X para maiúsculo
        cleaned = cleaned.ToUpper();
        
        // Se tem menos de 9 caracteres, apenas formata os dígitos disponíveis
        if (cleaned.Length <= 8)
        {
            var digits = new string(cleaned.Where(char.IsDigit).ToArray());
            
            if (digits.Length <= 2)
            {
                return digits;
            }
            else if (digits.Length <= 5)
            {
                return $"{digits.Substring(0, 2)}.{digits.Substring(2)}";
            }
            else
            {
                var firstPart = digits.Substring(0, 2);
                var secondPart = digits.Length > 5 ? digits.Substring(2, 3) : digits.Substring(2);
                var thirdPart = digits.Length > 5 ? digits.Substring(5) : "";
                
                if (string.IsNullOrEmpty(thirdPart))
                {
                    return $"{firstPart}.{secondPart}";
                }
                else
                {
                    return $"{firstPart}.{secondPart}.{thirdPart}";
                }
            }
        }
        else
        {
            // RG completo com verificador - formato 00.000.000-X ou 00.000.000-00
            var mainDigits = new string(cleaned.Take(8).Where(char.IsDigit).ToArray());
            var verifier = cleaned.Substring(8);
            
            // Se não temos 8 dígitos principais, pega o que tiver
            if (mainDigits.Length < 8)
            {
                var allDigits = new string(cleaned.Where(char.IsDigit).ToArray());
                if (allDigits.Length >= 8)
                {
                    mainDigits = allDigits.Substring(0, 8);
                    // O verificador é o que sobrar (número ou X)
                    var remaining = cleaned.Substring(8);
                    verifier = remaining;
                }
            }
            
            // Garante que temos pelo menos 8 dígitos para formatar
            if (mainDigits.Length >= 8)
            {
                var formatted = $"{mainDigits.Substring(0, 2)}.{mainDigits.Substring(2, 3)}.{mainDigits.Substring(5, 3)}";
                
                if (!string.IsNullOrEmpty(verifier))
                {
                    // Limita o verificador a 2 caracteres
                    if (verifier.Length > 2)
                        verifier = verifier.Substring(0, 2);
                    
                    formatted += $"-{verifier}";
                }
                
                return formatted;
            }
            else
            {
                // Fallback: formata o que tiver
                return FormatRg(mainDigits);
            }
        }
    }

    private static string FormatTelefone(string telefone)
    {
        // Remove tudo que não for dígito
        var digits = new string(telefone.Where(char.IsDigit).ToArray());

        // Limita a 11 dígitos (considerando código de área + número)
        if (digits.Length > 11)
            digits = digits.Substring(0, 11);

        // Formata como (XX) XXXXX-XXXX ou (XX) XXXX-XXXX
        if (digits.Length == 11)
            return $"({digits.Substring(0, 2)}) {digits.Substring(2, 5)}-{digits.Substring(7)}";
        if (digits.Length == 10)
            return $"({digits.Substring(0, 2)}) {digits.Substring(2, 4)}-{digits.Substring(6)}";

        return digits;
    }

    private void LoadExistingProfileData()
    {
        if (_existingProfile == null)
            return;

        NomeCompleto = _existingProfile.NomeCompleto;
        CPF = _existingProfile.CPF;
        RG = _existingProfile.RG ?? string.Empty;
        Telefone = _existingProfile.Telefone ?? string.Empty;
        SelectedGenero = GeneroOptions.FirstOrDefault(g => g.Value == (int)_existingProfile.Genero);
        DataNascimento = _existingProfile.DataNascimento;
        CRN = _existingProfile.CRN ?? string.Empty;
        CEP = _existingProfile.CEP;
        SelectedEstado = EstadoOptions.FirstOrDefault(e => e.Value == (int)_existingProfile.Estado);
        Endereco = _existingProfile.Endereco;
        Numero = _existingProfile.Numero;
        Cidade = _existingProfile.Cidade;
        Complemento = _existingProfile.Complemento ?? string.Empty;
        Bairro = _existingProfile.Bairro;

        // Debug output dos dados carregados
        System.Diagnostics.Debug.WriteLine("==> Dados do perfil carregados:");
        System.Diagnostics.Debug.WriteLine($"    NomeCompleto: '{NomeCompleto}'");
        System.Diagnostics.Debug.WriteLine($"    CPF: '{CPF}'");
        System.Diagnostics.Debug.WriteLine($"    RG: '{RG}'");
        System.Diagnostics.Debug.WriteLine($"    Telefone: '{Telefone}'");
        System.Diagnostics.Debug.WriteLine($"    Genero: '{SelectedGenero?.Display}'");
        System.Diagnostics.Debug.WriteLine($"    DataNascimento: '{DataNascimento}'");
        System.Diagnostics.Debug.WriteLine($"    CRN: '{CRN}'");
        System.Diagnostics.Debug.WriteLine($"    CEP: '{CEP}'");
        System.Diagnostics.Debug.WriteLine($"    Estado: '{SelectedEstado?.Display}'");
        System.Diagnostics.Debug.WriteLine($"    Endereco: '{Endereco}'");
        System.Diagnostics.Debug.WriteLine($"    Numero: '{Numero}'");
        System.Diagnostics.Debug.WriteLine($"    Cidade: '{Cidade}'");
        System.Diagnostics.Debug.WriteLine($"    Complemento: '{Complemento}'");
        System.Diagnostics.Debug.WriteLine($"    Bairro: '{Bairro}'");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
