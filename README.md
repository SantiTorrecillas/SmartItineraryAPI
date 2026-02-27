# Smart Itinerary API

A small ASP.NET Core Web API that demonstrates integrating with an AI provider (OpenAI), validation with FluentValidation, API documentation with Swagger, simple rate limiting, and containerized deployment with Docker. This project is intended to showcase API integration and Docker deployment knowledge.

---

## Table of contents
- [What it does](#what-it-does)
- [Requirements](#requirements)
- [Configuration](#configuration)
- [Run locally](#run-locally)
- [Docker](#docker)
- [API Endpoints](#api-endpoints)
- [Behavior & middleware](#behavior--middleware)
- [Caching](#caching)
- [Frontend demo](#frontend-demo)

---

## What it does
- Accepts itinerary generation requests (city, days, budget).
- Forwards a structured prompt to an OpenAI-compatible client and returns a parsed itinerary.
- Validates incoming requests using FluentValidation.
- Exposes a health endpoint and Swagger UI in Development.
- Applies a fixed-window rate limit for itinerary generation requests.

---

## Requirements
- .NET 9 SDK (project targets `net9.0`)
- Docker (for container builds)
- An OpenAI API key (stored securely; see Configuration)

Recommended IDE: Visual Studio 2022 or later. Use __Build > Rebuild Solution__ after changes to surface compile-time issues.

---

## Configuration

The project binds `OpenAiOptions` from the `OpenAI` configuration section. Example `appsettings.json` snippet:

{ "OpenAI": { "ApiKey": "<YOUR_API_KEY>", "Model": "gpt-mini" } }

Secure options:
- Use __User Secrets__ for local development: `dotnet user-secrets set "OpenAI:ApiKey" "<YOUR_API_KEY>"`
- Or set an environment variable: `OpenAI__ApiKey` (note the double-underscore binder convention)

The API reads `OpenAI:ApiKey` and `OpenAI:Model`.

---

## Run locally

1. Restore and build:
   - CLI: `dotnet restore && dotnet build`
   - Or in Visual Studio: use __Build > Rebuild Solution__.

2. Run:
   - CLI: `dotnet run --project SmartItineraryAPI` (or run from Visual Studio).

3. In Development, Swagger is exposed at:
   - `https://localhost:<port>/swagger`

---

## Docker

Build the image:

```
docker build -t smart-itinerary-api .
```

Run container (map container port 80 to host 8080 and provide OpenAI key):

```
docker run -e "OpenAI__ApiKey=<YOUR_API_KEY>" -p 8080:80 --rm smart-itinerary-api
```

Adjust port mapping if your Dockerfile exposes a different port. Verify the container receives the `OpenAI__ApiKey` environment variable; ASP.NET configuration will bind it to `OpenAI:ApiKey`.

---

## API Endpoints

1) Health
- GET `/api/system/health`
- Response (200):
	```json
	{ 
		"environment": "Development", 
		"service": "Smart Itinerary API", 
		"status": "Healthy", 
		"timestamp": "2026-02-18T12:34:56Z" 
	}
   ```
2) Generate itinerary (example)
- POST `/api/itinenary` (controller appears named `ItinenaryController` — check the controller for exact route)
- Request body:

    ```json
    {
        "city": "Rome",
        "days": 3,
        "budget": 1000
    }
    ```

    - Response (200):
    ```json
    {
        "city": "Rome",
        "days": 3,
        "daysPlan": [ /* ... */ ]
    }
    ```

---

## Behavior & middleware

- Validation: `FluentValidation` validators are registered from the assembly (e.g., `ItineraryRequestValidator`).
- Swagger: Enabled in Development environment (Swagger UI available).
- Rate limiting: A fixed-window limiter named `itinerary-policy` is configured allowing 5 requests per minute; when rejected returns HTTP 429 with a JSON error.
  - Important: ensure `UseRateLimiter()` is registered in the correct position in the pipeline (before `MapControllers()`).

## Caching

The API uses `IMemoryCache` to cache AI-generated itineraries and reduce repeated calls to the OpenAI service for identical requests.

- Key generation: cache keys are derived from a SHA-256 hash of the serialized `ItineraryRequest` to create a deterministic key: `itinerary:{hex-sha256(serialized-request)}`.
- Storage & TTL: the current implementation stores `ItineraryResponse` objects and sets an absolute expiration of six hours (`entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)`).
- Concurrency: `IMemoryCache.GetOrCreateAsync` is used to prevent duplicate concurrent fetches for the same key. Ensure the factory handles exceptions/cancellations to avoid caching failed results.
- Scaling: for multi-instance deployments use a distributed cache (e.g., Redis) instead of `IMemoryCache` to share cached data across instances.

## Frontend demo

A simple frontend is available to try the API in a browser:

- Frontend URL: https://smart-itinerary-front-end.vercel.app/

---

## Deployment / Live Demo

The Smart Itinerary API is deployed and accessible online at:

**https://smartitineraryapi.onrender.com**

- Health check (quick test):

**GET https://smartitineraryapi.onrender.com/api/system/health**
---
