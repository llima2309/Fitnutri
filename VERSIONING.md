# Sistema de Versionamento Fitnutri

Este documento explica como funciona o sistema de versionamento automático implementado no projeto Fitnutri.

## Como Funciona

O sistema de versionamento está baseado na propriedade `<Version>` do arquivo `Fitnutri.csproj` e funciona da seguinte forma:

### 1. Configuração da Versão

No arquivo `Fitnutri/Fitnutri.csproj`, as seguintes propriedades controlam a versão:

```xml
<PropertyGroup>
  <!-- Versioning -->
  <Version>1.0.0</Version>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
  <InformationalVersion>1.0.0</InformationalVersion>
</PropertyGroup>
```

### 2. Processo Automático de Deploy

Quando você faz push para a branch `master`, o GitHub Actions:

1. **Extrai a versão** do arquivo `.csproj`
2. **Cria tags Docker** com múltiplas versões:
   - `fitnutri-api:1.0.0-abc1234` (versão + commit hash - imutável)
   - `fitnutri-api:1.0.0` (versão atual)
   - `fitnutri-api:prod` (alias para produção)
   - `fitnutri-api:latest` (última versão)
3. **Cria uma tag Git** automaticamente: `v1.0.0-20241008-1430`
4. **Faz o deploy** para o ECS

### 3. Como Atualizar a Versão

Para lançar uma nova versão:

1. **Edite o arquivo** `Fitnutri/Fitnutri.csproj`
2. **Atualize a propriedade** `<Version>`:
   ```xml
   <Version>1.1.0</Version>
   <AssemblyVersion>1.1.0.0</AssemblyVersion>
   <FileVersion>1.1.0.0</FileVersion>
   <InformationalVersion>1.1.0</InformationalVersion>
   ```
3. **Commit e push** para a branch `master`
4. **O GitHub Actions** fará o resto automaticamente

## Convenções de Versionamento

Recomendamos usar [Semantic Versioning](https://semver.org/):

- **MAJOR.MINOR.PATCH** (ex: 1.0.0)
- **MAJOR**: Mudanças que quebram compatibilidade
- **MINOR**: Novas funcionalidades compatíveis
- **PATCH**: Correções de bugs

### Exemplos:
- `1.0.0` → `1.0.1` (correção de bug)
- `1.0.1` → `1.1.0` (nova funcionalidade)
- `1.1.0` → `2.0.0` (mudança que quebra compatibilidade)

## Tags Criadas Automaticamente

### Tags Git
- Formato: `v{VERSION}-{TIMESTAMP}`
- Exemplo: `v1.0.0-20241008-1430`
- Contém informações do deploy na mensagem

### Tags Docker
- `{VERSION}-{COMMIT}`: Tag imutável para rastreabilidade
- `{VERSION}`: Tag da versão atual
- `prod`: Tag alias para produção (usado pelo ECS)
- `latest`: Sempre aponta para a última versão

## Visualizando Versões

### Ver tags Git:
```bash
git tag -l "v*"
```

### Ver informações da versão atual:
```bash
dotnet --version
```

### Ver versão do assembly:
```bash
grep -o '<Version>[^<]*</Version>' Fitnutri/Fitnutri.csproj
```

## Rollback

Para fazer rollback para uma versão anterior:

1. **Identifique a tag** da versão desejada:
   ```bash
   git tag -l "v*"
   ```

2. **Reverta o arquivo** `.csproj` para a versão anterior, ou
3. **Use o AWS Console** para reverter para uma tag Docker anterior no ECS

## Troubleshooting

### Erro: "Versão não encontrada"
- Verifique se a propriedade `<Version>` existe no arquivo `.csproj`
- Certifique-se de que não há espaços extras na tag XML

### Tag Git já existe
- O sistema usa timestamp para evitar conflitos
- Se mesmo assim houver conflito, aguarde 1 minuto e tente novamente

### Deploy falhou
- Verifique os logs do GitHub Actions
- Confirme se as credenciais AWS estão configuradas
- Verifique se o ECS Service está usando a tag `:prod`
