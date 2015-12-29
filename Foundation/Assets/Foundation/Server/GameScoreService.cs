// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using Foundation.Server.Api;
using Foundation.Tasks;
using UnityEngine;

namespace Foundation.Server
{
    /// <summary>
    /// Example strongly typed web api score service client
    /// </summary>
    public class GameScoreService : ServiceClientBase
    {
        #region Internal

        public static readonly GameScoreService Instance = new GameScoreService();

        GameScoreService() : base("GameScore") { }

        #endregion

        #region Public Method

        /// <summary>
        /// Gets the high score list
        /// </summary>
        /// <returns></returns>
        public UnityTask<GameScore[]> Get(int take, int skip)
        {
            var dto = new Dictionary<string, int>
            {
                {"take", take},
                {"skip", skip}
            };

            return HttpPost<GameScore[]>("Get", dto);
        }

        /// <summary>
        /// Returns your current score
        /// </summary>
        /// <returns></returns>
        public UnityTask<GameScore> Self()
        {
            if (!IsAuthenticated)
                return UnityTask.FailedTask<GameScore>("Not authenticated");

            return HttpPost<GameScore>("Self");
        }

        /// <summary>
        /// Posts the score to the server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public UnityTask Update(GameScore entity)
        {
            if (!IsAuthenticated)
                return UnityTask.FailedTask<GameScore>("Not authenticated");

            return HttpPost("Update", entity);
        }

        #endregion
    }
}