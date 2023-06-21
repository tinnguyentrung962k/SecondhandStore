﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecondhandStore.EntityRequest;
using SecondhandStore.Models;
using SecondhandStore.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SecondhandStore.Controllers;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly IMapper _mapper;

    public AccountController(AccountService accountService, IMapper mapper)
    {
        _accountService = accountService;
        _mapper = mapper;
    }

    [SwaggerOperation(Summary =
        "Login for account with username and password. Return JWT Token if login successfully")]
    [HttpPost("login")]
    public async Task<IActionResult> LoginManagement([FromBody] LoginModelRequest loginModel)
    {
        // Tự tạo method login trong AccountService
        var loggedIn = await _accountService.Login(loginModel);
        
        if (loggedIn == null) 
            return BadRequest(new { message = "Invalid username or password." });
        
        var account = _mapper.Map<Account>(loginModel);
        var jwtToken = _accountService.CreateToken(account);
        return Ok(new
        {
            token = jwtToken
        });

    }

    // GET all action
    [HttpGet]
    [Route("api/[controller]/get-account-list")]
    public async Task<IActionResult> GetAccountList()
    {
        var accountList = await _accountService.GetAllAccounts();

        if (!accountList.Any())
            return NotFound();

        return Ok(accountList);
    }

    // GET by Id action
    [HttpGet()]
    [Route("api/[controller]/get-account-by-id")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        var existingAccount = await _accountService.GetAccountById(id);
        if (existingAccount is null)
            return NotFound();
        return Ok(existingAccount);
    }

    [HttpGet]
    [Route("api/[controller]/get-user-by-name")]
    public async Task<IActionResult> GetUserByName(string fullName)
    {
        var existingUser = await _accountService.GetUserByName(fullName);
        if (existingUser is null)
            return NotFound();
        return Ok(existingUser);
    }

    [HttpPost]
    [Route("api/[controller]/create-new-account")]
    public async Task<IActionResult> CreateNewAccount(AccountCreateRequest accountCreateRequest)
    {
        var mappedAccount = _mapper.Map<Account>(accountCreateRequest);

        await _accountService.AddAccount(mappedAccount);

        return CreatedAtAction(nameof(GetAccountList),
            new { id = mappedAccount.AccountId },
            mappedAccount);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(string id, AccountUpdateRequest accountUpdateRequest)
    {
        /*
        var existingAccount = await _accountService.GetAccountById(id);

        if (existingAccount.ToString() is null)
            return NotFound();
        
        existingAccount.Password = accountUpdateRequest.Password;
        existingAccount.Fullname = accountUpdateRequest.Fullname;
        existingAccount.Email = accountUpdateRequest.Email;
        existingAccount.Address = accountUpdateRequest.Address;
        existingAccount.PhoneNo = accountUpdateRequest.PhoneNo;
        existingAccount.IsActive = accountUpdateRequest.IsActive;
        existingAccount.RoleId = existingAccount.RoleId;
        
        
        if (existingAccount.ToString() is null)
            return NotFound();
        
        await _accountService.UpdateAccount(existingAccount);

        return NoContent();
        */
        try
        {
            var existingAccount = await _accountService.GetAccountById(id);

            if (existingAccount is null)
                return NotFound();

            var mappedAccount = _mapper.Map<Account>(accountUpdateRequest);

            await _accountService.UpdateAccount(mappedAccount);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Invalid Request");
        }
    }


    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleAccountStatus(string id)
    {
        try
        {
            var existingAccount = await _accountService.GetAccountById(id);

            if (existingAccount is null)
                return NotFound();

            existingAccount.IsActive = !existingAccount.IsActive;

            await _accountService.UpdateAccount(existingAccount);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Invalid Request");
        }
    }


    // POST action 
    //[HttpPost]
    //public IActionResult CreateNewAccount(Account account)
    //{
    //    AccountService.Create(account);
    //    return CreatedAtAction(nameof(Get), new { id = account.AccountId }, account);
    //}

    //// PUT action
    //[HttpPut("{id}")]
    //public IActionResult Update(string id, Account account)
    //{
    //    // This code will update the account and return a result
    //    if (id != account.AccountId)
    //        return BadRequest();

    //    var existingAccount = accountService.GetAll().Where(p => p.AccountId.Equals(id)).FirstOrDefault();
    //    if (existingAccount is null)
    //        return NotFound();

    //    accountService.Update(account);

    //    return NoContent();
    //}

    //// DELETE action
    //[HttpDelete("{id}")]
    //public IActionResult Delete(string id)
    //{
    //    // This code will delete the pizza and return a result
    //    var account = accountService.GetAll().Where(p => p.AccountId.Equals(id)).FirstOrDefault();

    //    if (account is null)
    //        return NotFound();

    //    accountService.Delete(account);

    //    return NoContent();
    //}
}