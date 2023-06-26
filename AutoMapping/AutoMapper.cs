using AutoMapper;
using SecondhandStore.EntityRequest;
using SecondhandStore.EntityViewModel;
using SecondhandStore.Models;
using SecondhandStore.Services;
using SecondhandStore.Repository;
using SecondhandStore.Infrastructure;
namespace SecondhandStore.AutoMapping;

public class AutoMapper : Profile
{
    public AutoMapper()
    {
        MapRole();
        MapAccount();
        MapTopUp();
        MapPost();
    }

    private void MapRole()
    {
        CreateMap<Role, RoleCreateRequest>()
            .ReverseMap();

        CreateMap<Role, RoleUpdateRequest>()
            .ReverseMap();

        CreateMap<RoleCreateRequest, Role>()
            .ReverseMap();

        CreateMap<RoleUpdateRequest, Role>()
            .ReverseMap();

        CreateMap<RoleEntityViewModel, Role>()
            .ReverseMap();

        CreateMap<Role, RoleEntityViewModel>()
            .ReverseMap();
    }

    private void MapAccount()
    {
        CreateMap<Account, AccountCreateRequest>()
            .ReverseMap();
        CreateMap<Account, AccountUpdateRequest>()
            .ReverseMap();
        CreateMap<AccountCreateRequest, Account>()
            .ReverseMap();
        CreateMap<AccountUpdateRequest, Account>()
            .ReverseMap();
    }
    private void MapDeactivateAccount()
    {
        CreateMap<Account, AccountDeactivateRequest>()
            .ReverseMap();
        CreateMap<AccountDeactivateRequest, Account>()
            .ReverseMap();
    }
    private void MapTopUp()
    {
        CreateMap<TopUp, TopUpCreateRequest>()
            .ReverseMap();
        CreateMap<TopUpCreateRequest, TopUp>()
           .ReverseMap();
    }
    private void MapPost()
    {
        CreateMap<Post, PostEntityViewModel>()
        .ForMember(d => d.Fullname, map => map.MapFrom(p => p.Account.Fullname));
        CreateMap<PostEntityViewModel, Post>()
        .ForMember(d => d.Account, map => map.Ignore())
        .ForMember(d => d.Category, map => map.Ignore())
        .ForMember(d => d.ExchangeOrders, map => map.Ignore())
        .ForMember(d => d.ExchangeRequests, map => map.Ignore())
        .ForMember(d => d.Reviews, map => map.Ignore());
        CreateMap<PostCreateRequest, Post>()
        .ReverseMap();

    }
}