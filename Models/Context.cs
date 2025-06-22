using Microsoft.EntityFrameworkCore;

namespace kaban.Models;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<CallbackEntity> Callbacks { set; get; }
    public DbSet<PlaceEntity> Places { set; get; }
    public DbSet<QuestionEntity> Questions { set; get; }
    public DbSet<EventPhotoEntity> EventPhotos { set; get; }
    public DbSet<HTMLBodyEntity> HTMLBodies { set; get; }
}