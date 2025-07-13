using AutoMapper;
using Skopia.Domain.Entidades; // Suas entidades de domínio
using Skopia.Domain.Enums;    // Seus enums de domínio
using Skopia.Services.Modelos; // Seus DTOs

namespace Skopia.Services.Mapeamento;

public class MapeamentoPerfil : Profile
{
    public MapeamentoPerfil()
    {
        // Mapeamentos de Projeto
        CreateMap<Projeto, ProjetoDto>()
            .ForMember(dest => dest.ContagemTarefas, // Propriedade no DTO
                opt => opt.MapFrom(src => src.Tarefas.Count)); // Conta as tarefas da entidade de domínio

        // Para criação de projeto: mapeia o DTO para a entidade.
        CreateMap<CriarProjetoDto, Projeto>()
             // AutoMapper tentará usar um construtor que corresponda aos parâmetros,
             // ou mapear diretamente as propriedades com 'private set'.
             // Se o construtor de Projeto for complexo, pode precisar de .ConstructUsing
             // ou .ForCtorParam. Para o caso de Projeto, parece simples o suficiente.
             .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
             .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
             .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.UsuarioId));

        // Para atualização de projeto: mapeia o DTO para a entidade existente.
        // Lembre-se: em serviços, você geralmente carrega a entidade existente e usa .Map(dto, entity)
        // ou chama métodos específicos da entidade.
        CreateMap<AtualizarProjetoDto, Projeto>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao));

        // Mapeamentos de Tarefa
        CreateMap<Tarefa, TarefaDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString())) // Converte enum para string
            .ForMember(dest => dest.Prioridade,
                opt => opt.MapFrom(src => src.Prioridade.ToString())); // Converte enum para string

        // Para criação de tarefa: mapeia o DTO para a entidade.
        CreateMap<CriarTarefaDto, Tarefa>()
            .ForMember(dest => dest.Status, // Define o status inicial como Pendente
                opt => opt.MapFrom(_ => StatusTarefa.Pendente))
            .ForMember(dest => dest.Prioridade, // Converte string de prioridade para enum
                opt => opt.MapFrom(src => ParsePrioridade(src.Prioridade)))
            // Propriedades do construtor de Tarefa (ProjetoId, UsuarioId, Titulo, Descricao, Prioridade, DataVencimento)
            // serão mapeadas automaticamente se os nomes/tipos de src.CriarTarefaDto corresponderem.
            // Ex: src.ProjetoId -> dest.ProjetoId
            // src.UsuarioId -> dest.UsuarioId
            // src.Titulo -> dest.Titulo
            // src.Descricao -> dest.Descricao
            // src.DataVencimento -> dest.DataVencimento
            ;

        // Para atualização de tarefa: mapeia o DTO para a entidade existente.
        // Nota importante: Para entidades de domínio com lógica de negócio em métodos (ex: AlterarStatus),
        // a melhor prática é carregar a entidade e chamar seus métodos, passando o UsuarioExecutorId.
        // Este mapeamento é mais adequado para propriedades simples.
        CreateMap<AtualizarTarefaDto, Tarefa>()
            .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
            .ForMember(dest => dest.DataConclusao, opt => opt.MapFrom(src => src.DataConclusao))
            .ForMember(dest => dest.Status, // Converte string de status para enum
                       opt => opt.MapFrom(src => ParseStatus(src.Status)));

        // Mapeamentos de Comentário
        CreateMap<ComentarioTarefa, ComentarioTarefaDto>(); // Mapeamento da entidade para o DTO
        CreateMap<CriarComentarioTarefaDto, ComentarioTarefa>(); // Mapeamento do DTO para a entidade

        // Mapeamentos de Histórico
        CreateMap<HistoricoAlteracaoTarefa, HistoricoTarefaDto>(); // Mapeamento da entidade para o DTO

        // Mapeamento de Relatório de Desempenho (se houver entidade de domínio correspondente ou se for um DTO construído na hora)
        // Geralmente, RelatorioDesempenhoDto é um DTO de projeção, não mapeado diretamente de uma entidade persistente.
        // Se você tiver uma entidade para isso, o mapeamento seria aqui.
        // Ex: CreateMap<EntidadeRelatorioDesempenho, RelatorioDesempenhoDto>();
    }

    // Método auxiliar para converter string de prioridade para enum PrioridadeTarefa
    private static PrioridadeTarefa ParsePrioridade(string prioridade)
    {
        return prioridade.ToLower() switch
        {
            "baixa" => PrioridadeTarefa.Baixa,
            "media" => PrioridadeTarefa.Media,
            "alta" => PrioridadeTarefa.Alta,
            _ => PrioridadeTarefa.Media // Valor padrão
        };
    }

    // Método auxiliar para converter string de status para enum StatusTarefa
    private static StatusTarefa ParseStatus(string status)
    {
        return status.ToLower() switch
        {
            "pendente" => StatusTarefa.Pendente,
            "emandamento" => StatusTarefa.EmAndamento,
            "concluida" => StatusTarefa.Concluida,
            "cancelada" => StatusTarefa.Cancelada,
            _ => StatusTarefa.Pendente // Valor padrão
        };
    }
}