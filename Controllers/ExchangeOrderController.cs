﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondhandStore.EntityRequest;
using SecondhandStore.EntityViewModel;
using SecondhandStore.Models;
using SecondhandStore.Services;
using System.Data;
using Microsoft.AspNetCore.Mvc.Routing;
using SecondhandStore.Extension;
using System;

namespace SecondhandStore.Controllers
{

    [ApiController]
    public class ExchangeOrderController : ControllerBase
    {
        private readonly ExchangeOrderService _exchangeOrderService;
        private readonly PostService _postService;
        private readonly AccountService _accountService;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        public ExchangeOrderController(ExchangeOrderService exchangeOrderService,PostService postService,AccountService accountService, IServiceProvider serviceProvider, IMapper mapper)
        {
            _accountService = accountService;
            _postService = postService; 
            _exchangeOrderService = exchangeOrderService;
            _emailService = serviceProvider.GetRequiredService<IEmailService>();
            _mapper = mapper;
        }
        [HttpGet("get-all-exchange-for-admin")]
        [Authorize(Roles = "AD")]
        public async Task<IActionResult> GetAllExchangement() {
            var exchangeList = await _exchangeOrderService.GetAllExchange();
            if (exchangeList == null) {
                return NotFound("No exchange found");
            }
            var mappedExchange = exchangeList.Select(p => _mapper.Map<ExchangeViewEntityModel>(p));
            return Ok(mappedExchange);
        }

