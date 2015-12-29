using System;
using System.Threading.Tasks;
using System.Web.Http;
using Foundation.Server.Api;
using Foundation.Server.Infrastructure;

namespace Foundation.Server.Controllers
{
    /// <summary>
    /// Realtime Messenger Authentication
    /// </summary>
    public class ApiRealtimeController : ApiControllerBase
    {

        /// <summary>
        /// Signs the user into the Realtime Messenger service
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/Realtime/SignIn")]
        public async Task<IHttpActionResult> SignIn(RealtimeSignIn model)
        {
            if (!AppConfig.Settings.RealtimeEnabled)
                return BadRequest("Application not configured for Realtime.");

            var details = new RealtimeToken
             {
                 AuthenticationToken = Guid.NewGuid().ToString()
             };

            // ORTC
            await OrtcHelper.PostAuthentication(
                details.AuthenticationToken,
                false,
                AppConfig.Settings.RealtimeId,
                86400,
                AppConfig.Settings.RealtimeSecret,
                model.Channels);
            
            return Ok(details);
        }

    }
}