using CicdPractice.Api.Exceptions;
using CicdPractice.Api.Managers;
using CicdPractice.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CicdPractice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerManager _playerManager;

    public PlayersController(IPlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlayers()
    {
        var players = await _playerManager.GetAllPlayersAsync();
        return Ok(players);
    }

    [HttpPost]
    public async Task<IActionResult> AddPlayer([FromBody] CreatePlayerRequest request)
    {
        try
        {
            var player = await _playerManager.AddPlayerAsync(request);
            return CreatedAtAction(nameof(GetAllPlayers), player);
        }
        catch (DuplicateJerseyNumberException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemovePlayer(int id)
    {
        try
        {
            await _playerManager.RemovePlayerAsync(id);
            return NoContent();
        }
        catch (PlayerNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("lineup")]
    public async Task<IActionResult> EditLineup([FromBody] EditLineupRequest request)
    {
        try
        {
            var startingLineup = await _playerManager.EditLineupAsync(request);
            return Ok(startingLineup);
        }
        catch (InvalidLineupException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PlayerNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
