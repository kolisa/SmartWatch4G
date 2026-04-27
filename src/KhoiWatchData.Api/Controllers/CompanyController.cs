using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/companies")]
public sealed class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>Creates a new company.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _companyService.CreateAsync(request);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return CreatedAtAction(nameof(GetCompany), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Returns a company by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var result = await _companyService.GetByIdAsync(id);
        if (result.IsFailure)
            return result.ErrorCode == 404
                ? NotFound(new { message = result.Error })
                : StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Returns all active companies.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var result = await _companyService.GetAllAsync();
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Updates company details.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _companyService.UpdateAsync(id, request);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return Ok(result.Value);
    }

    /// <summary>Soft-deletes a company. Users linked to this company will have their company_id set to NULL.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var result = await _companyService.DeleteAsync(id);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return NoContent();
    }

    /// <summary>Returns all active users belonging to this company.</summary>
    [HttpGet("{id:int}/users")]
    public async Task<IActionResult> GetCompanyUsers(int id)
    {
        var result = await _companyService.GetUsersAsync(id);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return Ok(result.Value);
    }
}
