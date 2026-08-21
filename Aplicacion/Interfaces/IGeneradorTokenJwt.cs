using SaborByte.Dominio.Identidad;

namespace SaborByte.Aplicacion.Interfaces;

public interface IGeneradorTokenJwt
{
    string Generar(Usuario usuario, IEnumerable<string> roles, IEnumerable<Guid> sucursalesPermitidas);
}
