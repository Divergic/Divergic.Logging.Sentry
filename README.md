# Introduction    

The Divergic.Logging.Sentry packages provide ```ILogger``` support for sending exceptions to [Sentry](https://sentry.io).

# Installation

There are several NuGet packages to give flexibility around how applications can use this feature.

- ```Install-Package Divergic.Logging.Sentry``` [on NuGet.org][2] contains the ```ILogger``` provider for sending errors to Sentry
- ```Install-Package Divergic.Logging.Sentry.Autofac``` [on NuGet.org][3] contains a helper module to registering ```RavenClient``` in Autofac
- ```Install-Package Divergic.Logging.Sentry.NodaTime``` [on NuGet.org][4] contains an extension to the base package to support serialization of NodaTime data types
- ```Install-Package Divergic.Logging.Sentry.All``` [on NuGet.org][5] is a meta package that contains all the above packages

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

Exceptions in .Net often contain additional information that is not included in the ```Exception.Message``` property. Some good examples of this are ```SqlException``` and the Azure ```StorageException```. This information is additional metadata about the error which is critical to identifying the error. Unfortunately most logging systems will only log the exception stacktrace and message. The result is an error report that is not actionable.

The Divergic.Logging.Sentry package caters for this by automatically adding each custom property on the exception to the error report sent to Sentry.

# Logging context data

Logging just the exception information may not provide enough information. You can provide any context data you like with this package by using the ```LogErrorWithContext``` and ```LogCriticalWithContext``` extension methods on ```ILogger```.

```csharp
public async Task ProcessPayment(string invoiceId, int amountInCents, Person customer, CancellationToken cancellationToken)
{
    try
    {
        await _gateway.ProcessPayment(invoiceId, amountInCents, customer.Email, cancellationToken).ConfigureAwait(false);
    }
    catch (PaymentGatewayException ex)
    {
        var paymentDetails = new {
            invoiceId,
            amountInCents
        };
        
        _logger.LogErrorWithContext(ex, paymentDetails);
    }
}
```

## Preventing duplicate reports

Exceptions can be caught, logged and then thrown again. A catch block higher up the callstack may then log the same exception again. This scenario is handled by ensuring that any particular exception instance is only reported to Sentry once.

## Supporting Autofac

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

## Support NodaTime serialization

Sending custom exception properties and exception context data to Sentry is really useful, but only as useful as how the data can be understood. ```NodaTime.Instant``` is a good example of this. The native JSON serialization of an Instant value will be ```{}``` rather than a readable date/time value. 

The Divergic.Logging.Sentry.NodaTime package provides a new extension method on ```ILoggerFactory``` to configure Sentry with NodaTime support.

```csharp
public void Configure(
    IApplicationBuilder app,
    IHostingEnvironment env,
    ILoggerFactory loggerFactory,
    IApplicationLifetime appLifetime)
{
    // You will need to create the RavenClient before configuring the logging integration
    var ravenClient = BuildRavenClient();
    
    loggerFactory.AddSentryWithNodaTime(ravenClient);
}
```

Any NodaTime data type found will be correctly serialized in the Sentry error report.

[0]: https://raygun.com/
[1]: https://sentry.io/welcome/
[2]: https://www.nuget.org/packages/Divergic.Logging.Sentry
[3]: https://www.nuget.org/packages/Divergic.Logging.Sentry.Autofac
[4]: https://www.nuget.org/packages/Divergic.Logging.Sentry.NodaTime
[5]: https://www.nuget.org/packages/Divergic.Logging.Sentry.All