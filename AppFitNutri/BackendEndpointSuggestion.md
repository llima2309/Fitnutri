# BACKEND ENDPOINT SUGGESTION

## Para adicionar no projeto Fitnutri (Program.cs)

### 1. Adicionar record no arquivo Contracts/AuthDtos.cs:

```csharp
public record ConfirmEmailByIdentifierRequest(string EmailOrUsername, int Code);
```

### 2. Adicionar endpoint no Program.cs (ap�s os outros endpoints de auth):

```csharp
// Confirma��o de e-mail por email/username (para uso durante login)
app.MapPost("/auth/confirm-email-by-identifier", async (ConfirmEmailByIdentifierRequest req, AppDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.EmailOrUsername))
        return Results.BadRequest(new { error = "Email ou nome de usu�rio � obrigat�rio." });

    // Buscar usu�rio por email ou username
    var user = await db.Users.FirstOrDefaultAsync(x => 
        x.Email == req.EmailOrUsername.Trim() || 
        x.UserName == req.EmailOrUsername.Trim(), ct);
    
    if (user is null) 
        return Results.NotFound(new { error = "Usu�rio n�o encontrado." });

    if (user.EmailConfirmed)
        return Results.Ok(new { message = "E-mail j� confirmado." });

    if (user.EmailVerificationCode is null)
        return Results.BadRequest(new { error = "N�o h� c�digo pendente para este usu�rio." });

    if (user.EmailVerificationCode != req.Code)
        return Results.BadRequest(new { error = "C�digo inv�lido." });

    user.EmailConfirmed = true;
    user.EmailVerificationCode = null; // limpa ap�s confirmar
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { message = "E-mail confirmado com sucesso." });
}).WithTags("Auth");
```

### 3. Como testar:

1. Criar um usu�rio
2. Aprovar o usu�rio (gera c�digo de verifica��o)
3. Tentar fazer login (falhar� com "E-mail n�o verificado")
4. Usar o popup para inserir o c�digo
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
  "error": "C�digo inv�lido."
}
```