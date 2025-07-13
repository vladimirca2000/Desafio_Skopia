using Skopia.Domain.Enums;
using Skopia.Domain.Excecoes;
using System.Text.RegularExpressions; 

namespace Skopia.Domain.Entidades;

public class Usuario : EntidadeBase // Herda de EntidadeBase
{
    // Propriedades primárias do usuário.
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public FuncaoUsuario Funcao { get; private set; }

    /// <summary>
    /// Construtor protegido (protected) sem parâmetros.
    /// Essencial para ORMs como o Entity Framework Core. Ele permite que o ORM
    /// instancie a entidade ao carregar dados do banco de dados, sem precisar
    /// passar por todas as validações do construtor principal. As propriedades
    /// serão então populadas via reflection.
    /// </summary>
    protected Usuario() { }

    /// <summary>
    /// Construtor principal para criar uma nova instância de Usuario.
    /// Este construtor é o ponto de entrada para a criação de um novo objeto Usuario
    /// no domínio, garantindo que ele seja criado em um estado válido e consistente.
    /// </summary>
    /// <param name="id">O ID único do usuário (Guid). Passado para a classe base.</param>
    /// <param name="nome">O nome completo do usuário.</param>
    /// <param name="email">O endereço de email único do usuário.</param>
    /// <param name="funcao">A função ou papel do usuário no sistema (e.g., Regular, Gerente).</param>
    public Usuario(Guid id, string nome, string email, FuncaoUsuario funcao) : base(id) // <<-- CORREÇÃO AQUI
    {
        // Validações de invariantes de domínio no construtor (Guard Clauses).
        // Estas validações garantem que a entidade nunca seja criada em um estado inválido.
        // A validação de 'id' já é feita no construtor da EntidadeBase.
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(nome), "O nome do usuário não pode ser vazio.");
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(email), "O email do usuário não pode ser vazio.");

        // Validação de formato de email usando Regex para garantir um padrão básico.
        // Em sistemas de produção, essa validação pode ser mais robusta ou delegada a um Value Object.
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        ExcecaoDominio.Quando(!emailRegex.IsMatch(email), "O formato do email é inválido.");

        // As propriedades são atribuídas após todas as validações passarem.
        Nome = nome;
        Email = email;
        Funcao = funcao;
    }

    /// <summary>
    /// Atualiza o nome do usuário.
    /// Este método encapsula a regra de negócio para a alteração do nome.
    /// </summary>
    /// <param name="novoNome">O novo nome do usuário.</param>
    public void AtualizarNome(string novoNome)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoNome), "O nome do usuário não pode ser vazio.");
        if (Nome != novoNome) // Evita atribuições desnecessárias se o nome não mudou
        {
            Nome = novoNome;
        }
    }

    /// <summary>
    /// Atualiza o email do usuário.
    /// Este método garante que o novo email seja válido antes de ser atribuído.
    /// </summary>
    /// <param name="novoEmail">O novo email do usuário.</param>
    public void AtualizarEmail(string novoEmail)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoEmail), "O email do usuário não pode ser vazio.");
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        ExcecaoDominio.Quando(!emailRegex.IsMatch(novoEmail), "O formato do email é inválido.");

        if (Email != novoEmail) // Evita atribuições desnecessárias
        {
            Email = novoEmail;
        }
    }

    /// <summary>
    /// Altera a função do usuário.
    /// </summary>
    /// <param name="novaFuncao">A nova função do usuário.</param>
    public void AlterarFuncao(FuncaoUsuario novaFuncao)
    {
        // Não há validação complexa aqui, mas poderia haver regras de negócio
        // sobre quais funções podem ser alteradas para quais.
        if (Funcao != novaFuncao) // Evita atribuições desnecessárias
        {
            Funcao = novaFuncao;
        }
    }
}