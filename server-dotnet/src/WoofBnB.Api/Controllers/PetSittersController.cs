using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WoofBnB.Application.Common.Responses;
using WoofBnB.Application.PetSitters;
using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Api.Controllers;

[ApiController]
[Route("api/petsitters")]
public class PetSittersController : ControllerBase
{
    private readonly IPetSitterService _service;
    private readonly IValidator<CreatePetSitterRequest> _validator;

    public PetSittersController(
        IPetSitterService service,
        IValidator<CreatePetSitterRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PetSitterDto>>>> GetAll()
    {
        var petSitters = await _service.GetAllAsync();

        return Ok(
            ApiResponse<List<PetSitterDto>>.Ok(
                "Pet sitters fetched successfully",
                petSitters));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PetSitterDto>>> Create(
        [FromBody] CreatePetSitterRequest request)
    {
        var validationResult =
            await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(error => error.ErrorMessage)
                .ToList();

            return BadRequest(
                ApiResponse<List<string>>.Fail(
                    "Validation failed",
                    "VALIDATION_ERROR"));
        }

        var petSitter =
            await _service.RegisterAsync(request);

        return CreatedAtAction(
            nameof(GetAll),
            null,
            ApiResponse<PetSitterDto>.Ok(
                "Pet sitter registered successfully",
                petSitter));
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<List<PetSitterDto>>>> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius)
    {
        var petSitters =
            await _service.GetNearbyAsync(
                lat,
                lng,
                radius);

        return Ok(
            ApiResponse<List<PetSitterDto>>.Ok(
                "Nearby pet sitters fetched successfully",
                petSitters));
    }
}