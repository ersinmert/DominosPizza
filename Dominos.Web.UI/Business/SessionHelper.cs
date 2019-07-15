﻿using Dominos.Common.Classes;
using Dominos.Common.Configuration;
using Dominos.Common.Constants;
using Dominos.Common.DTO.Output;
using Dominos.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Dominos.Web.UI.Business
{
    public class SessionHelper
    {
        public SessionHelper(IHttpContextAccessor httpContextAccessor,
            CookieHelper cookie,
            IOptions<DominosConfig> options)
        {
            httpContext = httpContextAccessor.HttpContext;
            _cookie = cookie;
            _config = options.Value;
        }

        private readonly HttpContext httpContext;
        private readonly CookieHelper _cookie;
        private readonly DominosConfig _config;

        public string SessionId
        {
            get { return httpContext.Session.Id; }
        }

        public string BasketKey
        {
            get
            {
                var cookieBasketKey = _cookie.Get(CookieKey.CustomerId);
                if (cookieBasketKey == null)
                {
                    cookieBasketKey = SessionId;
                    _cookie.Set(CookieKey.CustomerId, cookieBasketKey);
                }
                return cookieBasketKey;
            }
        }

        public CustomerOutputDTO Customer
        {
            get
            {
                var customer = Get<CustomerOutputDTO>(SessionKey.Customer);
                if (customer == null)
                {
                    try
                    {
                        var customerId = Convert.ToInt32(_cookie.Get(CookieKey.CustomerId));
                        if (customerId != default(int))
                        {
                            var url = $"{_config.DominosApiUrl}{_config.CustomerServices.CustomerById}?customerId={customerId}";
                            customer = HttpHelper.Get<ResponseEntity<CustomerOutputDTO>>(url)?.Result;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return customer;
            }
        }

        public int? CustomerId
        {
            get
            {
                var customerId = Customer?.CustomerId;
                if (customerId == null)
                {
                    try
                    {
                        customerId = Convert.ToInt32(_cookie.Get(CookieKey.CustomerId));
                    }
                    catch (Exception)
                    {

                    }
                }
                return customerId;
            }
        }

        public T Get<T>(string key)
        {
            try
            {
                var jsonObject = httpContext.Session.GetString(key);
                return JsonConvert.DeserializeObject<T>(jsonObject);
            }
            catch
            {
                return default(T);
            }
        }

        public void Set(string key, object value)
        {
            var valueJson = JsonConvert.SerializeObject(value);
            httpContext.Session.SetString(key, valueJson);
        }

        public void End()
        {
            httpContext.Session.Clear();
        }
    }
}