﻿using Arc4u.OAuth2.Token;
using Arc4u.Serializer;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Arc4u.Standard.UnitTest.Serialization
{
    public class CompressionAndSpeedTests
    {
        /// <summary>
        /// A normal and a "zuppafat" bearer token
        /// </summary>
        private static readonly string[] _bearerTokens =
        {
            "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IjJLdGtudlhUUzIwRFBYT1ZITHQzOHJFcm5XMCIsImtpZCI6IjJLdGtudlhUUzIwRFBYT1ZITHQzOHJFcm5XMCJ9.eyJhdWQiOiJodHRwczovL2JtYXAubG9jYWxob3N0Lmlpc2hvc3QiLCJpc3MiOiJodHRwOi8vYWRmc2Rldi5iZWxncmlkLm5ldC9hZGZzL3NlcnZpY2VzL3RydXN0IiwiaWF0IjoxNjY2MDc0Njg4LCJuYmYiOjE2NjYwNzQ2ODgsImV4cCI6MTY2NjExMDY4OCwiaHR0cDovL3NjaGVtYXMuYXJjNHUubmV0L3dzLzIwMTIvMDUvaWRlbnRpdHkvY2xhaW1zL2N1bHR1cmUiOiJubC1OTCIsImdpdmVuX25hbWUiOiJWaW5jZW50IiwiZmFtaWx5X25hbWUiOiJWYW4gRGVuIEJlcmdoZSIsImVtYWlsIjoiVmluY2VudC5WYW5EZW5CZXJnaGVAZWxpYS5iZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2hvbWVwaG9uZSI6IiIsImh0dHA6Ly9zY2hlbWFzLmFyYzR1Lm5ldC93cy8yMDEyLzA1L2lkZW50aXR5L2NsYWltcy9jb21wYW55IjoiRWxpYSIsImh0dHA6Ly9zY2hlbWFzLmFyYzR1Lm5ldC93cy8yMDEyLzA1L2lkZW50aXR5L2NsYWltcy9zaWQiOiJTLTEtOS0yMS0yMTM3MTQzOTU5LTE4NzY2OTM1MDYtMjU4MDM5ODU2My04MzE4NCIsInVwbiI6IlZWMDAwNkBCZWxncmlkLm5ldCIsIndpbmFjY291bnRuYW1lIjoiVlYwMDA2IiwicHJpbWFyeXNpZCI6IlMtMS01LTIxLTIxMzcxNDM5NTktMTg3NjY5MzUwNi0yNTgwMzk4NTYzLTgzMTg0IiwiaHR0cDovL3NjaGVtYXMuYXJjNHUubmV0L3dzLzIwMTIvMDUvaWRlbnRpdHkvY2xhaW1zL2F1dGhvcml6YXRpb24iOiJ7XCJBbGxPcGVyYXRpb25zXCI6W3tcIklEXCI6MTYsXCJOYW1lXCI6XCJBY2Nlc3NBcHBsaWNhdGlvblwifSx7XCJJRFwiOjIsXCJOYW1lXCI6XCJBY2Nlc3NBcHBsaWNhdGlvbkV4dFwifSx7XCJJRFwiOjEsXCJOYW1lXCI6XCJBY2Nlc3NBcHBsaWNhdGlvbkludFwifSx7XCJJRFwiOjI5LFwiTmFtZVwiOlwiQUZSUlJlcG9ydEV4dFwifSx7XCJJRFwiOjMwLFwiTmFtZVwiOlwiQUZSUlJlcG9ydEludFwifSx7XCJJRFwiOjYsXCJOYW1lXCI6XCJCaWRPdmVydmlld0V4dFwifSx7XCJJRFwiOjUsXCJOYW1lXCI6XCJCaWRPdmVydmlld0ludFwifSx7XCJJRFwiOjExLFwiTmFtZVwiOlwiQmlkUmVwb3J0RXh0XCJ9LHtcIklEXCI6MTAsXCJOYW1lXCI6XCJCaWRSZXBvcnRJbnRcIn0se1wiSURcIjoxMDAsXCJOYW1lXCI6XCJCaXBsZTAwMUJpZGRpbmdDYW5Db252ZXJ0QW5kU2VuZEZyb21FeGNlbFwifSx7XCJJRFwiOjEwMSxcIk5hbWVcIjpcIkJpcGxlMDAyQmlkZGluZ0NhbkNvbnZlcnRBbmRTZW5kRnJvbUV4Y2VsQXNNYXJrZXRQYXJ0eVwifSx7XCJJRFwiOjE3MixcIk5hbWVcIjpcIkJpcGxlMDAzQmlkZGluZ0NhbkNvbnZlcnRCaWRGcm9tRXhjZWxUb0JpZEZvcm1hdEZvclRlc3RpbmdcIn0se1wiSURcIjoxNzMsXCJOYW1lXCI6XCJCaXBsZTAwNEJpZGRpbmdDYW5Db252ZXJ0RnJvbUV4Y2VsVG9CYWNrdXBEUEZvcm1hdEZvclRlc3RpbmdcIn0se1wiSURcIjoxMDIsXCJOYW1lXCI6XCJCaXBsZTAwNUJpZGRpbmdDYW5TZW5kUHJlcXVhbGlmaWNhdGlvbkJpZFwifSx7XCJJRFwiOjEwMyxcIk5hbWVcIjpcIkJpcGxlMDA2QmlkZGluZ0NhblNlbmRQcmVxdWFsaWZpY2F0aW9uQmlkQXNNYXJrZXRQYXJ0eVwifSx7XCJJRFwiOjEwNyxcIk5hbWVcIjpcIkJpcGxlMDEwQmlkUmVmZXJlbnRpYWxDYW5BcHBseUNSSUZpbHRlclwifSx7XCJJRFwiOjEwOCxcIk5hbWVcIjpcIkJpcGxlMDExQmlkUmVmZXJlbnRpYWxDYW5DYW5jZWxBcHBsaWVkQ1JJRmlsdGVyXCJ9LHtcIklEXCI6MTExLFwiTmFtZVwiOlwiQmlwbGUwMTJCaWRSZWZlcmVudGlhbENhbkV4cG9ydFRvQmFja3VwRHBUZW1wbGF0ZVwifSx7XCJJRFwiOjExMixcIk5hbWVcIjpcIkJpcGxlMDEzQmlkUmVmZXJlbnRpYWxDYW5FeHBvcnRUb0JhY2t1cERwVGVtcGxhdGVBc01hcmtldFBhcnR5XCJ9LHtcIklEXCI6MTE5LFwiTmFtZVwiOlwiQmlwbGUwMTRCaWRSZWZlcmVudGlhbENhbkV4cG9ydFRvQmlkVGVtcGxhdGVcIn0se1wiSURcIjoxMjAsXCJOYW1lXCI6XCJCaXBsZTAxNUJpZFJlZmVyZW50aWFsQ2FuRXhwb3J0VG9CaWRUZW1wbGF0ZUFzTWFya2V0UGFydHlcIn0se1wiSURcIjoxMjEsXCJOYW1lXCI6XCJCaXBsZTAxNk1vbml0b3JpbmdDYW5HZXRBZ2dyZWdhdGVkVm9sdW1lXCJ9LHtcIklEXCI6MTIyLFwiTmFtZVwiOlwiQmlwbGUwMTdNb25pdG9yaW5nQ2FuR2V0QWdncmVnYXRlZFZvbHVtZUFzTWFya2V0UGFydHlcIn0se1wiSURcIjoyMDgsXCJOYW1lXCI6XCJCaXBsZTAxOE1vbml0b3JpbmdDYW5HZXRBZ2dyZWdhdGVkVm9sdW1lUGVyQ29tcGFueVwifSx7XCJJRFwiOjEwNixcIk5hbWVcIjpcIkJpcGxlMDE5QmlkUmVmZXJlbnRpYWxDYW5HZXRBcHBsaWVkQ1JJRmlsdGVyXCJ9LHtcIklEXCI6MTA5LFwiTmFtZVwiOlwiQmlwbGUwMjBCaWRSZWZlcmVudGlhbENhbkdldEJhY2t1cERQRGV0YWlsXCJ9LHtcIklEXCI6MTEwLFwiTmFtZVwiOlwiQmlwbGUwMjFCaWRSZWZlcmVudGlhbENhbkdldEJhY2t1cERQRGV0YWlsQXNNYXJrZXRQYXJ0eVwifSx7XCJJRFwiOjExNixcIk5hbWVcIjpcIkJpcGxlMDIyQmlkUmVmZXJlbnRpYWxDYW5HZXRCaWREZXRhaWxBc01hcmtldFBhcnR5XCJ9LHtcIklEXCI6MTEzLFwiTmFtZVwiOlwiQmlwbGUwMjNCaWRSZWZlcmVudGlhbENhbkdldEJpZERldGFpbEJ5VmVyc2lvbklkZW50aWZpZXJcIn0se1wiSURcIjoxMTQsXCJOYW1lXCI6XCJCaXBsZTAyNEJpZFJlZmVyZW50aWFsQ2FuR2V0QmlkRGV0YWlsQnlWZXJzaW9uSWRlbnRpZmllckFzTWFya2V0UGFydHlcIn0se1wiSURcIjoxMTcsXCJOYW1lXCI6XCJCaXBsZTAyNUJpZFJlZmVyZW50aWFsQ2FuR2V0Qmlkc0J5RGF5UHJvZHVjdEFuZERpcmVjdGlvblwifSx7XCJJRFwiOjExOCxcIk5hbWVcIjpcIkJpcGxlMDI2QmlkUmVmZXJlbnRpYWxDYW5HZXRCaWRzQnlFbGVjdHJpY2FsWm9uZVwifSx7XCJJRFwiOjIxMSxcIk5hbWVcIjpcIkJpcGxlMDI3QmlkUmVmZXJlbnRpYWxDYW5HZXRJbnRlcm5hbEJpZERldGFpbFwifSx7XCJJRFwiOjEyMyxcIk5hbWVcIjpcIkJpcGxlMDI4QmlkUmVmZXJlbnRpYWxDYW5HZXRVbmF2YWlsYWJpbGl0aWVzXCJ9LHtcIklEXCI6MTI0LFwiTmFtZVwiOlwiQmlwbGUwMjlCaWRSZWZlcmVudGlhbENhblNhdmVVbmF2YWlsYWJpbGl0aWVzXCJ9LHtcIklEXCI6MjE2LFwiTmFtZVwiOlwiQmlwbGUwMzBSZXBvcnRpbmdDYW5FeHBvcnRSZXBvcnRcIn0se1wiSURcIjoxMDQsXCJOYW1lXCI6XCJCaXBsZTA0MENhbkdldEVudmlyb25tZW50SW5mb1wifSx7XCJJRFwiOjEwNSxcIk5hbWVcIjpcIkJpcGxlMDQxQ2FuTWFuYWdlU2VydmVyTG9nZ2luZ0xldmVsXCJ9LHtcIklEXCI6MTM1LFwiTmFtZVwiOlwiQmlwbGUwNTBDb250cmFjdGluZ0NhbkdldEFsbFBhcmFtZXRlcnNcIn0se1wiSURcIjoyMDMsXCJOYW1lXCI6XCJCaXBsZTA1MUNvbnRyYWN0aW5nQ2FuR2V0Q29tcGFuaWVzXCJ9LHtcIklEXCI6MTI2LFwiTmFtZVwiOlwiQmlwbGUwNTJDb250cmFjdGluZ0NhbkdldENvbXBhbmllc0FzTWFya2V0UGFydHlcIn0se1wiSURcIjoxMjcsXCJOYW1lXCI6XCJCaXBsZTA1M0NvbnRyYWN0aW5nQ2FuR2V0Q29udHJhY3RzXCJ9LHtcIklEXCI6MTI4LFwiTmFtZVwiOlwiQmlwbGUwNTRDb250cmFjdGluZ0NhbkdldENvbnRyYWN0c0FzTWFya2V0UGFydHlcIn0se1wiSURcIjoxMjksXCJOYW1lXCI6XCJCaXBsZTA1NUNvbnRyYWN0aW5nQ2FuR2V0Q1JJTGV2ZWxzXCJ9LHtcIklEXCI6MjA2LFwiTmFtZVwiOlwiQmlwbGUwNTZDb250cmFjdGluZ0NhbkdldERlbGl2ZXJ5UG9pbnRzXCJ9LHtcIklEXCI6MTMxLFwiTmFtZVwiOlwiQmlwbGUwNTdDb250cmFjdGluZ0NhbkdldERlbGl2ZXJ5UG9pbnRzQXNNYXJrZXRQYXJ0eVwifSx7XCJJRFwiOjIwNCxcIk5hbWVcIjpcIkJpcGxlMDU4Q29udHJhY3RpbmdDYW5HZXRIZWxwTGlua3NcIn0se1wiSURcIjoxMzgsXCJOYW1lXCI6XCJCaXBsZTA1OUNvbnRyYWN0aW5nQ2FuR2V0SGVscExpbmtzQXNNYXJrZXRQYXJ0eVwifSx7XCJJRFwiOjEzMyxcIk5hbWVcIjpcIkJpcGxlMDYwQ29udHJhY3RpbmdDYW5HZXRQYXJhbWV0ZXJzXCJ9LHtcIklEXCI6MTM0LFwiTmFtZVwiOlwiQmlwbGUwNjFDb250cmFjdGluZ0NhbkdldFBhcmFtZXRlcnNBc01hcmtldFBhcnR5XCJ9LHtcIklEXCI6MTMyLFwiTmFtZVwiOlwiQmlwbGUwNjJDb250cmFjdGluZ0NhbkdldFZhbGlkRGVsaXZlcnlQb2ludHNCeUVsZWN0cmljYWxab25lc1wifSx7XCJJRFwiOjE3NSxcIk5hbWVcIjpcIkJpcGxlMDYzQ29udHJhY3RpbmdDYW5JbXBvcnRDUklMZXZlbHNGb3JUZXN0aW5nXCJ9LHtcIklEXCI6MTM2LFwiTmFtZVwiOlwiQmlwbGUwNjRDb250cmFjdGluZ0NhblNhdmVQYXJhbWV0ZXJzXCJ9LHtcIklEXCI6MTQ1LFwiTmFtZVwiOlwiQmlwbGUwNzBJbnRlcmZhY2VCaWRkaW5nQ2FuUHJvY2Vzc1dhaXRpbmdGb3JDb25maXJtYXRpb25CaWRzXCJ9LHtcIklEXCI6MTUzLFwiTmFtZVwiOlwiQmlwbGUwNzFJbnRlcmZhY2VCaWRSZWZlcmVudGlhbENhbkFwcGx5U2NoZWR1bGVkQ1JJRmlsdGVyXCJ9LHtcIklEXCI6MTUwLFwiTmFtZVwiOlwiQmlwbGUwNzJJbnRlcmZhY2VCaWRSZWZlcmVudGlhbENhbkV4cG9ydEJpZENvbmZpcm1hdGlvblwifSx7XCJJRFwiOjE0NyxcIk5hbWVcIjpcIkJpcGxlMDczSW50ZXJmYWNlQmlkUmVmZXJlbnRpYWxDYW5HZXRBRlJSQmlkc0xlZ2FjeVZlcnNpb25cIn0se1wiSURcIjoxNTIsXCJOYW1lXCI6XCJCaXBsZTA3NEludGVyZmFjZUJpZFJlZmVyZW50aWFsQ2FuR2V0QmFja3VwRGVsaXZlcnlQb2ludHNcIn0se1wiSURcIjoxNDgsXCJOYW1lXCI6XCJCaXBsZTA3NUludGVyZmFjZUJpZFJlZmVyZW50aWFsQ2FuR2V0Qmlkc1wifSx7XCJJRFwiOjE0OSxcIk5hbWVcIjpcIkJpcGxlMDc2SW50ZXJmYWNlQmlkUmVmZXJlbnRpYWxDYW5HZXRCaWRzVXBkYXRlZEFmdGVyR0NUXCJ9LHtcIklEXCI6MTUxLFwiTmFtZVwiOlwiQmlwbGUwNzdJbnRlcmZhY2VCaWRSZWZlcmVudGlhbENhblJlamVjdE5vbkNvbmZpcm1lZEJpZHNcIn0se1wiSURcIjoxNDYsXCJOYW1lXCI6XCJCaXBsZTA3OEludGVyZmFjZUJpZFJlZmVyZW50aWFsQ2FuU2VuZE5vdGlmaWNhdGlvblwifSx7XCJJRFwiOjE1NSxcIk5hbWVcIjpcIkJpcGxlMDc5SW50ZXJmYWNlQ01JQ2FuU3RhcnRNYXJpV29ya2Zsb3dcIn0se1wiSURcIjoxNTQsXCJOYW1lXCI6XCJCaXBsZTA4MEludGVyZmFjZUNNSUNhblN0YXJ0UGljYXNzb1dvcmtmbG93XCJ9LHtcIklEXCI6MjA5LFwiTmFtZVwiOlwiQmlwbGUwODFJbnRlcmZhY2VDb250cmFjdGluZ0NhbkdldEF2YWlsYWJpbGl0eVBsYW5zXCJ9LHtcIklEXCI6MjEwLFwiTmFtZVwiOlwiQmlwbGUwODJJbnRlcmZhY2VDb250cmFjdGluZ0NhbkdldEF2YWlsYWJsZUNvb3JkaW5hYmxlRGVsaXZlcnlQb2ludHNcIn0se1wiSURcIjoxNjgsXCJOYW1lXCI6XCJCaXBsZTA4M0ludGVyZmFjZUNvbnRyYWN0aW5nQ2FuR2V0Q29tcGFueVwifSx7XCJJRFwiOjE2OSxcIk5hbWVcIjpcIkJpcGxlMDg0SW50ZXJmYWNlQ29udHJhY3RpbmdDYW5HZXRDb21wYW55Rm9yRXh0ZXJuYWxcIn0se1wiSURcIjoxNzAsXCJOYW1lXCI6XCJCaXBsZTA4NUludGVyZmFjZUNvbnRyYWN0aW5nQ2FuR2V0Q29tcGFueU5hbWVcIn0se1wiSURcIjoxNjYsXCJOYW1lXCI6XCJCaXBsZTA4NkludGVyZmFjZUNvbnRyYWN0aW5nQ2FuR2V0Q29udHJhY3R1YWxEYXRhXCJ9LHtcIklEXCI6MTU4LFwiTmFtZVwiOlwiQmlwbGUwODdJbnRlcmZhY2VDb250cmFjdGluZ0NhbkdldE9ibGlnYXRpb25zXCJ9LHtcIklEXCI6MTU2LFwiTmFtZVwiOlwiQmlwbGUwODhJbnRlcmZhY2VDb250cmFjdGluZ0NhbkdldFBhcmFtZXRlcnNcIn0se1wiSURcIjoxNTksXCJOYW1lXCI6XCJCaXBsZTA4OUludGVyZmFjZUNvbnRyYWN0aW5nQ2FuR2V0VmFsaWREZWxpdmVyeVBvaW50c0J5RWxlY3RyaWNhbFpvbmVcIn0se1wiSURcIjoxNjIsXCJOYW1lXCI6XCJCaXBsZTA5MEludGVyZmFjZUNvbnRyYWN0aW5nQ2FuSW1wb3J0Q29udHJhY3R1YWxEYXRhQUZSUlwifSx7XCJJRFwiOjE2NyxcIk5hbWVcIjpcIkJpcGxlMDkxSW50ZXJmYWNlQ29udHJhY3RpbmdDYW5JbXBvcnRDb250cmFjdHVhbERhdGFBRlJSRnJvbUNDTGVnYWN5XCJ9LHtcIklEXCI6MTY1LFwiTmFtZVwiOlwiQmlwbGUwOTJJbnRlcmZhY2VDb250cmFjdGluZ0NhbkltcG9ydENvbnRyYWN0dWFsRGF0YUVsZWN0cmljYWxab25lc1wifSx7XCJJRFwiOjE2MCxcIk5hbWVcIjpcIkJpcGxlMDkzSW50ZXJmYWNlQ29udHJhY3RpbmdDYW5JbXBvcnRDb250cmFjdHVhbERhdGFNRlJSXCJ9LHtcIklEXCI6MTYxLFwiTmFtZVwiOlwiQmlwbGUwOTRJbnRlcmZhY2VDb250cmFjdGluZ0NhbkltcG9ydENvbnRyYWN0dWFsRGF0YVJlZGlzcGF0Y2hpbmdcIn0se1wiSURcIjoxNjQsXCJOYW1lXCI6XCJCaXBsZTA5NUludGVyZmFjZUNvbnRyYWN0aW5nQ2FuSW1wb3J0Q29udHJhY3R1YWxEYXRhVGVjaG5pY2FsRmFjaWxpdGllc1wifSx7XCJJRFwiOjE2MyxcIk5hbWVcIjpcIkJpcGxlMDk2SW50ZXJmYWNlQ29udHJhY3RpbmdDYW5JbXBvcnRDb250cmFjdHVhbERhdGFUZWNobmljYWxVbml0c1wifSx7XCJJRFwiOjE1NyxcIk5hbWVcIjpcIkJpcGxlMDk3SW50ZXJmYWNlQ29udHJhY3RpbmdDYW5JbXBvcnRPYmxpZ2F0aW9uVHJhbnNmZXJzXCJ9LHtcIklEXCI6MTcxLFwiTmFtZVwiOlwiQmlwbGUwOThJbnRlcmZhY2VNb25pdG9yaW5nQ2FuUnVuQ2FwYWNpdHlNb25pdG9yaW5nXCJ9LHtcIklEXCI6MTgyLFwiTmFtZVwiOlwiQmlwbGUwOTlJbnRlcmZhY2VUZXN0Q2FuRmx1c2hDYWNoZVwifSx7XCJJRFwiOjE4NixcIk5hbWVcIjpcIkJpcGxlMTAwSW50ZXJmYWNlVGVzdENhbkdldFRva2VuXCJ9LHtcIklEXCI6MTgwLFwiTmFtZVwiOlwiQmlwbGUxMDFJbnRlcmZhY2VUZXN0Q2FuUHVzaEJhY2t1cERlbGl2ZXJ5UG9pbnRzU3VibWl0dGVkTWVzc2FnZVwifSx7XCJJRFwiOjE4MSxcIk5hbWVcIjpcIkJpcGxlMTAySW50ZXJmYWNlVGVzdENhblB1c2hCYWNrdXBEZWxpdmVyeVBvaW50c1N1Ym1pdHRlZE1lc3NhZ2VGcm9tRmlsZVwifSx7XCJJRFwiOjE3NixcIk5hbWVcIjpcIkJpcGxlMTAzSW50ZXJmYWNlVGVzdENhblB1c2hCaWRDb25maXJtYXRpb25NZXNzYWdlXCJ9LHtcIklEXCI6MTc3LFwiTmFtZVwiOlwiQmlwbGUxMDRJbnRlcmZhY2VUZXN0Q2FuUHVzaEJpZENvbmZpcm1hdGlvbk1lc3NhZ2VGcm9tRmlsZVwifSx7XCJJRFwiOjE3OCxcIk5hbWVcIjpcIkJpcGxlMTA1SW50ZXJmYWNlVGVzdENhblB1c2hCaWRTdWJtaXR0ZWRNZXNzYWdlXCJ9LHtcIklEXCI6MTc5LFwiTmFtZVwiOlwiQmlwbGUxMDZJbnRlcmZhY2VUZXN0Q2FuUHVzaEJpZFN1Ym1pdHRlZE1lc3NhZ2VGcm9tRmlsZVwifSx7XCJJRFwiOjE4NSxcIk5hbWVcIjpcIkJpcGxlMTA3SW50ZXJmYWNlVGVzdENhblB1c2hDb250cmFjdGVkUG93ZXJVcGRhdGVkTWVzc2FnZVwifSx7XCJJRFwiOjE4MyxcIk5hbWVcIjpcIkJpcGxlMTA4SW50ZXJmYWNlVGVzdENhblB1c2hDb250cmFjdHVhbERhdGFVcGRhdGVkTWVzc2FnZVwifSx7XCJJRFwiOjE4NCxcIk5hbWVcIjpcIkJpcGxlMTA5SW50ZXJmYWNlVGVzdENhblB1c2hDUklMZXZlbFVwZGF0ZWRNZXNzYWdlXCJ9LHtcIklEXCI6MjEyLFwiTmFtZVwiOlwiQmlwbGUxMTBJbnRlcmZhY2VUZXN0Q2FuRmx1c2hUb2tlbkNhY2hlXCJ9LHtcIklEXCI6MjEzLFwiTmFtZVwiOlwiQmlwbGUxMTFJbnRlcmZhY2VDb250cmFjdGluZ0NhbkdldERlbGl2ZXJ5UG9pbnRzQnlFYW5cIn0se1wiSURcIjoyMTQsXCJOYW1lXCI6XCJCaXBsZTExMkludGVyZmFjZVJlcG9ydGluZ0NhbkdlbmVyYXRlUmVwb3J0RmlsZVwifSx7XCJJRFwiOjIxNSxcIk5hbWVcIjpcIkJpcGxlMTEzSW50ZXJmYWNlQ29udHJhY3RpbmdDYW5HZXRBbGxDb21wYW5pZXNcIn0se1wiSURcIjoyMTgsXCJOYW1lXCI6XCJCaXBsZTExNEludGVyZmFjZUNvbnRyYWN0aW5nQ2FuR2V0Q29udHJhY3RzV2l0aEF2YWlsYWJsZURlbGl2ZXJ5UG9pbnRzXCJ9LHtcIklEXCI6MjE5LFwiTmFtZVwiOlwiQmlwbGUxMTVJbnRlcmZhY2VDb250cmFjdGluZ0NhbkdldFRlY2huaWNhbEZhY2lsaXRpZXNcIn0se1wiSURcIjoyMjAsXCJOYW1lXCI6XCJCaXBsZTExNkludGVyZmFjZU1vbml0b3JpbmdDYW5SdW5DaGVja0luY29oZXJlbmN5XCJ9LHtcIklEXCI6MjIxLFwiTmFtZVwiOlwiQmlwbGUxMTdJbnRlcmZhY2VNb25pdG9yaW5nQ2FuUnVuQ2hlY2tOb21pbmF0aW9uc1wifSx7XCJJRFwiOjIyMixcIk5hbWVcIjpcIkJpcGxlMTE4SW50ZXJmYWNlQmlkUmVmZXJlbnRpYWxDYW5HZXRVbmF2YWlsYWJpbGl0aWVzXCJ9LHtcIklEXCI6MjIzLFwiTmFtZVwiOlwiQmlwbGUxMTlJbnRlcmZhY2VCaWRSZWZlcmVudGlhbENhblNhdmVVbmF2YWlsYWJpbGl0aWVzXCJ9LHtcIklEXCI6MTQxLFwiTmFtZVwiOlwiQmlwbGUxMjBNZXNzYWdlTG9nQ2FuR2V0QWNjZXB0ZWRCaWREZXRhaWxzU3BlY2lmaWNWZXJzaW9uXCJ9LHtcIklEXCI6MTQyLFwiTmFtZVwiOlwiQmlwbGUxMjFNZXNzYWdlTG9nQ2FuR2V0QWNjZXB0ZWRCaWREZXRhaWxzU3BlY2lmaWNWZXJzaW9uQXNNYXJrZXRQYXJ0eVwifSx7XCJJRFwiOjEzOSxcIk5hbWVcIjpcIkJpcGxlMTIyTWVzc2FnZUxvZ0NhbkdldE1lc3NhZ2VEZXRhaWxzXCJ9LHtcIklEXCI6MTQwLFwiTmFtZVwiOlwiQmlwbGUxMjNNZXNzYWdlTG9nQ2FuR2V0TWVzc2FnZURldGFpbHNBc01hcmtldFBhcnR5XCJ9LHtcIklEXCI6MTQzLFwiTmFtZVwiOlwiQmlwbGUxMzBNb25pdG9yaW5nQ2FuR2V0Q2FwYWNpdHlNb25pdG9yaW5nXCJ9LHtcIklEXCI6MTQ0LFwiTmFtZVwiOlwiQmlwbGUxMzFNb25pdG9yaW5nQ2FuR2V0Q2FwYWNpdHlNb25pdG9yaW5nQXNNYXJrZXRQYXJ0eVwifSx7XCJJRFwiOjIwMixcIk5hbWVcIjpcIkJpcGxlMTQwVUlDYW5BY2Nlc3NBcHBsaWNhdGlvblwifSx7XCJJRFwiOjE5NyxcIk5hbWVcIjpcIkJpcGxlMTQxVUlDYW5FZGl0Qmlkc1wifSx7XCJJRFwiOjE5OCxcIk5hbWVcIjpcIkJpcGxlMTQyVUlDYW5FZGl0UGFyYW1ldGVyc1wifSx7XCJJRFwiOjE4OSxcIk5hbWVcIjpcIkJpcGxlMTQzVUlDYW5TZWVCYWNrdXBEZWxpdmVyeVBvaW50c1NjcmVlblwifSx7XCJJRFwiOjE4OCxcIk5hbWVcIjpcIkJpcGxlMTQ0VUlDYW5TZWVCaWRSZWZlcmVudGlhbFNjcmVlblwifSx7XCJJRFwiOjIwMSxcIk5hbWVcIjpcIkJpcGxlMTQ1VUlDYW5TZWVEYXNoYm9hcmRDUklGaWx0ZXJlZFZvbHVtZVwifSx7XCJJRFwiOjIwMCxcIk5hbWVcIjpcIkJpcGxlMTQ2VUlDYW5TZWVEYXNoYm9hcmRFbmVyZ3lCaWRWb2x1bWVcIn0se1wiSURcIjoxOTksXCJOYW1lXCI6XCJCaXBsZTE0N1VJQ2FuU2VlRGFzaGJvYXJkT2JsaWdhdGlvbk1vbml0b3JpbmdcIn0se1wiSURcIjoxODcsXCJOYW1lXCI6XCJCaXBsZTE0OFVJQ2FuU2VlRGFzaGJvYXJkU2NyZWVuXCJ9LHtcIklEXCI6MTk1LFwiTmFtZVwiOlwiQmlwbGUxNDlVSUNhblNlZUhlbHBNZW51XCJ9LHtcIklEXCI6MTk2LFwiTmFtZVwiOlwiQmlwbGUxNTBVSUNhblNlZU1hcmtldFBhcnR5TGlzdFwifSx7XCJJRFwiOjE5MCxcIk5hbWVcIjpcIkJpcGxlMTUxVUlDYW5TZWVNZXNzYWdlTG9nU2NyZWVuXCJ9LHtcIklEXCI6MTkyLFwiTmFtZVwiOlwiQmlwbGUxNTJVSUNhblNlZVBhcmFtZXRlcnNTY3JlZW5cIn0se1wiSURcIjoxOTMsXCJOYW1lXCI6XCJCaXBsZTE1M1VJQ2FuU2VlVXBsb2FkUHJlcXVhbGlmaWNhdGlvbkJpZFNjcmVlblwifSx7XCJJRFwiOjE5NCxcIk5hbWVcIjpcIkJpcGxlMTU0VUlDYW5TZWVVcGxvYWRab25lU2NyZWVuXCJ9LHtcIklEXCI6MTkxLFwiTmFtZVwiOlwiQmlwbGUxNTVVSUNhblNlZVpvbmVTY3JlZW5cIn0se1wiSURcIjoyMDcsXCJOYW1lXCI6XCJCaXBsZTE1NlVJTG9nZ2VyQ2FuUG9zdEVycm9yXCJ9LHtcIklEXCI6MjE3LFwiTmFtZVwiOlwiQmlwbGUxNTdVSUNhblNlZVJlcG9ydGluZ0V4cG9ydFwifSx7XCJJRFwiOjIyNCxcIk5hbWVcIjpcIkJpcGxlMTU4SW50ZXJmYWNlSW50ZXJuYWxCaWRSZWZlcmVudGlhbENhbkZpbmRSZXF1ZXN0ZWRCaWRzXCJ9LHtcIklEXCI6MjI1LFwiTmFtZVwiOlwiQmlwbGUxNTlJbnRlcmZhY2VCaWRSZWZlcmVudGlhbENhbkdldExhc3RVcGRhdGVkVGltZUJpZHNcIn0se1wiSURcIjoyMjYsXCJOYW1lXCI6XCJCaXBsZTE2MFVJQ2FuQ29uZmlybVVwbG9hZFwifSx7XCJJRFwiOjIyNyxcIk5hbWVcIjpcIkJpcGxlMTYxQ2FuU2tpcFVzZXJDaGVja1wifSx7XCJJRFwiOjM0LFwiTmFtZVwiOlwiQ2FuRXhlY3V0ZVRlc3RNaWNyb3NlcnZpY2VcIn0se1wiSURcIjozMixcIk5hbWVcIjpcIkNhbk1hbmFnZVNlcnZlckxvZ2dpbmdMZXZlbFwifSx7XCJJRFwiOjMzLFwiTmFtZVwiOlwiQ2FuU2VlSm9ic1wifSx7XCJJRFwiOjMxLFwiTmFtZVwiOlwiQ2FuU2VlU3dhZ2dlckZhY2FkZUFwaVwifSx7XCJJRFwiOjIzLFwiTmFtZVwiOlwiQ2hlY2tGb3JJbmNvcnJlY3ROb21pbmF0aW9uXCJ9LHtcIklEXCI6OCxcIk5hbWVcIjpcIkNyZWF0ZUJpZEV4dFwifSx7XCJJRFwiOjIyLFwiTmFtZVwiOlwiRG93bmxvYWRSZXBvcnRcIn0se1wiSURcIjoyOCxcIk5hbWVcIjpcIkZjcjIwMjBSZXBvcnRFeHRcIn0se1wiSURcIjoyNyxcIk5hbWVcIjpcIkZjcjIwMjBSZXBvcnRJbnRcIn0se1wiSURcIjoxOSxcIk5hbWVcIjpcIkZDUlJlcG9ydEV4dFwifSx7XCJJRFwiOjE4LFwiTmFtZVwiOlwiRkNSUmVwb3J0SW50XCJ9LHtcIklEXCI6MTIsXCJOYW1lXCI6XCJMb2dzSW50XCJ9LHtcIklEXCI6MjUsXCJOYW1lXCI6XCJtRlJSUmVwb3J0RXh0XCJ9LHtcIklEXCI6MjYsXCJOYW1lXCI6XCJtRlJSUmVwb3J0SW50XCJ9LHtcIklEXCI6MTcsXCJOYW1lXCI6XCJSMU5vbWluYXRpb25zRXh0XCJ9LHtcIklEXCI6MjQsXCJOYW1lXCI6XCJSMk92ZXJ2aWV3RXh0XCJ9LHtcIklEXCI6MjEsXCJOYW1lXCI6XCJSM05vbkNpcHVSZXBvcnRFeHRcIn0se1wiSURcIjoyMCxcIk5hbWVcIjpcIlIzTm9uQ2lwdVJlcG9ydEludFwifSx7XCJJRFwiOjEzLFwiTmFtZVwiOlwiU2hvd0hlbHBcIn0se1wiSURcIjoxNSxcIk5hbWVcIjpcIlNob3dVc2VyTWFudWFsRXh0XCJ9LHtcIklEXCI6MTQsXCJOYW1lXCI6XCJTaG93VXNlck1hbnVhbEludFwifSx7XCJJRFwiOjksXCJOYW1lXCI6XCJVcGRhdGVCaWRFeHRcIn0se1wiSURcIjo0LFwiTmFtZVwiOlwiVXBkYXRlU2V0dGluZ3NcIn0se1wiSURcIjo3LFwiTmFtZVwiOlwiVmlld0NvbnRyYWN0dWFsRGF0YVwifSx7XCJJRFwiOjMsXCJOYW1lXCI6XCJWaWV3U2V0dGluZ3NcIn1dLFwiT3BlcmF0aW9uc1wiOlt7XCJPcGVyYXRpb25zXCI6WzEsMyw0LDUsNywxMCwxMiwxMywxNCwxNiwxOCwyMCwyMiwyNiwyNywzMCwzMSwzMiwzMywzNCwxMDAsMTAyLDEwNCwxMDUsMTA2LDEwNywxMDgsMTA5LDExMSwxMTMsMTE3LDExOCwxMTksMTIxLDEyMywxMjQsMTI3LDEyOSwxMzIsMTMzLDEzNSwxMzYsMTM5LDE0MSwxNDMsMTQ1LDE0NiwxNDcsMTQ4LDE0OSwxNTAsMTUxLDE1MiwxNTMsMTU0LDE1NSwxNTYsMTU3LDE1OCwxNTksMTYwLDE2MSwxNjIsMTYzLDE2NCwxNjUsMTY2LDE2NywxNjgsMTY5LDE3MCwxNzEsMTcyLDE3MywxNzUsMTc2LDE3NywxNzgsMTc5LDE4MCwxODEsMTgyLDE4MywxODQsMTg1LDE4NiwxODcsMTg4LDE4OSwxOTAsMTkxLDE5MiwxOTMsMTk0LDE5NSwxOTYsMTk3LDE5OCwxOTksMjAwLDIwMSwyMDIsMjAzLDIwNCwyMDYsMjA3LDIwOCwyMDksMjEwLDIxMSwyMTIsMjEzLDIxNCwyMTUsMjE2LDIxNywyMTgsMjE5LDIyMCwyMjEsMjIyLDIyMywyMjQsMjI1XSxcIlNjb3BlXCI6XCJcIn1dLFwiUm9sZXNcIjpbe1wiUm9sZXNcIjpbXCJBZG1pblwiLFwiQmlwbGVBZG1pblwiXSxcIlNjb3BlXCI6XCJcIn1dLFwiU2NvcGVzXCI6W1wiXCJdfSIsImh0dHA6Ly9zY2hlbWFzLmFyYzR1Lm5ldC93cy8yMDEyLzA1L2lkZW50aXR5L2NsYWltcy9zZXJ2aWNlc2lkIjoiUy0xLTUtMjEtMjEzNzE0Mzk1OS0xODc2NjkzNTA2LTI1ODAzOTg1NjMtODA3NTIiLCJhcHB0eXBlIjoiUHVibGljIiwiYXBwaWQiOiJhYjFmMjMwYS1hOTI3LTQ3NjgtYjM4ZC1lNzZkNzU0MTc4M2YiLCJhdXRobWV0aG9kIjoiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2F1dGhlbnRpY2F0aW9ubWV0aG9kL3dpbmRvd3MiLCJhdXRoX3RpbWUiOiIyMDIyLTEwLTE4VDA2OjMxOjEzLjgwNFoiLCJ2ZXIiOiIxLjAiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24gb3BlbmlkIn0.ILS3bFXMCPoXHzQGNHyq4bpC0Z4cAQUiZPyG-ozS_eI8pyVSXEOUEH61rCz79_SWtvJPVgsFNjZUNLI0zwp95jshZXBkjfd149NTcuRJvU59gkOat7i6ol7mZ1I694n8qBctkn5p0ZaqxSyCaCq7jnjjMnKshDFwgbUqOfNPIWffi1_xsnXWDxhc3xfuhMh_Xyjg3sd9274lRQ1AF2uFy93VIXaKpCSMhrDLCJcj0Q16dRQDhckwF4pNv3kmbgZVoFFlsF_ihhgJ-0osuPZQcSJgnYhNmRA6fY-jqJ-bw940XI4Zzt5hkjE7Ps89z20zxJ9DkINh3AQWp3Fp4aWz5Q",
            "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkVBYURSMHRZVU5OWWZCSmotRHpucnNyakxyRSIsImtpZCI6IkVBYURSMHRZVU5OWWZCSmotRHpucnNyakxyRSJ9.eyJhdWQiOiJodHRwczovL3BydC50ZXN0Lmhvc3QiLCJpc3MiOiJodHRwOi8vYWRmc3Rlc3QuYmVsZ3JpZC5uZXQvYWRmcy9zZXJ2aWNlcy90cnVzdCIsImlhdCI6MTY2NjA3NDgzNiwibmJmIjoxNjY2MDc0ODM2LCJleHAiOjE2NjYxMTA4MzYsImh0dHA6Ly9zY2hlbWFzLmFyYzR1Lm5ldC93cy8yMDEyLzA1L2lkZW50aXR5L2NsYWltcy9jdWx0dXJlIjoibmwtTkwiLCJnaXZlbl9uYW1lIjoiVmluY2VudCIsImZhbWlseV9uYW1lIjoiVmFuIERlbiBCZXJnaGUiLCJlbWFpbCI6IlZpbmNlbnQuVmFuRGVuQmVyZ2hlQGVsaWEuYmUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9ob21lcGhvbmUiOiIiLCJodHRwOi8vc2NoZW1hcy5hcmM0dS5uZXQvd3MvMjAxMi8wNS9pZGVudGl0eS9jbGFpbXMvY29tcGFueSI6IkVsaWEiLCJodHRwOi8vc2NoZW1hcy5hcmM0dS5uZXQvd3MvMjAxMi8wNS9pZGVudGl0eS9jbGFpbXMvc2lkIjoiUy0xLTktMjEtMjEzNzE0Mzk1OS0xODc2NjkzNTA2LTI1ODAzOTg1NjMtODMxODQiLCJ1cG4iOiJWVjAwMDZAQmVsZ3JpZC5uZXQiLCJ3aW5hY2NvdW50bmFtZSI6IlZWMDAwNiIsInByaW1hcnlzaWQiOiJTLTEtNS0yMS0yMTM3MTQzOTU5LTE4NzY2OTM1MDYtMjU4MDM5ODU2My04MzE4NCIsImh0dHA6Ly9zY2hlbWFzLmFyYzR1Lm5ldC93cy8yMDEyLzA1L2lkZW50aXR5L2NsYWltcy9hdXRob3JpemF0aW9uIjoie1wiQWxsT3BlcmF0aW9uc1wiOlt7XCJJRFwiOjEsXCJOYW1lXCI6XCJBY2Nlc3NBcHBsaWNhdGlvblwifSx7XCJJRFwiOjIsXCJOYW1lXCI6XCJDYW5NYW5hZ2VTZXJ2ZXJMb2dnaW5nTGV2ZWxcIn0se1wiSURcIjozLFwiTmFtZVwiOlwiQ2FuU2VlU3dhZ2dlckZhY2FkZUFwaVwifSx7XCJJRFwiOjcsXCJOYW1lXCI6XCJEZWFjdGl2YXRlUGFyY2VsXCJ9LHtcIklEXCI6OCxcIk5hbWVcIjpcIkRlbGV0ZVBhcmNlbFwifSx7XCJJRFwiOjUsXCJOYW1lXCI6XCJFeHBvcnRQYXJjZWxEYXRhXCJ9LHtcIklEXCI6NixcIk5hbWVcIjpcIkltcG9ydENhZGFzdGVySW5mb1wifSx7XCJJRFwiOjQsXCJOYW1lXCI6XCJNYW5hZ2VQYXJjZWxcIn1dLFwiT3BlcmF0aW9uc1wiOlt7XCJPcGVyYXRpb25zXCI6WzEsMiwzLDQsNSw2LDcsOF0sXCJTY29wZVwiOlwiXCJ9XSxcIlJvbGVzXCI6W3tcIlJvbGVzXCI6W1wiQWRtaW5cIl0sXCJTY29wZVwiOlwiXCJ9XSxcIlNjb3Blc1wiOltcIlwiXX0iLCJodHRwOi8vc2NoZW1hcy5hcmM0dS5uZXQvd3MvMjAxMi8wNS9pZGVudGl0eS9jbGFpbXMvc2VydmljZXNpZCI6IlMtMS01LTIxLTIxMzcxNDM5NTktMTg3NjY5MzUwNi0yNTgwMzk4NTYzLTEwNTM0NiIsImFwcHR5cGUiOiJQdWJsaWMiLCJhcHBpZCI6ImI3NGI5NTI5LWQ4NmEtNGNhYS1hYTExLTEyYzM3NGEyYWFmYSIsImF1dGhtZXRob2QiOiJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvYXV0aGVudGljYXRpb25tZXRob2Qvd2luZG93cyIsImF1dGhfdGltZSI6IjIwMjItMTAtMThUMDY6MzM6NTYuMTU5WiIsInZlciI6IjEuMCIsInNjcCI6InVzZXJfaW1wZXJzb25hdGlvbiBvcGVuaWQifQ.LHBdN1W5i3GZlEwEudIpa36IsgE_5P8VYGcsFSWuQdntuEVwhmujyH8tGogXW_kzEFHZFfdzspNY4umiBEtUp5m5u9ZULT1mPcaHywsVM0337EApFdwzFbRn1JFwNc2gMFxOiJJEadBMveGR7LWATvYNGlB0B9RFXka28pfiHU9KEbvXEXv6MfGIYNNQ60rW-ekTkE7M-jEQ7rb7VeE_VlaXfFLtAbuaeqB19tji2M6qsDYqMDhlVlMAZxzMDxCvRp6xiZoLhJOoadwm2X9mcY-bKrgVU-K2Eczmqzi1FPFR4ss9Y6WNOD_QdawWC57g372ezVgI8cl_vHgRY15ZCw"
        };

        private static readonly JwtSecurityToken _jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

        private static readonly TokenInfo _tokenInfo = new TokenInfo("Bearer", _jwt.EncodedPayload, _jwt.EncodedHeader, DateTime.UtcNow);

        private sealed class Measurement
        {
            public string Method { get; set; }
            public TimeSpan TimeSpan { get; set; } 
            public int Size { get; set; }
        }

        private readonly ITestOutputHelper _output;

        public CompressionAndSpeedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static Measurement Measure(IObjectSerialization objectSerialization, string method)
        {
            var sw = Stopwatch.StartNew();
            int size = 0;
            for (int iterations = 0; iterations < 10000; ++iterations)
            {
                size = 0;

                foreach (var bearerToken in _bearerTokens)
                {
                    var data = objectSerialization.Serialize(bearerToken);
                    size += data.Length;
                    // this measures time only, it's not a test of correctness in this context.
                    objectSerialization.Deserialize<string>(data);                    
                }

                {
                    var data = objectSerialization.Serialize(_tokenInfo);
                    size += data.Length;
                    objectSerialization.Deserialize<TokenInfo>(data);
                }

                // add other relevant cases here.
            }
            sw.Stop();
            return new Measurement { Method = method, TimeSpan = sw.Elapsed, Size = size };
        }

        private void ShowMeasurements(IEnumerable<Measurement> measurements, string title)
        {
            _output.WriteLine(title);
            _output.WriteLine("Method\tTime\tSize");
            foreach (var measurement in measurements)
                _output.WriteLine($"{measurement.Method}\t{measurement.TimeSpan}\t{measurement.Size}");
            _output.WriteLine("");
        }

        /// <summary>
        /// Measure response 
        /// </summary>
        [Fact]
        public void CheckRuntimeTypeModelConcurrencyAsync()
        {
            // arrange
            var protoBufSerialization = new ProtoBufSerialization();
            var protoBufZipSerialization = new ProtoBufZipSerialization();
            var oldJsonSerialization = new JsonSerialization();
            var newJsonSerialization = new JsonSerialization2();
            var jsonCompressedSerialization = new JsonSerialization2(compressed: true);

            // act
            var list = new List<Measurement>();
            list.Add(Measure(protoBufSerialization, "Protobuf"));
            list.Add(Measure(protoBufZipSerialization, "ProtobufZip"));
            list.Add(Measure(oldJsonSerialization, "JsonSerialization"));
            list.Add(Measure(newJsonSerialization, "JsonSerialization2(compressed : false)"));
            list.Add(Measure(jsonCompressedSerialization, "JsonSerialization2(compressed: true)"));

            // sort by fastest compression
            list.Sort((item1, item2) => item1.TimeSpan.CompareTo(item2.TimeSpan));
            ShowMeasurements(list, "Results ordered by ascending serialization speed");
            // sort by smallest size
            list.Sort((item1, item2) => item1.Size.CompareTo(item2.Size));
            ShowMeasurements(list, "Results ordered by ascending serialization size");
        }
    }
}
