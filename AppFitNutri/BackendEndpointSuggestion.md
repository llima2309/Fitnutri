# BACKEND ENDPOINT SUGGESTION

## Para adicionar no projeto Fitnutri (Program.cs)

### 1. Adicionar record no arquivo Contracts/AuthDtos.cs:

```csharp
public record ConfirmEmailByIdentifierRequest(string EmailOrUsername, int Code);
```

### 2. Adicionar endpoint no Program.cs (após os outros endpoints de auth):

```csharp
// Confirmação de e-mail por email/username (para uso durante login)
app.MapPost("/auth/confirm-email-by-identifier", async (ConfirmEmailByIdentifierRequest req, AppDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.EmailOrUsername))
        return Results.BadRequest(new { error = "Email ou nome de usuário é obrigatório." });

    // Buscar usuário por email ou username
    var user = await db.Users.FirstOrDefaultAsync(x => 
        x.Email == req.EmailOrUsername.Trim() || 
        x.UserName == req.EmailOrUsername.Trim(), ct);
    
    if (user is null) 
        return Results.NotFound(new { error = "Usuário não encontrado." });

    if (user.EmailConfirmed)
        return Results.Ok(new { message = "E-mail já confirmado." });

    if (user.EmailVerificationCode is null)
        return Results.BadRequest(new { error = "Não há código pendente para este usuário." });

    if (user.EmailVerificationCode != req.Code)
        return Results.BadRequest(new { error = "Código inválido." });

    user.EmailConfirmed = true;
    user.EmailVerificationCode = null; // limpa após confirmar
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { message = "E-mail confirmado com sucesso." });
}).WithTags("Auth");
```

### 3. Como testar:

1. Criar um usuário
2. Aprovar o usuário (gera código de verificação)
3. Tentar fazer login (falhará com "E-mail não verificado")
4. Usar o popup para inserir o código
5. Fazer login novamente (agora com sucesso)

### 4. Endpoint de teste manual (Postman/curl):

```http
POST /auth/confirm-email-by-identifier
Content-Type: application/json
x-api-key: <SUA_API_KEY>

{
  "emailOrUsername": "usuario@email.com",
  "code": 123456
}
```

### 5. Resposta esperada:

**Sucesso (200):**
```json
{
  "message": "E-mail confirmado com sucesso."
}
```

**Erro (400):**
```json
{
  "error": "Código inválido."
}
```