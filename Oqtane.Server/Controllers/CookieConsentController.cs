using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Globalization;
using Oqtane.Infrastructure;
using Oqtane.Services;
using System.Threading.Tasks;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class CookieConsentController : Controller
    {
        private readonly ICookieConsentService _cookieConsentService;

        public CookieConsentController(ICookieConsentService cookieConsentService)
        {
            _cookieConsentService = cookieConsentService;
        }

        [HttpGet("IsActioned")]
        public async Task<bool> IsActioned()
        {
            return await _cookieConsentService.IsActionedAsync();
        }

        [HttpGet("CanTrack")]
        public async Task<bool> CanTrack(string optout)
        {
            return await _cookieConsentService.CanTrackAsync(bool.Parse(optout));
        }

        [HttpGet("CreateActionedCookie")]
        public async Task<string> CreateActionedCookie()
        {
            return await _cookieConsentService.CreateActionedCookieAsync();
        }

        [HttpGet("CreateConsentCookie")]
        public async Task<string> CreateConsentCookie()
        {
            return await _cookieConsentService.CreateConsentCookieAsync();
        }

        [HttpGet("WithdrawConsentCookie")]
        public async Task<string> WithdrawConsentCookie()
        {
            return await _cookieConsentService.WithdrawConsentCookieAsync();
        }
    }
}
