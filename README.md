# Introduction    

The Divergic.Logging.Sentry collection of packages provide ```ILogger``` support for sending exceptions to [Sentry.io](https://sentry.io). They build upon the [Divergic.Logging](https://github.com/Divergic/Divergic.Logging) package [on NuGet](https://www.nuget.org/packages/Divergic.Logging) which provides extension methods for adding context information when logging exceptions.

# Installation

There are several NuGet packages to give flexibility around how applications can use this feature.

- ```Install-Package Divergic.Logging.Sentry``` [on NuGet.org](https://www.nuget.org/packages/Divergic.Logging.Sentry) contains the ```ILogger``` provider for sending errors to Sentry
- ```Install-Package Divergic.Logging.Sentry.Autofac``` [on NuGet.org](https://www.nuget.org/packages/Divergic.Logging.Sentry.Autofac) contains a helper module to registering ```RavenClient``` in Autofac
- ```Install-Package Divergic.Logging.Sentry.All``` [on NuGet.org](https://www.nuget.org/packages/Divergic.Logging.Sentry.All) is a meta package that contains all the above packages
- ```Install-Package Divergic.Logging.NodaTime``` [on NuGet.org](https://www.nuget.org/packages/Divergic.Logging.NodaTime) contains an ```ILoggerFactory``` extension to support serialization of NodaTime data types

# Configuration

Adding the Divergic.Logging.Sentry package will give access to the ```AddSentry``` method on ```ILoggerFactory```. You must provide an ```IRavenClient``` value at this point.

```csharp
public void Configure(
    IApplicationBuilder app,
    IHostingEnvironment env,
    ILoggerFactory loggerFactory,
    IApplicationLifetime appLifetime)
{
    // You will need to create the RavenClient before configuring the logging integration
    var ravenClient = BuildRavenClient();
    
    loggerFactory.AddSentry(ravenClient);
}
```

Any log message that contains a ```System.Exception``` parameter will be sent to Sentry. All other log messages will be ignored by this logging provider.

# Custom properties

Exceptions in .Net often contain additional information that is not included in the ```Exception.Message``` property. Some good examples of this are ```SqlException```, ```ReflectionTypeLoadException ``` and the Azure ```StorageException```. This information is additional metadata about the exception which is critical to identifying the error. Unfortunately most logging systems will only log the exception stacktrace and message. The result is an error report that is unactionable.

The Divergic.Logging.Sentry package caters for this by automatically adding each custom property on the exception to the error report sent to Sentry.

# Preventing duplicate reports

Exceptions can be caught, logged and then thrown again. A catch block higher up the callstack may then log the same exception again. This scenario is handled by ensuring that any particular exception instance is only reported to Sentry once.

# Supporting Autofac

The Divergic.Logging.Sentry.Autofac package contains a module to assist with setting up Sentry in an Autofac container. The requirement is that the application bootstrapping has already been able to create a ```ISentryConfig``` class and registered it in Autofac. The Autofac module registers a new RavenClient instance using that configuration.

Here is an example for ASP.Net core.

```csharp
internal static IContainer BuildContainer(ISentryConfig sentryConfig)
{
    var builder = new ContainerBuilder();
    
    builder.AddInstance(sentryConfig).As<ISentryConfig>();
    builder.AddModule<SentryModule>();
    
    return builder.Build();
}

public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // Load this from a configuration, this is just an example
    var sentryConfig = new SentryConfig
    {
        Dsn = "Sentry DSN value here",
        Environment = "Local",
        Version = "0.1.0"
    }

    var container = BuildContainer(sentryConfig);
    
    var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
    var ravenClient = container.Resolve<IRavenClient>();    
    
    loggerFactory.AddSentry(ravenClient);
    
    // Create the IServiceProvider based on the container.
    return new AutofacServiceProvider(container);
}
```

## Logging exception context data

In addition to logging custom exception properties, these packages can also include additional context data and custom serialization for sending errors to Sentry.

See the related [Divergic.Logging and Divergic.Logging.NodaTime packages](https://github.com/Divergic/Divergic.Logging) for further information.