using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;


    public UserController(IUserService userService, 
                          ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    [HttpGet]
    public async Task<ActionResult<UserOutDto>> GetUser()
    {
        var user = await _userService.GetUserAsync(HttpContext.User.Identity!.Name);
        return Ok(user);
    }

    [HttpPatch]
    public async Task<ActionResult> PatchUser(
        JsonPatchDocument<UserForUpdateDto> patchDoc)
    {
        try
        {
            await _userService.PatchUserAsync(HttpContext.User.Identity!.Name,
                                              patchDoc, this);
            return Ok();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteUser()
    {
        await _userService.DeleteUserAsync(HttpContext.User.Identity!.Name);
        return NoContent();
    }
}