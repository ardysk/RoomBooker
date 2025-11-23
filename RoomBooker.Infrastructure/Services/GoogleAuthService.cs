using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using RoomBooker.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoomBooker.Infrastructure.Services
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _config;

        public GoogleAuthService(IConfiguration config)
        {
            _config = config;
        }

        private GoogleAuthorizationCodeFlow GetGoogleFlow()
        {
            var clientId = _config["Google:ClientId"];
            var clientSecret = _config["Google:ClientSecret"];

            return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { CalendarService.Scope.Calendar },
                DataStore = null
            });
        }

        public string GenerateAuthUrl(string redirectUri)
        {
            var flow = GetGoogleFlow();
            var request = (GoogleAuthorizationCodeRequestUrl)flow.CreateAuthorizationCodeRequest(redirectUri);
            request.Prompt = "consent";
            request.AccessType = "offline";
            return request.Build().ToString();
        }

        public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
        {
            var flow = GetGoogleFlow();
            return await flow.ExchangeCodeForTokenAsync(
                userId: "user",
                code: code,
                redirectUri: redirectUri,
                taskCancellationToken: CancellationToken.None
            );
        }

        public async Task AddReservationToCalendarAsync(User user, Reservation reservation)
        {
            // 1. Sprawdź, czy user ma tokeny
            if (string.IsNullOrEmpty(user.GoogleAccessToken) || string.IsNullOrEmpty(user.GoogleRefreshToken))
                return;

            var secrets = new ClientSecrets
            {
                ClientId = _config["Google:ClientId"],
                ClientSecret = _config["Google:ClientSecret"]
            };

            var tokenResponse = new TokenResponse
            {
                AccessToken = user.GoogleAccessToken,
                RefreshToken = user.GoogleRefreshToken,
                ExpiresInSeconds = 3600,
                IssuedUtc = DateTime.UtcNow
            };

            var credential = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = new[] { CalendarService.Scope.Calendar }
                }),
                "user",
                tokenResponse);

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "RoomBooker",
            });

            var ev = new Event
            {
                Summary = $"Rezerwacja: {reservation.Room?.Name ?? "Sala"}",
                Description = $"Cel: {reservation.Purpose}",
                Location = reservation.Room?.Name,
                Start = new EventDateTime { DateTimeDateTimeOffset = reservation.StartTimeUtc },
                End = new EventDateTime { DateTimeDateTimeOffset = reservation.EndTimeUtc }
            };

            await service.Events.Insert(ev, "primary").ExecuteAsync();
        }
    }
}