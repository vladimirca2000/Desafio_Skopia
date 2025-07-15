using Skopia.Domain.Entidades; 

public interface IRepositorioTarefa
{
    
    Task<IEnumerable<Tarefa>> ObterTodosPorProjetoIdAsync(Guid projetoId);

    Task<Tarefa?> ObterPorIdAsync(Guid id);

    Task<Tarefa> CriarAsync(Tarefa tarefa);

    Task<Tarefa> AtualizarAsync(Tarefa tarefa);

    Task<bool> ExcluirAsync(Guid id);

    //Task<int> ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(DateTime dataInicio);
    Task<IEnumerable<(Usuario Usuario, int Contagem)>> ObterContagemTarefasConcluidasPorUsuarioDesdeDataAsync(DateTime dataInicio);

    Task AdicionarHistoricoAsync(HistoricoAlteracaoTarefa historico);

    Task<IEnumerable<Tarefa>> ObterTarefasAtrasadasAsync();

    Task<IEnumerable<Tarefa>> ObterTarefasComVencimentoProximoAsync(TimeSpan periodo);

    Task<IEnumerable<Tarefa>> ObterTarefasPorPeriodoDeVencimentoAsync(DateTime dataInicioVencimento, DateTime dataFimVencimento);

    Task<bool> PossuiTarefasPendentesParaProjetoAsync(Guid projetoId);

    
}