using Microsoft.AspNetCore.Mvc;
using WoofBnB.Application.Common;
using WoofBnB.Application.Common.Responses;
using WoofBnB.Application.PetSitters;
using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Api.Controllers;

/// <summary>
/// Mirrors server/src/modules/petsitter/petsitter.routes.js: POST /, GET /, GET /nearby —
/// all public, matching Node exactly (no `authenticate` middleware on any petsitter
/// route). GET /petsitters/{id} is deliberately NOT implemented: the Node repository
/// has an unused findPetSitterById, but no route ever calls it (decision D-9).
/// </summary>
[ApiController]
[Route("api/petsitters")]
public sealed class PetSittersController : ControllerBase
{
    private readonly IPetSitterService _service;
    private readonly IApiResponseFactory _responseFactory;

    public PetSittersController(IPetSitterService service, IApiResponseFactory responseFactory)
    {
        _service = service;
        _responseFactory = responseFactory;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PetSitterDto>>> Register([FromBody] CreatePetSitterRequest request)
    {
        var result = await _service.RegisterAsync(request);

        return StatusCode(
            HttpStatusCodes.Created,
            _responseFactory.Success(HttpStatusCodes.Created, "Pet sitter registered successfully", result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PetSitterDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(_responseFactory.Success(HttpStatusCodes.Ok, "Pet sitters fetched successfully", result));
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<List<PetSitterDto>>>> GetNearby([FromQuery] NearbyPetSitterQuery query)
    {
        var result = await _service.GetNearbyAsync(query);

        return Ok(_responseFactory.Success(HttpStatusCodes.Ok, "Nearby pet sitters fetched successfully", result));
    }
}
