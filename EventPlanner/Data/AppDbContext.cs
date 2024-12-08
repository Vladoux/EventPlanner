using EventPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventAttribute> EventAttribute { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Participants)
            .WithMany(u => u.Events)
            .UsingEntity<Dictionary<string, object>>(
                "EventUser", // Промежуточная таблица
                j => j.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey("EventId")
                    .OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity<EventAttribute>()
            .HasOne(ea => ea.Event)          // Связь: EventAttribute имеет 1 Event
            .WithMany(e => e.Attributes)    // Event может иметь много EventAttribute
            .HasForeignKey(ea => ea.EventId) // Указание внешнего ключа
            .OnDelete(DeleteBehavior.Cascade); // Удаление Event удаляет связанные EventAttributes
        
    }
}