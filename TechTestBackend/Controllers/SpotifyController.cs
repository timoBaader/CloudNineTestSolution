using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MessagePack.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public IActionResult SearchTracks(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Song name cannot be empty.");
        }

        try
        {
            Spotifysong[] result = SpotifyHelper.GetTracks(name);

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
    public IActionResult Like(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Song name cannot be empty.");
        }

        var track = SpotifyHelper.GetTrack(id);
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
    public IActionResult RemoveLike(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Song name cannot be empty.");
        }

        var track = SpotifyHelper.GetTrack(id);
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
    public IActionResult ListLiked()
    {
        var likedSongs = _context.LikedSongs.ToList();
        var validLikedSongs = new List<Spotifysong>();

        // TODO: make API calls async
        try
        {
            validLikedSongs = likedSongs
                .Select(s => new { Song = s, Track = SpotifyHelper.GetTrack(s.Id) })
                .Where(x => x.Track != null)
                .Select(x => x.Song)
                .ToList();
        }
        catch (Exception e)
        {
            return StatusCode(
                500,
                new { error = "An unexpected error ocurred", details = e.Message }
            );
        }
        return Ok(validLikedSongs);
    }

    private bool SongExists(string id)
    {
        return _context.LikedSongs.Any(s => s.Id == id);
    }
}
