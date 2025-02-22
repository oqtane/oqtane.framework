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

        [HttpGet("CanTrack")]
        public async Task<bool> CanTrack()
        {
            return await _cookieConsentService.CanTrackAsync();
        }

        [HttpGet("CreateConsentCookie")]
        public async Task<string> CreateConsentCookie()
        {
            return await _cookieConsentService.CreateConsentCookieAsync();
        }
    }
}
