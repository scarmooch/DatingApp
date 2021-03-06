using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {

        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.PhotoUrl, opt => 
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p =>
                    p.IsMain).Url))
                .ForMember(dest => dest.Age, opt =>
                opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<User, UserForDetailedDto>()
                .ForMember(dest => dest.PhotoUrl, opt => 
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p =>
                    p.IsMain).Url))
                .ForMember(dest => dest.Age, opt =>
                opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotosForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<MessageForCreationDto, Message>();
            CreateMap<Message, MessageForReturnDto>()
                .ForMember(dest => dest.SenderPhotoUrl, opt =>
                    opt.MapFrom(src => src.Sender.Photos.FirstOrDefault( p => 
                    p.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, opt =>
                    opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p =>
                    p.IsMain).Url));
            // to use the same Dto in both directions:
            //CreateMap<MessageForCreationDto, Message>().ReverseMap();

        }
        
    }
}