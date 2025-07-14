using AutoMapper;
using Skopia.Domain.Entidades; // Suas entidades de domínio
using Skopia.Domain.Enums;    // Seus enums de domínio
using Skopia.Services.Modelos; // Seus DTOs

namespace Skopia.Services.Mapeamento;

public class MapeamentoPerfil : Profile
{
    public MapeamentoPerfil()
    {
        
        CreateMap<Projeto, ProjetoDto>()
            .ForMember(dest => dest.ContagemTarefas, 
                opt => opt.MapFrom(src => src.Tarefas.Count)); 

        
        CreateMap<CriarProjetoDto, Projeto>()             
             .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
             .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
             .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.UsuarioId));

        
        CreateMap<AtualizarProjetoDto, Projeto>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao));

        
        CreateMap<Tarefa, TarefaDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString())) 
            .ForMember(dest => dest.Prioridade,
                opt => opt.MapFrom(src => src.Prioridade.ToString())); 

        
        CreateMap<CriarTarefaDto, Tarefa>()
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(_ => StatusTarefa.Pendente))
            .ForMember(dest => dest.Prioridade, 
                opt => opt.MapFrom(src => ParsePrioridade(src.Prioridade)));

        
        CreateMap<AtualizarTarefaDto, Tarefa>()
            .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Titulo))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
            .ForMember(dest => dest.DataConclusao, opt => opt.MapFrom(src => src.DataConclusao))
            .ForMember(dest => dest.Status, 
                       opt => opt.MapFrom(src => ParseStatus(src.Status)));

        
        CreateMap<ComentarioTarefa, ComentarioTarefaDto>(); 
        CreateMap<CriarComentarioTarefaDto, ComentarioTarefa>(); 

       
        CreateMap<HistoricoAlteracaoTarefa, HistoricoTarefaDto>(); 

    }

    
    private static PrioridadeTarefa ParsePrioridade(string prioridade)
    {
        return prioridade.ToLower() switch
        {
            "baixa" => PrioridadeTarefa.Baixa,
            "media" => PrioridadeTarefa.Media,
            "alta" => PrioridadeTarefa.Alta,
            _ => PrioridadeTarefa.Media 
        };
    }

    
    private static StatusTarefa ParseStatus(string status)
    {
        return status.ToLower() switch
        {
            "pendente" => StatusTarefa.Pendente,
            "emandamento" => StatusTarefa.EmAndamento,
            "concluida" => StatusTarefa.Concluida,
            "cancelada" => StatusTarefa.Cancelada,
            _ => StatusTarefa.Pendente 
        };
    }
}