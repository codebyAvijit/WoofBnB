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

    public PetSittersController(
        IPetSitterService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PetSitterDto>>>> GetAll()
    {
        var petSitters =
            await _service.GetAllAsync();

        return Ok(
            ApiResponse<List<PetSitterDto>>.Ok(
                "Pet sitters fetched successfully",
                petSitters));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PetSitterDto>>> GetById(
        int id)
    {
        var petSitter =
            await _service.GetByIdAsync(id);

        return Ok(
            ApiResponse<PetSitterDto>.Ok(
                "Pet sitter fetched successfully",
                petSitter));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PetSitterDto>>> Create(
        [FromBody] CreatePetSitterRequest request)
    {
        var petSitter =
            await _service.RegisterAsync(request);

        return StatusCode(
            StatusCodes.Status201Created,
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