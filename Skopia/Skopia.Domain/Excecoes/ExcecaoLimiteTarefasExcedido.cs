namespace Skopia.Domain.Excecoes;

public class ExcecaoLimiteTarefasExcedido : ExcecaoDominio
{
    public ExcecaoLimiteTarefasExcedido(string mensagem) : base(mensagem)
    {
    }

    public ExcecaoLimiteTarefasExcedido(string mensagem, Exception excecaoInterna) : base(mensagem, excecaoInterna)
    {
    }
}