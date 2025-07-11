using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Excecoes;

public class ExcecaoProjetoComTarefasPendentes: ExcecaoDominio
{
    public ExcecaoProjetoComTarefasPendentes(string mensagem) : base(mensagem)
    {
    }
}
