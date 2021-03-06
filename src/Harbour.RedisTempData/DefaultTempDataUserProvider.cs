﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Harbour.RedisTempData
{
    public class DefaultTempDataUserProvider : ITempDataUserProvider
    {
        private const string defaultCookieName = "aid";
        private const string cachedHttpContextKey = "__DefaultTempDataUserProvider.User";

        private readonly string fallbackCookieName;
        private readonly ISessionIDManager sessionIdManager;

        public DefaultTempDataUserProvider()
            : this(defaultCookieName)
        {

        }

        public DefaultTempDataUserProvider(string fallbackCookieName)
            : this(fallbackCookieName, new SessionIDManager())
        {
            
        }

        // For testing.
        internal DefaultTempDataUserProvider(string fallbackCookieName, ISessionIDManager sessionIdManager)
        {
            this.fallbackCookieName = fallbackCookieName;
            this.sessionIdManager = sessionIdManager;
        }

        public string GetUser(ControllerContext context)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Use the same user for the duration of the request.
            if (httpContext.Items.Contains(cachedHttpContextKey))
            {
                return (string)httpContext.Items[cachedHttpContextKey];
            }

            var cookie = request.Cookies[fallbackCookieName];

            string user;

            // Fallback to the current session ID. However, if you're going 
            // this route, you should probably be using the default
            // SessionStateTempDataProvider in MVC :).
            if (httpContext.Session != null)
            {
                user = httpContext.Session.SessionID;
            }
            else if (httpContext.Request.IsAuthenticated)
            {
                // The user has gone from being an anonymous user to an 
                // authenticated user.
                if (IsValidCookie(cookie))
                {
                    // Even though we're authenticated, the anonymous ID is
                    // used for this request because we want to grab the temp
                    // data from the previous request (when the user was 
                    // unauthenticated).
                    user = cookie.Value;

                    // Expire the cookie since don't need the cookie anymore.
                    response.Cookies.Add(new HttpCookie(fallbackCookieName)
                    {
                        Expires = DateTime.UtcNow.AddYears(-1)
                    });
                }
                else
                {
                    user = httpContext.User.Identity.Name;
                }
            }
            else if (!IsValidCookie(cookie))
            {
                // The session ID manager is used to generate a secure ID that
                // is valid for a cookie (no reason to reinvent the wheel!).
                user = sessionIdManager.CreateSessionID(httpContext.ApplicationInstance.Context);

                // Issue a new cookie identifying the anonymous user.
                response.Cookies.Add(new HttpCookie(fallbackCookieName, user)
                {
                    HttpOnly = true
                });
            }
            else
            {
                user = cookie.Value;
            }

            httpContext.Items[cachedHttpContextKey] = user;
            return user;
        }

        private bool IsValidCookie(HttpCookie cookie)
        {
            return cookie != null && sessionIdManager.Validate(cookie.Value);
        }
    }
}
