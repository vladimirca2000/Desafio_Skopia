using Skopia.Domain.Repositorios.Interfaces;
using System; // Necessário para IDisposable
using System.Threading.Tasks; // Necessário para Task<int>

namespace Skopia.Domain.Interfaces.UnitOfWork;

/// <summary>
/// Define o contrato para o padrão Unit of Work (Unidade de Trabalho).
/// Este padrão é fundamental para gerenciar o estado do banco de dados e coordenar o trabalho
/// de múltiplos repositórios em uma única transação lógica e atômica.
/// A IUnitOfWork é crucial para garantir a consistência dos dados, pois todas as operações
/// de persistência (inserções, atualizações, exclusões) realizadas dentro de uma única
/// "unidade de trabalho" são confirmadas ou revertidas juntas.
/// Além disso, ela desacopla a camada de aplicação dos detalhes de implementação do banco de dados
/// e do gerenciamento direto do DbContext.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Propriedades que expõem as interfaces dos repositórios que este Unit of Work gerenciará.
    // Isso permite que a Camada de Aplicação acesse os repositórios específicos através do Unit of Work,
    // promovendo o encapsulamento e a coesão das operações de persistência relacionadas.
    // A utilização de interfaces aqui garante flexibilidade e facilita a testabilidade (mocking).

    /// <summary>
    /// Obtém o repositório para a entidade Projeto.
    /// Permite acessar operações CRUD e consultas específicas para a entidade Projeto,
    /// como a criação de novos projetos, atualização de detalhes ou busca por projetos existentes.
    /// </summary>
    IRepositorioProjeto Projetos { get; }

    /// <summary>
    /// Obtém o repositório para a entidade Tarefa.
    /// Fornece acesso a funcionalidades de persistência para a entidade Tarefa,
    /// abrangendo desde a criação e atribuição de tarefas até a modificação de seu status e prazos.
    /// </summary>
    IRepositorioTarefa Tarefas { get; }

    /// <summary>
    /// Obtém o repositório para a entidade Usuario.
    /// Habilita a manipulação de dados relacionados a usuários, incluindo autenticação,
    /// gerenciamento de perfis e atribuição de permissões.
    /// </summary>
    IRepositorioUsuario Usuarios { get; }

    /// <summary>
    /// Obtém o repositório para a entidade ComentarioTarefa.
    /// Permite gerenciar comentários associados a tarefas, facilitando a comunicação
    /// e o registro de discussões sobre o progresso e os desafios das tarefas.
    /// </summary>
    // CORREÇÃO AQUI: A propriedade deve ser do tipo IRepositorioComentarioTarefa
    IRepositorioComentarioTarefa ComentariosTarefas { get; }

    /// <summary>
    /// Obtém o repositório para a entidade HistoricoAlteracaoTarefa.
    /// Fornece acesso para registrar e consultar o histórico de alterações de tarefas,
    /// o que é vital para auditoria, rastreabilidade e compreensão da evolução de uma tarefa.
    /// </summary>
    IRepositorioHistoricoAlteracaoTarefa HistoricosAlteracaoTarefa { get; }

    /// <summary>
    /// Salva todas as mudanças feitas nos repositórios rastreados por esta unidade de trabalho
    /// no banco de dados, como uma única transação atômica.
    /// Se múltiplas operações de persistência (inserções, atualizações, exclusões) forem realizadas
    /// em diferentes repositórios dentro da mesma unidade de trabalho, todas elas serão
    /// confirmadas ou revertidas juntas. Isso garante a integridade e a consistência dos dados.
    /// O uso de `Task<int>` indica que a operação é assíncrona (não bloqueia a thread principal)
    /// e retorna o número de registros afetados, o que pode ser útil para validações ou logs.
    /// </summary>
    /// <returns>O número de objetos de estado escritos no banco de dados.
    /// Este valor é útil para verificar se as operações foram bem-sucedidas e quantos registros foram afetados.</returns>
    Task<int> CommitAsync();
}