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

docker build -t smart-itinerary-api .

Run container (map container port 80 to host 8080 and provide OpenAI key):

docker run -e "OpenAI__ApiKey=<YOUR_API_KEY>" -p 8080:80 --rm smart-itinerary-api

Adjust port mapping if your Dockerfile exposes a different port. Verify the container receives the `OpenAI__ApiKey` environment variable; ASP.NET configuration will bind it to `OpenAI:ApiKey`.

---

## API Endpoints

1) Health
- GET `/api/system/health`
- Response (200):

{ 
	"environment": "Development", 
	"service": "Smart Itinerary API", 
	"status": "Healthy", 
	"timestamp": "2026-02-18T12:34:56Z" 
}

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
        "daysPlan": [
            {
                "dayNumber": 1,
                "plans": [
                    {
                        "title": "Visit the Colosseum",
                        "description": "Scheduled at 09:00",
                        "estimatedPrice": 18
                    },
                    {
                        "title": "Lunch at a local trattoria",
                        "description": "Scheduled at 12:00",
                        "estimatedPrice": 25
                    },
                    {
                        "title": "Explore Roman Forum and Palatine Hill",
                        "description": "Scheduled at 14:00",
                        "estimatedPrice": 22
                    }
                ]
            },
            {
                "dayNumber": 2,
                "plans": [
                    {
                        "title": "Visit Vatican Museums and Sistine Chapel",
                        "description": "Scheduled at 09:00",
                        "estimatedPrice": 30
                    },
                    {
                        "title": "Lunch near Vatican City",
                        "description": "Scheduled at 12:30",
                        "estimatedPrice": 20
                    },
                    {
                        "title": "St. Peter's Basilica visit and climb dome",
                        "description": "Scheduled at 14:00",
                        "estimatedPrice": 10
                    }
                ]
            },
            {
                "dayNumber": 3,
                "plans": [
                    {
                        "title": "Walking tour of Trastevere",
                        "description": "Scheduled at 09:00",
                        "estimatedPrice": 15
                    },
                    {
                        "title": "Lunch in Trastevere",
                        "description": "Scheduled at 12:00",
                        "estimatedPrice": 30
                    },
                    {
                        "title": "Visit Pantheon and Piazza Navona",
                        "description": "Scheduled at 14:00",
                        "estimatedPrice": 5
                    },
                    {
                        "title": "Gelato tasting in a famous gelateria",
                        "description": "Scheduled at 16:00",
                        "estimatedPrice": 10
                    }
                ]
            }
        ]
    }
    ```

---

## Behavior & middleware

- Validation: `FluentValidation` validators are registered from the assembly (e.g., `ItineraryRequestValidator`).
- Swagger: Enabled in Development environment (Swagger UI available).
- Rate limiting: A fixed-window limiter named `itinerary-policy` is configured allowing 5 requests per minute; when rejected returns HTTP 429 with a JSON error.
  - Important: ensure `UseRateLimiter()` is registered in the correct position in the pipeline (see Known Issues below).

---