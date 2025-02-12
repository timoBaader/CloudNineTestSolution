using Microsoft.EntityFrameworkCore;

namespace TechTestBackend;

public class SongstorageContext : DbContext
{
    public SongstorageContext(DbContextOptions<SongstorageContext> options)
        : base(options) { }

    // FIX: renaming to "LikedSongs" as "Songs" leaves too much up for interpretation
    public DbSet<Soptifysong> LikedSongs { get; set; }
}
