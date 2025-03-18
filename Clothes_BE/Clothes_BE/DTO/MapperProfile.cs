using AutoMapper;
using Clothes_BE.Models;

namespace Clothes_BE.DTO
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<Products, ProductResultDTO>()
                .ForMember(c => c.id, dto => dto.MapFrom(src => src.id))
                .ForMember(c => c.title, dto => dto.MapFrom(src => src.title))
                .ForMember(c => c.price, dto => dto.MapFrom(src => src.price))
                .ForMember(c => c.old_price, dto => dto.MapFrom(src => src.old_price))
                .ForMember(c => c.description, dto => dto.MapFrom(src => src.description))
                .ForMember(c => c.options, dto => dto.MapFrom(src => src.product_options.Select((x,index) => new ValueMapDTO {id = index, value = x.option_id}).ToList()));
        }

    }
}
