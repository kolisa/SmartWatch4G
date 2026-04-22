using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Creates a new user linked to a device.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.CreateAsync(request);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                409 => Conflict(new { message = result.Error }),
                _   => BadRequest(new { message = result.Error })
            };

        return CreatedAtAction(nameof(GetUser),
            new { deviceId = result.Value!.DeviceId }, result.Value);
    }

    /// <summary>Returns a single user by device ID.</summary>
    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetUser(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _userService.GetByDeviceIdAsync(deviceId);
        if (result.IsFailure)
            return result.ErrorCode == 404
                ? NotFound(new { message = result.Error })
                : StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Returns all active users.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllAsync();
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Updates an existing user's details.</summary>
    [HttpPut("{deviceId}")]
    public async Task<IActionResult> UpdateUser(string deviceId, [FromBody] UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.UpdateAsync(deviceId, request);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return Ok(result.Value);
    }

    /// <summary>Deactivates a user (soft delete).</summary>
    [HttpDelete("{deviceId}")]
    public async Task<IActionResult> DeleteUser(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _userService.DeleteAsync(deviceId);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return NoContent();
    }

    /// <summary>Links or unlinks a user to a company. Set companyId to null to remove the association.</summary>
    [HttpPut("{deviceId}/company")]
    public async Task<IActionResult> LinkToCompany(string deviceId, [FromBody] LinkUserToCompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _userService.LinkToCompanyAsync(deviceId, request.CompanyId);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return Ok(result.Value);
    }
}
