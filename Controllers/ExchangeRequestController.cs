using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecondhandStore.EntityRequest;
using SecondhandStore.EntityViewModel;
using SecondhandStore.Models;
using SecondhandStore.Services;

namespace SecondhandStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRequestController : ControllerBase
    {
        private readonly ExchangeRequestService _exchangeRequestService;
        private readonly IMapper _mapper;
        private readonly ExchangeOrderService _exchangeOrderService;
        private readonly AccountService _accountService;
        public ExchangeRequestController(ExchangeRequestService exchangeRequestService,ExchangeOrderService exchangeOrderService,AccountService accountService ,IMapper mapper)
        {
            _exchangeRequestService = exchangeRequestService;
            _exchangeOrderService = exchangeOrderService;
            _accountService = accountService;
            _mapper = mapper;
        }

        // GET all action
        [HttpGet]
        public async Task<IActionResult> GetRequestList()
        {
            var requestList = await _exchangeRequestService.GetAllRequest();

            if (requestList.Count == 0 || !requestList.Any())
                return NotFound();

            return Ok(requestList);
        }

        // GET by Id action
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTopUpById(int id)
        {
            var existingRequest = await _exchangeRequestService.GetRequestById(id);
            if (existingRequest is null)
                return NotFound();
            return Ok(existingRequest);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(string id)
        {
            var account = _accountService.GetAccountById(id);
            ExchangeRequestCreateRequest exchangeRequestCreateRequest = new ExchangeRequestCreateRequest();
            var mappedRequest = _mapper.Map<ExchangeRequest>(exchangeRequestCreateRequest);

            await _exchangeRequestService.AddRequest(mappedRequest);

            ExchangeOrderEntityViewModel exchangeOrder = new ExchangeOrderEntityViewModel {
                AccountId = exchangeRequestCreateRequest.SellerId,
                PostId = exchangeRequestCreateRequest.PostId,
                OrderDate = DateTime.Now,
                OrderStatus = false,
                ReceiverEmail = account.Id,

            };

            var mappedOrder = _mapper.Map<ExchangeOrder>(exchangeOrderCreateRequest);

            await _exchangeOrderService.AddOrder(mappedOrder);

            return CreatedAtAction(nameof(GetRequestList),
                new { id = mappedRequest.RequestDetailId },
                mappedRequest);

           

            
        }
    }
}
