Dependency Injection as of ASP .NET 6
=====================================

Dependency Injection in ASP .NET Core based on the [video from Rahul Nath](https://youtu.be/YR6HkvNBpX4).

## Notes

*Table of Content*  

 [Intro](#intro)  
 [Creating a Service](#creating-a-service)  
 [`ServiceDescriptor` Type](#servicedescriptor-type)  
 [Creating Types being injected our service](#creating-types-being-injected-our-service)  
 [Registering our services](#registering-our-services)  
 [Using our services](#using-our-services)  
 [Multiple registration of the same service](#multiple-registration-of-the-same-service)  
 [Register a service only if non-existent](#register-a-service-only-if-non-existent)  
 [Extra Read](#extra-read)  

> Though the explanation include .NET 5 ways of doing, code snippets are .NET 6.

### Intro

In .NET 5 the constructor for `Startup` type can be injected 3 types:

- `IConfiguration`
- `IWebHostEnvironment`
- `IHostEnvironment`

Any other type will cause an `InvalidOperationException` with the message *Unable to resole service for type `YourType` while attempting to activate `projectName.Startup`*.

`ConfigureServices()` calls  `services.AddControllers()` right from the get go which cover several services such as authorisation, CORS and more.

[Back to Top](#dependency-injection-as-of-asp-net-6)

### Creating a Service

Let's set up a service that could be injected.

It starts from the definition of a common interface `IOperation`. It simply has an `OperationId` property of type `Guid`. 
This interface is further implemented in a set of interfaces. This makes aliases for the sake of clarity.
Our service will implement those different interfaces.

> A custom service we'd like to inject.
```csharp
public interface IOperation
{
    Guid OperationId { get; }
}

public interface IOperationTransient : IOperation { }
public interface IOperationScoped : IOperation { }
public interface IOperationSingleton : IOperation { }
public interface IOperationSingletonInstance : IOperation { }

public class Operation : IOperationScoped, IOperationSingleton, IOperationSingletonInstance, IOperationTransient
{
    public Operation() : this(Guid.NewGuid()) { }

    public Operation(Guid id)
    {
        OperationId = id;
    }

    public Guid OperationId { get; private set; }
}
```

[Back to Top](#dependency-injection-as-of-asp-net-6)

### `ServiceDescriptor` Type

In .NET 5 the `Configureservices()` method takes a `IServiceCollection` which is a collection of `ServiceDescritor`.
This type has a type, implementation and lifetime.

Valid lifetimes are :

- *Transient* : Lifespen of a request. Should dependencies need to be passed further a new instance is created.
- *Scoped* : Lifespan of a request. There is one instance of the service in the request. Should dependencies need to be passed further, the same instance is shared.
- *Singleton* : Are created when needed and disposed when the app is shut down. Should dependencies need to be passed further, the same instance is shared across the appliciation.

To add out service we can create our own `ServiceDescriptor` and register it with the `IServiceCollection.Add()` method.
In such case we need to provide the relevant informations disclosed earlier: type of the service, its implementation and desired lifetime.

Though perfectly working, it is pretty verbose and there are shortcut methods to do the same :

- `AddTransient<TType, TImplementation>()`
- `AddScoped<TType, TImplementation>()`
- `AddSingleton<TType, TImplementation>()`

> In `Startup.ConfigureServices()` (.NET 5) or `Program.cs` (.NET 6)
```csharp
var  item = new ServiceDescriptor(
    typeof(IOperationTransient), 
    typeof(Operation),  // or use the factory overload: a => new Operation()
    ServiceLifetime.Transient);

builder.Services.Add(item);

builder.Services.AddTransient<IOperationTransient, Operation>();
builder.Services.AddScoped<IOperationScoped, Operation>();
builder.Services.AddSingleton<IOperationSingleton, Operation>();
builder.Services.AddSingleton<IOperationSingletonInstance>(a => new Operation(Guid.Empty));
```

[Back to Top](#dependency-injection-as-of-asp-net-6)

### Creating Types being injected our service

We create types `DependencyService1` and `DependencyService2` that are injected our services to demonstrate their lifetime.

> `DependencyService2` has the exact same implementation


```csharp 
using System.Text;

namespace ASP_DI;

public class DependencyService1
{
    private readonly IOperationTransient transient;
    private readonly IOperationScoped scoped;
    private readonly IOperationSingleton singleton;
    private readonly IOperationSingletonInstance singletonInstance;
    private readonly IOutputLogger _outputLogger;

    public DependencyService1(IOperationScoped operationScoped,
            IOperationSingleton operationSingleton,
            IOperationSingletonInstance operationSingletonInstance,
            IOperationTransient operationTransient,
            IOutputLogger outputLogger)
    {
        transient = operationTransient;
        scoped = operationScoped;
        singleton = operationSingleton;
        singletonInstance = operationSingletonInstance;
        _outputLogger = outputLogger;
    }

    public void Write()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"{Environment.NewLine}From Dependency Service 1");
        sb.AppendLine($"Transient → {transient.OperationId}");
        sb.AppendLine($"Scoped → {scoped.OperationId}");
        sb.AppendLine($"Singleton → {singleton.OperationId}");
        sb.AppendLine($"SingletonInstance → {singletonInstance.OperationId}");

        _outputLogger.Log(sb.ToString());
    }
}
```

[Back to Top](#dependency-injection-as-of-asp-net-6)

### Registering our services

We need to register any dependency as well

```csharp
builder.Services.AddTransient<IOutputLogger, OutputLogger>();
builder.Services.AddTransient<DependencyService1, DependencyService1>();
builder.Services.AddTransient<DependencyService2, DependencyService2>();
```

[Back to Top](#dependency-injection-as-of-asp-net-6)

### Using our services

The next step is the actual use in the controller. 
Services are injected in the constructor and then used where relevant.

> 
```csharp
private readonly DependencyService1 _service1;
private readonly DependencyService2 _service2;

public WeatherForecastController(ILogger<WeatherForecastController> logger,
    DependencyService1 service1, DependencyService2 service2)
{
    _logger = logger;
    _service1 = service1;
    _service2 = service2;
}

[HttpGet(Name = "GetWeatherForecast")]
public IEnumerable<object> Get()
{
    _service1.Write();
    _service2.Write();

    return Enumerable.Range(1, 5).Select(index => new
    {
        Id = index,
        forecast = new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }
    })
    .ToArray();
}
```

Bellow is a sample output in which we do 2 requests.  
We can observe: 

- Transient ids are always different.
- Scoped ids are shared within a request
- Singleton ids remain the same

> Sample Output
```
From Dependency Service 1
Transient → 2ba3c00e-a7b0-4ad9-9958-ae5d50ffa64d
Scoped → 0e25ac25-e0b9-4320-a341-23ffae14968a
Singleton → bd73c8c0-98f5-41e5-9ac9-83ca6206307f
SingletonInstance → 00000000-0000-0000-0000-000000000000

From Dependency Service 2
Transient → 231852d4-a5a3-461a-802b-b88d0cea258e
Scoped → 0e25ac25-e0b9-4320-a341-23ffae14968a
Singleton → bd73c8c0-98f5-41e5-9ac9-83ca6206307f
SingletonInstance → 00000000-0000-0000-0000-000000000000

// New request 

From Dependency Service 1
Transient → aebd848b-b00a-4e25-b0e8-5a31b4ea81f7
Scoped → ba536dd2-33c5-4031-9721-eb154a666dc2
Singleton → bd73c8c0-98f5-41e5-9ac9-83ca6206307f
SingletonInstance → 00000000-0000-0000-0000-000000000000

From Dependency Service 2
Transient → dbeb9232-a297-4020-b4dc-68a5395d3d4a
Scoped → ba536dd2-33c5-4031-9721-eb154a666dc2
Singleton → bd73c8c0-98f5-41e5-9ac9-83ca6206307f
SingletonInstance → 00000000-0000-0000-0000-000000000000
```

> Fun fact : `Console.WriteLine` did not work for me unlike in Rahul video to output to the console.
> Guess what I did. Answer in `OutputLogger.cs` file. 😉

[Back to Top](#dependency-injection-as-of-asp-net-6)

### Multiple registration of the same service

Should we register the same service several times the last declaration takes precedence.

```csharp 
builder.Services.AddSingleton<IOperationSingletonInstance>(a => new Operation(Guid.Empty));
builder.Services.AddSingleton<IOperationSingletonInstance>(a => new Operation());
```

> Sample output after we declare another `IOperationSingletonInstance`
```
From Dependency Service 1
Transient → 47a5859b-c363-4a7f-ba46-5cdbc456db46
Scoped → b08bacaa-50ba-45cd-a844-120612099251
Singleton → f194449f-9228-491b-b835-899d9708323a
SingletonInstance → c37a1dea-ee74-4183-87e6-027ee638b797

From Dependency Service 2
Transient → 36543424-bd1b-4bcf-ad19-e783be4fb877
Scoped → b08bacaa-50ba-45cd-a844-120612099251
Singleton → f194449f-9228-491b-b835-899d9708323a
SingletonInstance → c37a1dea-ee74-4183-87e6-027ee638b797
```

Should we inject `IEnumerable<IOperationSingletonInstance>` in the controller, we would get all the declared instances.
In the controller action, we go through our collection to display them.

```csharp
_allSingletonInstances.ToList()
    .ForEach(entry =>
        _outputLogger.Log($"Instance → {entry.OperationId}"));
```

The output goes as expected.

> Sample output
```
Instance → 00000000-0000-0000-0000-000000000000
Instance → c37a1dea-ee74-4183-87e6-027ee638b797
```

This means that even-though only the last declared service is served, all declared services get instanciated and remain accessible (if injected).

[Back to Top](#dependency-injection-as-of-asp-net-6)

### Register a service only if non-existent

There are also a set of extension methods in the `Microsoft.Extensions.DependencyInjection.Extensions` package meant to register services that won't actually do it should there be already an instance available.
They start with the *Try* word.

- `TryAddScoped()`
- `TryAddSingleton()`
- `TryAddTransient()`

> Also there is `TryAddEnumerable()` which will add a `ServiceDescriptor` for a service only if the implementation differ.

```csharp
builder.Services.AddSingleton<IOperationSingletonInstance>(a => new Operation(Guid.Empty));
builder.Services.TryAddSingleton<IOperationSingletonInstance>(a => new Operation());
```

Should we use this on the second declaration for `IOperationSingletonInstance`, we would revert to the previous output.

```
From Dependency Service 1
Transient → 1e575e94-533f-4d58-91fb-2bf32d6945df
Scoped → f0359928-9511-4993-9a1c-c0dd108fc7f6
Singleton → 25a9600b-6b8b-4ac0-8085-9dee8cfd322c
SingletonInstance → 00000000-0000-0000-0000-000000000000

From Dependency Service 2
Transient → c854ed55-7174-4a6f-b838-43a7a0749db4
Scoped → f0359928-9511-4993-9a1c-c0dd108fc7f6
Singleton → 25a9600b-6b8b-4ac0-8085-9dee8cfd322c
SingletonInstance → 00000000-0000-0000-0000-000000000000

Instance → 00000000-0000-0000-0000-000000000000
```

[Back to Top](#dependency-injection-as-of-asp-net-6)

### Extra Read

- Framework-provided Services [@MSDN](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#framework-provided-services-1)
- Read the rules to stay compatible with Dependency Injection [@MSDN](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0#design-services-for-dependency-injection)
- Default Service Container Replacement [@MSDN](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines#default-service-container-replacement)

[Back to Top](#dependency-injection-as-of-asp-net-6)