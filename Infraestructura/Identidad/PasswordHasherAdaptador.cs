using Microsoft.AspNetCore.Identity;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Identidad;

namespace SaborByte.Infraestructura.Identidad;

public class PasswordHasherAdaptador : IPasswordHasher
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(default!, password);

    public bool Verificar(string hashAlmacenado, string passwordIngresada) =>
        _hasher.VerifyHashedPassword(default!, hashAlmacenado, passwordIngresada) != PasswordVerificationResult.Failed;
}
