using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.ActionFilters.Detection;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.Services
{
    public class ReCaptchaService
    {
        private readonly ReCaptchaClient _reCaptchaClient;
        private readonly ReCaptchaSettings _settings;
        private readonly IEnumerable<IDetectRobots> _robotDetectors;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReCaptchaService> _logger;
        private readonly IStringLocalizer<ReCaptchaService> S;

        public ReCaptchaService(ReCaptchaClient reCaptchaClient, IOptions<ReCaptchaSettings> optionsAccessor, IEnumerable<IDetectRobots> robotDetectors, IHttpContextAccessor httpContextAccessor, ILogger<ReCaptchaService> logger, IStringLocalizer<ReCaptchaService> stringLocalizer)    
        {
            _reCaptchaClient = reCaptchaClient;
            _settings = optionsAccessor.Value;
            _robotDetectors = robotDetectors;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            S = stringLocalizer;
        }

        /// <summary>
        /// Flags the behavior as that of a robot 
        /// </summary>
        public void MaybeThisIsARobot()
        {
            _robotDetectors.Invoke(i => i.FlagAsRobot(), _logger);
        }

        /// <summary>
        /// Determines if the request has been made by a robot
        /// </summary>
        /// <returns>Yes (true) or no (false)</returns>
        public bool IsThisARobot()
        {
            var result = _robotDetectors.Invoke(i => i.DetectRobot(), _logger);
            return result.Any(a => a.IsRobot);
        }

        /// <summary>
        /// Clears all robot markers, we are dealing with a human
        /// </summary>
        /// <returns></returns>
        public void ThisIsAHuman()
        {
            _robotDetectors.Invoke(i => i.IsNotARobot(), _logger);
        }

        /// <summary>
        /// Verifies the ReCaptcha response with the ReCaptcha webservice
        /// </summary>
        /// <param name="reCaptchaResponse"></param>
        /// <returns></returns>
        public async Task<bool> VerifyCaptchaResponseAsync(string reCaptchaResponse)
        {
            return !String.IsNullOrWhiteSpace(reCaptchaResponse) && await _reCaptchaClient.VerifyAsync(reCaptchaResponse, _settings.SecretKey);
        }

        /// <summary>
        /// Validates the captcha that is in the Form of the current request
        /// </summary>
        /// <param name="reportError">Lambda for reporting errors</param>
        public async Task<bool> ValidateCaptchaAsync(Action<string, string> reportError)
        {
            var reCaptchaResponse = _httpContextAccessor.HttpContext?.Request?.Form?[Constants.ReCaptchaServerResponseHeaderName].ToString();

            var isValid = !String.IsNullOrEmpty(reCaptchaResponse) && await VerifyCaptchaResponseAsync(reCaptchaResponse);

            if (!isValid)
            {
                reportError("ReCaptcha", S["Failed to validate captcha"]);
            }

            return isValid;
        }
    }
}