        // GET all action
        [HttpGet("get-all-request-list")]
        [Authorize(Roles = "US")]
        public async Task<IActionResult> GetExchangeRequest()
        {
            var userId = User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "accountId")?.Value ?? string.Empty;
            var id = Int32.Parse(userId);
            var requestList = await _exchangeOrderService.GetExchangeRequest(id);
            if (requestList is null)
            {
                return NotFound();
            }
            var mappedExchangeRequest = requestList.Select(p => _mapper.Map<ExchangeRequestEntityViewModel>(p));
            return Ok(mappedExchangeRequest);

        }
        [HttpGet("get-all-order-list")]
        [Authorize(Roles = "US")]
        public async Task<IActionResult> GetExchangeOrder()
        {
            var userId = User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "accountId")?.Value ?? string.Empty;
            var id = Int32.Parse(userId);
            var orderList = await _exchangeOrderService.GetExchangeOrder(id);
            if (orderList is null)
            {
                return NotFound();
            }
            var mappedExchangeOrder = orderList.Select(p => _mapper.Map<ExchangeOrderEntityViewModel>(p));
            return Ok(mappedExchangeOrder);

        }
        [HttpPost("send-exchange-request")]
        [Authorize(Roles = "US")]
        public async Task<IActionResult> SendExchangeRequest(ExchangeOrderCreateRequest exchangeOrderCreateRequest) 
        {
            var userId = User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "accountId")?.Value ?? string.Empty;
            int parseUserId = Int32.Parse(userId);
            var chosenPost = await _postService.GetPostById(exchangeOrderCreateRequest.PostId);
            var order = await _exchangeOrderService.GetExchangeByPostId(parseUserId, chosenPost.PostId);
            if (chosenPost is null)
            {
                return NotFound();
            }
            if (chosenPost.AccountId == parseUserId || chosenPost.PostStatusId == 2 || order.Any()) {
                return BadRequest("You cannot choose this post!");
            }
            var mappedExchange = _mapper.Map<ExchangeOrder>(exchangeOrderCreateRequest);
            mappedExchange.BuyerId = parseUserId;
            mappedExchange.SellerId = chosenPost.AccountId;
            mappedExchange.OrderDate = DateTime.Now;
            mappedExchange.OrderStatusId = 6;
            mappedExchange.PostId = chosenPost.PostId;
            var seller = await _accountService.GetAccountById(chosenPost.AccountId);
            var buyer = await _accountService.GetAccountById(parseUserId);
            try
            {
                SendMailModel request = new SendMailModel();
                request.ReceiveAddress = seller.Email;
                request.Subject = "New Order Notification";
                EmailContent content = new EmailContent();
                content.Dear = "Dear " + seller.Fullname + ",";
                content.BodyContent = "You have new order from " + buyer.Fullname + " for a product: " + chosenPost.ProductName + ".\nPlease check your exchange order.\nThank You!";
                request.Content = content.ToString(); 
                _emailService.SendMail(request);
            }
            catch (Exception ex)
            {
                return BadRequest("Cannot send email");
            }
            
            await _exchangeOrderService.AddExchangeRequest(mappedExchange);
            return Ok("Request Successfully");

        }
        [HttpPut("accept-request-for-an-order")]
        [Authorize(Roles = "US")]
        public async Task<IActionResult> AcceptExchangeRequest(int orderId) {
            var userId = User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "accountId")?.Value ?? string.Empty;
            int parseUserId = Int32.Parse(userId);
            var exchange = await _exchangeOrderService.GetExchangeById(orderId);
            if (exchange == null)
            {
                return BadRequest("Error!");
            }
            else {
                exchange.OrderStatusId = 4;
                await _exchangeOrderService.UpdateExchange(exchange);
                var relatedExchange = await _exchangeOrderService.GetAllRelatedProductPost(exchange.OrderId, exchange.PostId);
                var chosenPost = await _postService.GetPostById(exchange.PostId);
                foreach (var exchangeComponent in relatedExchange) {
                    exchangeComponent.OrderStatusId = 7;
                    await _exchangeOrderService.UpdateExchange(exchangeComponent);
                    var seller = await _accountService.GetAccountById(exchangeComponent.SellerId);
                    var buyer = await _accountService.GetAccountById(exchangeComponent.BuyerId);
                    try
                    {
                        SendMailModel request = new SendMailModel();
                        request.ReceiveAddress = buyer.Email;
                        request.Subject = "Cancel Order Notification";
                        EmailContent content = new EmailContent();
                        content.Dear = "Dear " + buyer.Fullname + ",";
                        content.BodyContent = seller.Fullname + " have cancelled a request with order Id #" + orderId + ":" + chosenPost.ProductName + ".\nReason: Another request for this product has been accepted" + "\nHave a nice day!";
                        request.Content = content.ToString();
                        _emailService.SendMail(request);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Cannot send email");
                    }
                }
                chosenPost.PostStatusId = 2;
                await _exchangeOrderService.UpdateExchange(exchange);
                return Ok("Accepted ! Please, carry out delivery.");
            }
        }
        [HttpPut("cancel-request")]
        [Authorize(Roles = "US")]
        public async Task<IActionResult> RejectExchangeRequest(int orderId)
        {
            var userId = User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "accountId")?.Value ?? string.Empty;
            int parseUserId = Int32.Parse(userId);
            var exchange = await _exchangeOrderService.GetExchangeById(orderId);
            if (exchange == null)
            {
                return BadRequest("Error!");
            }
            else
            {
                var chosenPost = await _postService.GetPostById(exchange.PostId);
                exchange.OrderStatusId = 7;
                chosenPost.PostStatusId = 1;
                await _exchangeOrderService.UpdateExchange(exchange);
                var seller = await _accountService.GetAccountById(exchange.SellerId);
                var buyer = await _accountService.GetAccountById(exchange.BuyerId);
                try
                {
                    SendMailModel request = new SendMailModel();
                    request.ReceiveAddress = seller.Email;
                    request.Subject = "Cancel Order Notification";
                    EmailContent content = new EmailContent();
                    content.Dear = "Dear " + seller.Fullname +",";
                    content.BodyContent = buyer.Fullname + " have cancelled an order with order Id #"+orderId+":" + chosenPost.ProductName +"." + "\nHave a nice day!";
                    request.Content = content.ToString();
                    _emailService.SendMail(request);
                }
                catch (Exception ex)
                {
                    return BadRequest("Cannot send email");
                }
                var relatedExchange = await _exchangeOrderService.GetAllRelatedProductPost(exchange.OrderId, exchange.PostId);
                foreach (var exchangeComponent in relatedExchange)
                {
                    exchangeComponent.OrderStatusId = 6;
                    await _exchangeOrderService.UpdateExchange(exchangeComponent);
                    var reorderbuyer = await _accountService.GetAccountById(exchangeComponent.BuyerId);
                    try
                    {
                        SendMailModel request = new SendMailModel();
                        request.ReceiveAddress = reorderbuyer.Email;
                        request.Subject = "Re-Order Notification";
                        EmailContent content = new EmailContent();
                        content.Dear = "Dear " + reorderbuyer.Fullname + ",";
                        content.BodyContent = "You have an opportunity for re-order exchange with order Id #" + orderId + ":" + chosenPost.ProductName + ".\nHave a nice day!";
                        request.Content = content.ToString();
                        _emailService.SendMail(request);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Cannot send email");
                    }
                }

                return Ok("Cancelled Successfully!");
            }
        }

    }
}
