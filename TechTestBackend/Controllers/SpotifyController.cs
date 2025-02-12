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
        try
        {
            // TODO: Implement this method
            object trak = SpotifyHelper.GetTracks(name);

            return Ok(trak);
        }
        catch (Exception e)
        {
            // this is the best practice for not leaking error details
            return NotFound();
        }
    }

    [HttpPost]
    [Route("like")]
    public IActionResult Like(string id)
    {
        var track = SpotifyHelper.GetTrack(id); //check if trak exists
        if (track.Id == null || SpotifyId(id) == false)
        {
            return StatusCode(400);
        }

        var song = new Soptifysong(); //create new song
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
        var track = SpotifyHelper.GetTrack(id);
        if (track.Id == null || SpotifyId(id) == false)
        {
            return StatusCode(400); // bad request wrong id not existing in spotify
        }

        var song = new Soptifysong();
        song.Id = id;

        if (!SongExists(song.Id))
            return Conflict("Song not among liked songs");

        try
        {
            _context.LikedSongs.Remove(song); // this is not working every tume
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
        var likedSongs = _context.LikedSongs;
        var validLikedSongs = new List<Soptifysong>();

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

    private static bool SpotifyId(object id)
    {
        return id.ToString()?.Length == 22;
    }
}
