﻿using Dominos.Common.Classes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace Dominos.Api.Controllers
{
    public class BaseController : ControllerBase
    {
        public IActionResult HttpEntity<T>(ResponseEntity<T> response)
        {
            switch (response.HttpCode)
            {
                case HttpStatusCode.OK: return Ok(response);
                case HttpStatusCode.InternalServerError: return StatusCode(500, response);
                case HttpStatusCode.NotFound: return NotFound(response);
                case HttpStatusCode.BadRequest: return BadRequest(response);
                case HttpStatusCode.Unauthorized: return Unauthorized(response);
                default: throw new ApplicationException();
            }
        }
    }
}