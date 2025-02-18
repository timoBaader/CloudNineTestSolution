using Microsoft.AspNetCore.Mvc;

namespace TechTestBackend.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    private readonly SongstorageContext _context;

    public SpotifyController(SongstorageContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("searchTracks")]
    public async Task<IActionResult> SearchTracks(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Song name cannot be empty.");
        }

        try
        {
            Spotifysong[] result = await SpotifyHelper.GetTracksAsync(name);

            if (!result.Any())
            {
                return NotFound("No songs were found");
            }

            return Ok(result);
        }
        // TODO: Catch 429 error codes as it indicates spotify's rate limit has been exceeded
        // use the "retryAfter" header and implement a retry method
        catch (Exception e)
        {
            return StatusCode(
                500,
                new { error = "An unexpected error occurred.", details = e.Message }
            );
        }
    }

    [HttpPost]
    [Route("like")]
    public async Task<IActionResult> Like(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Song name cannot be empty.");
        }

        var track = await SpotifyHelper.GetTrackAsync(id);
        if (track.Id == null)
        {
            return NotFound("Song not found");
        }

        var song = new Spotifysong();
        song.Id = id;
        song.Name = track.Name;

        if (SongExists(song.Id))
            return Conflict("Song already liked");

        try
        {
            _context.LikedSongs.Add(song);

            _context.SaveChanges();
        }
        catch (Exception e)
        {
            return StatusCode(
                500,
                new { error = "An unexpected error occured", details = e.Message }
            );
        }

        return Ok();
    }

    [HttpPost]
    [Route("removeLike")]
    public async Task<IActionResult> RemoveLike(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Song name cannot be empty.");
        }

        var track = await SpotifyHelper.GetTrackAsync(id);
        if (track.Id == null)
        {
            return StatusCode(400);
        }

        var song = new Spotifysong();
        song.Id = id;

        if (!SongExists(song.Id))
            return Conflict("Song not among liked songs");

        try
        {
            _context.LikedSongs.Remove(song);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            return StatusCode(
                500,
                new { error = "An unexpected error ocurred", details = e.Message }
            );
        }

        return Ok();
    }

    [HttpGet]
    [Route("listLiked")]
    public async Task<IActionResult> ListLiked()
    {
        var likedSongs = _context.LikedSongs.ToList();
        var validLikedSongs = new List<Spotifysong>();

        try
        {
            foreach (var song in likedSongs)
            {
                var track = await SpotifyHelper.GetTrackAsync(song.Id);
                if (track != null)
                {
                    validLikedSongs.Add(song);
                }
            }
        }
        catch (Exception e)
        {
            return StatusCode(
                500,
                new { error = "An unexpected error occurred", details = e.Message }
            );
        }
        return Ok(validLikedSongs);
    }

    private bool SongExists(string id)
    {
        return _context.LikedSongs.Any(s => s.Id == id);
    }
}
