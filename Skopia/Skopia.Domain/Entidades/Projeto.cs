using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Entidades;

/// <summary>
/// Representa um projeto no sistema, atuando como um Aggregate Root para suas tarefas.
/// Encapsula as informações e o comportamento de um projeto, garantindo a consistência das regras de negócio.
/// Um Aggregate Root é a única entidade dentro de um agregado que pode ser referenciada diretamente por outras entidades
/// ou objetos de fora do agregado, servindo como um ponto de entrada para todas as operações transacionais.
/// </summary>
public class Projeto : EntidadeBase // Herda de EntidadeBase, recebendo Id, EstaDeletado e QuandoDeletou (para soft delete)
{
    // Propriedades primárias do projeto.
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public DateTime DataCriacao { get; private set; } // Esta data é definida pelo domínio no construtor.
    public Guid UsuarioId { get; private set; }

    // Coleção interna de tarefas.
    // É importante notar que esta coleção pode ser populada pelo Entity Framework Core (lazy/eager loading).
    // As operações sobre ela (AdicionarTarefa, RemoverTarefa) são consistentes dentro do limite do Aggregate.
    private readonly List<Tarefa> _tarefas;

    // Exposição da coleção de tarefas como IReadOnlyCollection<T> para garantir encapsulamento e imutabilidade externa.
    public IReadOnlyCollection<Tarefa> Tarefas => _tarefas.AsReadOnly();

    private const int LIMITE_MAXIMO_TAREFAS = 20; // Exemplo de regra de negócio definida no domínio.

    /// <summary>
    /// Construtor protegido (protected) para uso exclusivo do ORM (Entity Framework Core).
    /// Ele é invocado pelo EF Core para materializar entidades do banco de dados.
    /// Este construtor chama o construtor padrão da EntidadeBase, que também é protected e gera um novo GUID para o Id.
    /// A chamada explícita `base()` aumenta a clareza sobre qual construtor da classe base está sendo utilizado.
    /// </summary>
    protected Projeto() : base()
    {
        _tarefas = new List<Tarefa>();
    }

    /// <summary>
    /// Construtor principal para criar uma nova instância de Projeto.
    /// Garante que o projeto seja criado em um estado válido, aplicando validações de domínio.
    /// A chamada explícita `base()` garante que o Id da entidade base seja gerado no momento da criação do Projeto.
    /// </summary>
    /// <param name="nome">O nome do projeto, um campo obrigatório.</param>
    /// <param name="descricao">Uma descrição opcional do projeto.</param>
    /// <param name="usuarioId">O ID do usuário que está criando o projeto.</param>
    public Projeto(string nome, string? descricao, Guid usuarioId) : base()
    {
        _tarefas = new List<Tarefa>();

        // Validações das invariantes de domínio.
        // ExcecaoDominio é uma classe customizada para lançar exceções de domínio, garantindo que a entidade esteja sempre em um estado válido.
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(nome), "O nome do projeto não pode ser vazio.");
        ExcecaoDominio.Quando(usuarioId == Guid.Empty, "O ID do usuário não pode ser vazio.");

        // Atribuição das propriedades.
        Nome = nome;
        Descricao = descricao;
        DataCriacao = DateTime.UtcNow; // A data de criação é definida no momento da instanciação da entidade no domínio.
                                       // Isso garante que a entidade é a fonte da verdade para sua própria data de criação.
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Atualiza o nome do projeto.
    /// Aplica validação de domínio para garantir que o nome não seja vazio.
    /// Este método é um "setter" controlado, garantindo que as regras de negócio sejam aplicadas antes de alterar o estado.
    /// </summary>
    /// <param name="novoNome">O novo nome a ser atribuído ao projeto.</param>
    public void AtualizarNome(string novoNome)
    {
        ExcecaoDominio.Quando(string.IsNullOrWhiteSpace(novoNome), "O nome do projeto não pode ser vazio.");
        if (Nome != novoNome) // Evita atribuições desnecessárias se o valor não mudou.
        {
            Nome = novoNome;
        }
    }

    /// <summary>
    /// Atualiza a descrição do projeto.
    /// Permite que a descrição seja nula.
    /// </summary>
    /// <param name="novaDescricao">A nova descrição a ser atribuída ao projeto.</param>
    public void AtualizarDescricao(string? novaDescricao)
    {
        if (Descricao != novaDescricao) // Evita atribuições desnecessárias.
        {
            Descricao = novaDescricao;
        }
    }

    /// <summary>
    /// Adiciona uma nova tarefa ao projeto.
    /// Valida se a tarefa é nula, se pertence ao projeto correto e se o limite de tarefas foi atingido.
    /// Esta é uma regra de consistência do Aggregate Root: uma tarefa só pode ser adicionada se pertencer a este projeto.
    /// </summary>
    /// <param name="novaTarefa">A instância da tarefa a ser adicionada.</param>
    public void AdicionarTarefa(Tarefa novaTarefa)
    {
        ExcecaoDominio.Quando(novaTarefa == null, "A tarefa não pode ser nula.");
        // CORREÇÃO AQUI: Garante que a tarefa está associada a este projeto
        ExcecaoDominio.Quando(novaTarefa?.ProjetoId != this.Id, "A tarefa pertence a outro projeto e não pode ser adicionada aqui.");

        ExcecaoDominio.Quando(_tarefas.Count >= LIMITE_MAXIMO_TAREFAS,
            $"Limite máximo de {LIMITE_MAXIMO_TAREFAS} tarefas por projeto atingido. Não é possível adicionar mais tarefas.");
        if (novaTarefa is not null)
            _tarefas.Add(novaTarefa);
    }

    /// <summary>
    /// Remove uma tarefa do projeto pelo seu ID.
    /// Nota: Esta remoção é da coleção em memória. A persistência (física ou lógica) será gerenciada pelo repositório
    /// quando o Aggregate Root (Projeto) for salvo.
    /// </summary>
    /// <param name="tarefaId">O ID da tarefa a ser removida.</param>
    public void RemoverTarefa(Guid tarefaId)
    {
        var tarefaParaRemover = _tarefas.FirstOrDefault(t => t.Id == tarefaId);
        ExcecaoDominio.Quando(tarefaParaRemover == null, "Tarefa não encontrada neste projeto.");

        if (tarefaParaRemover is not null)
            _tarefas.Remove(tarefaParaRemover);
    }

    /// <summary>
    /// Retorna uma tarefa específica pelo seu ID.
    /// </summary>
    /// <param name="tarefaId">O ID da tarefa a ser obtida.</param>
    /// <returns>A instância da Tarefa, se encontrada; caso contrário, null.</returns>
    public Tarefa? ObterTarefaPorId(Guid tarefaId)
    {
        return _tarefas.FirstOrDefault(t => t.Id == tarefaId);
    }

    // REMOVIDOS: PossuiTarefasPendentes() e PodeSerRemovido()
    // A lógica de "se um projeto pode ser removido" agora reside no ProjetoService.
    // A lógica de "possui tarefas pendentes" para um projeto será uma consulta direta no IRepositorioTarefa,
    // que é usada pelo ProjetoService.
}