﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TherapyApp.Entities;

namespace TherapyApp;

public class TherapyDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public TherapyDbContext(DbContextOptions<TherapyDbContext> options) : base(options){}

    public DbSet<Diary> Diaries { get; set; }
    public DbSet<DiaryEntry> DiaryEntries { get; set; }
    public DbSet<Emotion> Emotions { get; set; }
    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<Speciality> Specialities { get; set; }
    public DbSet<Therapist> TherapistUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
              .HasOne(x => x.TherapistUser)
              .WithOne(x => x.User)
              .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Meeting>()
              .HasOne(m => m.Client)
              .WithMany()
              .HasForeignKey(m => m.ClientId)
              .OnDelete(DeleteBehavior.Restrict);  

        builder.Entity<Meeting>()
            .HasOne(m => m.Therapist)
            .WithMany()
            .HasForeignKey(m => m.TherapistId)
            .OnDelete(DeleteBehavior.Restrict);  
    }
}
