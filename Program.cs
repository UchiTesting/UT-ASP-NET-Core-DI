using ASP_DI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var item = new ServiceDescriptor(
    typeof(IOperationTransient),
    typeof(Operation),
    ServiceLifetime.Transient);

builder.Services.Add(item);

builder.Services.AddTransient<IOperationTransient, Operation>();
builder.Services.AddScoped<IOperationScoped, Operation>();
builder.Services.AddSingleton<IOperationSingleton, Operation>();
builder.Services.AddSingleton<IOperationSingletonInstance>(a => new Operation(Guid.Empty));
builder.Services.AddSingleton<IOperationSingletonInstance>(a => new Operation());

builder.Services.AddTransient<IOutputLogger, OutputLogger>();
builder.Services.AddTransient<DependencyService1, DependencyService1>();
builder.Services.AddTransient<DependencyService2, DependencyService2>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
