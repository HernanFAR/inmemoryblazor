using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BlazorApp1.Client.Tests;

public class BlazorWasmAppFactory : WebApplicationFactory<Server.Pages.ErrorModel>
{
    private bool _disposed;
    private IHost? _host;
    private readonly Uri _baseUrl = new("http://127.0.0.1:8081");

    public BlazorWasmAppFactory()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = _baseUrl;

    }
    
    public Uri ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress;
        }
    }

    public override IServiceProvider Services
    {
        get
        {
            EnsureServer();
            return _host!.Services;
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Create the host for TestServer now before we
        // modify the builder to use Kestrel instead.
        var testHost = builder.Build();

        // Modify the host builder to use Kestrel instead
        // of TestServer so we can listen on a real address.
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        // Create and start the Kestrel server before the test server,
        // otherwise due to the way the deferred host builder works
        // for minimal hosting, the server will not get "initialized
        // enough" for the address it is listening on to be available.
        // See https://github.com/dotnet/aspnetcore/issues/33846.
        _host = builder.Build();
        _host.Start();

        // Extract the selected dynamic port out of the Kestrel server
        // and assign it onto the client options for convenience so it
        // "just works" as otherwise it'll be the default http://localhost
        // URL, which won't route to the Kestrel-hosted HTTP server.
        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();

        // Return the host that uses TestServer, rather than the real one.
        // Otherwise the internals will complain about the host's server
        // not being an instance of the concrete type TestServer.
        // See https://github.com/dotnet/aspnetcore/pull/34702.
        testHost.Start();

        return testHost;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());

        builder.ConfigureTestServices(services =>
        {

        });

        // Configure the server address for the server to
        // listen on for HTTPS requests on a dynamic port.

        builder.UseUrls(_baseUrl.ToString());
    }

    public async Task InitializeAsync()
    {
        EnsureServer();
    }

    private void EnsureServer()
    {
        if (_host is null)
        {
            // This forces WebApplicationFactory to bootstrap the server
            using var _ = CreateDefaultClient();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_disposed) return;

        if (disposing)
        {
            _host?.Dispose();
        }

        _disposed = true;
    }
}
