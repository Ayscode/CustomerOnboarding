﻿using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace CustomerOnboarding.Middleware
{
    public class ApiKeyRequirement : IAuthorizationRequirement
    {
        public string EpApiKey { get; }

        public ApiKeyRequirement(string epApiKeyRequirement)
        {
            EpApiKey = epApiKeyRequirement;
        }
    }
}
