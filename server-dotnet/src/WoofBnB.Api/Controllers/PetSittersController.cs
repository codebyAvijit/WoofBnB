using Microsoft.AspNetCore.Mvc;
using WoofBnB.Application.PetSitters;
using WoofBnB.Application.PetSitters.DTOs;

namespace WoofBnB.Api.Controllers;

[ApiController]
[Route("api/petsitters")]
public class PetSittersController : ControllerBase
{
    private readonly IPetSitterService _service;

    public PetSittersController(IPetSitterService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<PetSitterDto>>> GetAll()
    {
        var petSitters = await _service.GetAllAsync();

        return Ok(petSitters);
    }

    [HttpPost]
    public async Task<ActionResult<PetSitterDto>> Create(
        [FromBody] CreatePetSitterRequest request)
    {
        var petSitter =
            await _service.RegisterAsync(request);

        return CreatedAtAction(
            nameof(GetAll),
            null,
            petSitter);
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<List<PetSitterDto>>> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius)
    {
        var petSitters =
            await _service.GetNearbyAsync(
                lat,
                lng,
                radius);

        return Ok(petSitters);
    }
}