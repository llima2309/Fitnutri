using System;
using System.Threading;
using System.Threading.Tasks;
using Fitnutri.Auth;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Fitnutri.test.Auth
{
    public class ConfirmEmailCodeTests
    {
        private static AppDbContext InMemoryDb()
        {
            var opt = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(opt);
        }

        private static IAuthService CreateSut(AppDbContext db)
        {
            var jwt = Options.Create(new JwtOptions
            {
                Issuer = "test",
                Audience = "test",
                Key = "this_is_a_very_long_test_key_at_least_32_chars__",
                ExpiresMinutes = 5
            });
            return new AuthService(db, jwt);
        }

        // Duplicamos a lógica do endpoint aqui para testar a regra sem HTTP pipeline
        private static async Task<IResult> ConfirmEmailAsync(Guid userId, int code, AppDbContext db, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
            if (user is null) return TypedResults.NotFound(new { error = "Usuário não encontrado." });
            if (user.EmailConfirmed) return TypedResults.Ok(new { message = "E-mail já confirmado." });
            if (user.EmailVerificationCode is null) return TypedResults.BadRequest(new { error = "Não há código pendente para este usuário." });
            if (user.EmailVerificationCode != code) return TypedResults.BadRequest(new { error = "Código inválido." });

            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new { message = "E-mail confirmado com sucesso." });
        }

        [Fact]
        public async Task Deve_Confirmar_Quando_Codigo_Confere()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            var user = await sut.RegisterAsync("joao123", "joao@email.com", "Strong!123", CancellationToken.None);
            user.Status = Domain.UserStatus.Approved;
            user.EmailConfirmed = false;
            user.EmailVerificationCode = 123456;
            await db.SaveChangesAsync(CancellationToken.None);

            var res = await ConfirmEmailAsync(user.Id, 123456, db, CancellationToken.None);

            var status = res.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
            status.StatusCode.Should().Be(StatusCodes.Status200OK);

            var refreshed = await db.Users.FirstAsync(x => x.Id == user.Id);

            refreshed.EmailConfirmed.Should().BeTrue();
            refreshed.EmailVerificationCode.Should().BeNull();
        }

        [Fact]
        public async Task Deve_Falhar_Quando_Codigo_Invalido()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            var user = await sut.RegisterAsync("joao123", "joao@email.com", "Strong!123", CancellationToken.None);
            user.Status = Domain.UserStatus.Approved;
            user.EmailConfirmed = false;
            user.EmailVerificationCode = 123456;
            await db.SaveChangesAsync(CancellationToken.None);

            var res = await ConfirmEmailAsync(user.Id, 111111, db, CancellationToken.None);

            var status = res.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
            status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var refreshed = await db.Users.FirstAsync(x => x.Id == user.Id);
            refreshed.EmailConfirmed.Should().BeFalse();
            refreshed.EmailVerificationCode.Should().Be(123456);
        }

        [Fact]
        public async Task Deve_Retornar_Ok_Se_Ja_Confirmado()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            var user = await sut.RegisterAsync("joao123", "joao@email.com", "Strong!123", CancellationToken.None);
            user.Status = Domain.UserStatus.Approved;
            user.EmailConfirmed = true;
            user.EmailVerificationCode = 123456;
            await db.SaveChangesAsync(CancellationToken.None);

            var res = await ConfirmEmailAsync(user.Id, 999999, db, CancellationToken.None);


            var status = res.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
            status.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task Deve_Retornar_NotFound_Se_Usuario_Nao_Existe()
        {
            using var db = InMemoryDb();

            var res = await ConfirmEmailAsync(Guid.NewGuid(), 123456, db, CancellationToken.None);

            var status = res.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
            status.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
