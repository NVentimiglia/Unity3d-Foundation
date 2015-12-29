using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Foundation.Server.Api;

namespace Foundation.Server.Controllers
{
    /// <summary>
    /// Example custom model controller. A simple high scores controller
    /// </summary>
    [Authorize]
    [Route("api/GameScores")]
    public class GameScoresController : ApiControllerBase
    {
        [HttpPost]
        [Route("api/Scores/Get")]
        [AllowAnonymous]
        public async Task<IEnumerable<GameScore>> Get(int take = 10, int skip = 0)
        {
            // Select Data
            return await AppDatabase.Scores.OrderByDescending(o => o.Score).Skip(skip).Take(take).ToArrayAsync();
        }


        [HttpPost]
        [Route("api/Scores/Self")]
        public async Task<IHttpActionResult> Self()
        {
            // Select Data
            var model = await AppDatabase.Scores.OrderByDescending(o => o.Score).FirstOrDefaultAsync(o => o.UserId == UserId);
            return Ok(model);
        }

        [HttpPost]
        [Route("api/Scores/Update")]
        public async Task<IHttpActionResult> Update(GameScore score)
        {
            //Cleanup. This should be a background job.
            var badRange = AppDatabase.Scores.Where(o => o.CreatedOn < DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0)));
            AppDatabase.Scores.RemoveRange(badRange);
            await AppDatabase.SaveChangesAsync();

            // Select old
            var model = await AppDatabase.Scores.OrderByDescending(o => o.Score).FirstOrDefaultAsync(o => o.UserId == UserId);


            //Update ?
            if (model != null )
            {
                //Skip if less than
                if (model.Score > score.Score)
                {
                    return Ok();
                }

                model.Score = score.Score;
            }
            else
            {
                //Create
                AppDatabase.Scores.Add(score);
            }

            await AppDatabase.SaveChangesAsync();

            return Ok();
        }
    }
}
