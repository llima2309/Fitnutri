# 🔑 Como Obter a API Key

## Passo a Passo

### 1. Localize o arquivo `appsettings.json`

O arquivo está em: `/Fitnutri/appsettings.json`

### 2. Encontre a seção ApiKey

```json
{
  "ApiKey": {
    "Enabled": true,
    "Header": "x-api-key",
    "Key": "SUA_CHAVE_AQUI"
  }
}
```

### 3. Use a chave na página de teste

Cole o valor do campo `"Key"` no campo **x-api-key** da página de teste.

## ⚠️ Importante

- A API Key é **obrigatória** para todas as requisições
- Se o campo `"Key"` estiver vazio no `appsettings.json`, você precisará configurá-lo primeiro
- Nunca compartilhe sua API Key publicamente

## 🔐 Como Gerar uma Nova API Key (Opcional)

Se você quiser gerar uma nova API Key segura, pode usar:

### No Terminal (Linux/Mac):
```bash
openssl rand -base64 32
```

### No PowerShell (Windows):
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

### Online:
Você também pode usar geradores online confiáveis como:
- https://www.uuidgenerator.net/api-key-generator

Depois de gerar, atualize o `appsettings.json` com a nova chave.

## 📝 Exemplo Completo de Configuração

```json
{
  "ApiKey": {
    "Enabled": true,
    "Header": "x-api-key",
    "Key": "minha-chave-super-secreta-123456"
  }
}
```

## 🧪 Testando a API Key

Para testar se sua API Key está funcionando, você pode fazer uma requisição simples:

```bash
curl -X GET "https://localhost:7001/api/perfis" \
  -H "x-api-key: SUA_CHAVE_AQUI" \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -k
```

Se retornar erro 401 ou 403, verifique se:
1. A API Key está correta
2. O campo `"Enabled"` está `true` no appsettings.json
3. O nome do header está correto (`x-api-key`)

---

**Nota de Segurança**: Em produção, nunca commite a API Key real no controle de versão. Use variáveis de ambiente ou serviços de gerenciamento de segredos (como Azure Key Vault, AWS Secrets Manager, etc.).

