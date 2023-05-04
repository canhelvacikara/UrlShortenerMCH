#region Using Part
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualBasic;
using UrlShortenerMCH.Constants;
using UrlShortenerMCH.Entities;
#endregion

#region Application and Database

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(_ => new LiteDatabase("short-links.db"));
await using var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", ctx =>
{
    ctx.Response.ContentType = "text/html";
    return ctx.Response.SendFileAsync("index.html");
});

app.MapPost("/url", ShortenerDelegate);

app.MapFallback(RedirectDelegate);

await app.RunAsync();
#endregion

#region Create Short Url
static async Task ShortenerDelegate(HttpContext httpContext)
{
    var request = await httpContext.Request.ReadFromJsonAsync<InputUrl>() ?? new InputUrl();
    ResultSet resultDto = new ResultSet();

    //Validation for invalid url
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var inputUri))
    {
        resultDto.HasError = true;
        resultDto.StatusCode = StatusCodes.Status400BadRequest;
        resultDto.Description = ConstantStrings.InvalidUrlMessage;
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(resultDto);
        return;
    }

    //Validation for max 6 character length of custom url
    if(request.CustomUrl !=null && request.CustomUrl.Length >6 )
    {
        resultDto.HasError = true;
        resultDto.StatusCode = StatusCodes.Status400BadRequest;
        resultDto.Description = ConstantStrings.UrlLengthMessage;
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(resultDto);
        return;
    }

    var liteDb = httpContext.RequestServices.GetRequiredService<ILiteDatabase>();
    var links = liteDb.GetCollection<ShortUrl>(BsonAutoId.Int32);
    ShortUrl? entry = null;
    //Check if existing url
    if (request.CustomUrl != null)
    {
        entry = links.FindOne(d => d.CustomUrl == request.CustomUrl);
        if (entry != null)
        {
            resultDto.HasError = true;
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            resultDto.StatusCode = StatusCodes.Status400BadRequest;
            resultDto.Description = ConstantStrings.ExistingUrlMessage;
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(resultDto);
            return;
        }
    }
    entry = new ShortUrl(inputUri, request.CustomUrl);
    links.Insert(entry);
    var result = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{entry.UrlChunk}";
    resultDto.HasError = false;
    resultDto.Url = result;
    await httpContext.Response.WriteAsJsonAsync(resultDto);
}
#endregion

#region Redirect to Main Url
static async Task RedirectDelegate(HttpContext httpContext)
{
    var db = httpContext.RequestServices.GetRequiredService<ILiteDatabase>();
    var collection = db.GetCollection<ShortUrl>();

    var path = httpContext.Request.Path.ToUriComponent().Trim('/');
    var entry = collection.Find(p => p.CustomUrl == path).FirstOrDefault();
    if (entry != null)
    {
        httpContext.Response.Redirect(entry?.Url ?? "/");
    }
    else
    {
        var id = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(path));
        entry = collection.Find(p => p.Id == id).FirstOrDefault();
        httpContext.Response.Redirect(entry?.Url ?? "/");
    }

    await Task.CompletedTask;
}

#endregion

