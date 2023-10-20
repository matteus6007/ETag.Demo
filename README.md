# ETag Action Filter Demo

Action Filter for generating an ETag.

## Configuring Application

Add globally:

```
builder.Services.AddControllers(options =>
{
    // comment out to remove global ETag filter
    options.Filters.Add<ETagAttribute>();
});
```

For more control comment out global filter (see above) and add `[ETag]` attribute to a controller or action.

## Running Application

```
dotnet run --project ETag.Demo.Api/ETag.Demo.Api.csproj
```

## Testing Application

Get request without ETag - returns `200 - OK`:

```
curl -v --location https://localhost:7145/api/HealthCheck
```

Returns:

```
< HTTP/1.1 200 OK
< Content-Type: application/json; charset=utf-8
< Date: Thu, 19 Oct 2023 09:54:05 GMT
< Server: Kestrel
< ETag: 091522979350EA738566E350E738B350
< Transfer-Encoding: chunked

{"healthy":"OK"}
```

Get request with ETag - returns `304 - Not Modified`:

```
curl -v --location https://localhost:7145/api/HealthCheck  --header 'If-None-Match:091522979350EA738566E350E738B350'
```

Returns:

```
< HTTP/1.1 304 Not Modified
< Date: Thu, 19 Oct 2023 10:00:04 GMT
< Server: Kestrel
< ETag: 091522979350EA738566E350E738B350
```