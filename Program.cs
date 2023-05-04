using AtonIttpTestTask.Domain.Interfaces;
using AtonIttpTestTask.Domain.Models;
using AtonIttpTestTask.Repository.DataBases;
using AtonIttpTestTask.Repository.Extension;
using AtonIttpTestTask.Repository.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddRepository();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors(x => x
//   .AllowAnyMethod()
//   .AllowAnyHeader()
//   .SetIsOriginAllowed(origin => true)
//   .AllowCredentials());

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetService<IUserRepository>();
    if (await userRepository.IsLoginAreAvailableAsync("Admin"))
    {
        User admin = new User
        {
            Login = "Admin",
            Password = "admin",
            Name = "Антон",
            Gender = 1,
            Admin = true,
            CreatedOn = DateTime.Now,
            CreatedBy = "Admin",
            ModifiedOn = DateTime.Now,
            ModifiedBy = "Admin"
        };
        await userRepository.CreateAsync(admin);
        await userRepository.SaveAsync();
    }
}

app.Run();
