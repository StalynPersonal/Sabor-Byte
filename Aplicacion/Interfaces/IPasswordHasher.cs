namespace SaborByte.Aplicacion.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verificar(string hashAlmacenado, string passwordIngresada);
}
