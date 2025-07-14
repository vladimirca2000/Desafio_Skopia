using Skopia.Domain.Enums;
using Skopia.Domain.Excecoes;
using System.Text.RegularExpressions; 

namespace Skopia.Domain.Entidades;

public class Usuario : EntidadeBase 
{
   
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public FuncaoUsuario Funcao { get; private set; }

    
    protected Usuario() { }

    
    public Usuario(Guid id, string nome, string email, FuncaoUsuario funcao) : base(id) 
    {
        
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(nome), "O nome do usuário não pode ser vazio.");
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(email), "O email do usuário não pode ser vazio.");

       
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        ExcecaoDominio.Quando(!emailRegex.IsMatch(email), "O formato do email é inválido.");

       
        Nome = nome;
        Email = email;
        Funcao = funcao;
    }

   
    public void AtualizarNome(string novoNome)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoNome), "O nome do usuário não pode ser vazio.");
        if (Nome != novoNome) 
        {
            Nome = novoNome;
        }
    }

    
    public void AtualizarEmail(string novoEmail)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoEmail), "O email do usuário não pode ser vazio.");
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        ExcecaoDominio.Quando(!emailRegex.IsMatch(novoEmail), "O formato do email é inválido.");

        if (Email != novoEmail) 
        {
            Email = novoEmail;
        }
    }

    
    public void AlterarFuncao(FuncaoUsuario novaFuncao)
    {
       
        if (Funcao != novaFuncao) 
        {
            Funcao = novaFuncao;
        }
    }
}