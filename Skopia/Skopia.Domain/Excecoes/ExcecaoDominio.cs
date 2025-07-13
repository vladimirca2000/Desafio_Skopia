namespace Skopia.Domain.Excecoes;

public class ExcecaoDominio: Exception
{
    public ExcecaoDominio(string mensagem) : base(mensagem)
    {
    }

    public ExcecaoDominio(string mensagem, Exception excecaoInterna) : base(mensagem, excecaoInterna)
    {
    }

    public static void Quando(bool condicao, string mensagem)
    {
        if (condicao)
        {
            throw new ExcecaoDominio(mensagem);
        }
    }
}
