using EventPlanner.Models;
using EventPlanner.Models.FrontModels;

namespace EventPlanner.Mappings;
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventDto>()
            .ReverseMap(); // Добавляем маппинг в обратном направлении

        // Маппинг EventAttribute <-> EventAttributeDto
        CreateMap<EventAttribute, EventAttributeDto>()
            .ReverseMap();

        // Маппинг для создания (CreateEventDto -> Event)
        CreateMap<CreateEventDto, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Игнорируем Id при создании
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.Attributes));

        CreateMap<CreateEventAttributeDto, EventAttribute>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();

    }
}
