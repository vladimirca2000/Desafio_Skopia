using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Excecoes;

public class ExcecaoLimiteTarefasExcedido: ExcecaoDominio
{
    public ExcecaoLimiteTarefasExcedido(string mensagem) : base(mensagem)
    {
    }
}
