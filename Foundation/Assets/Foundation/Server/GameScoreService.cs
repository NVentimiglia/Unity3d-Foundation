// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published	: 2015
//  -------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Foundation.Server.Api;
using FullSerializer;

namespace Assets.Foundation.Server
{
    /// <summary>
    /// CRUD service for untyped dynamic objects
    /// </summary>
    public class GameScoreService 
    {
        #region Internal

        public static readonly GameScoreService Instance = new GameScoreService();

        public CloudConfig Config
        {
            get { return CloudConfig.Instance; }
        }

        public AccountService AccountService
        {
            get { return AccountService.Instance; }
        }

        public readonly ServiceClient ServiceClient = new ServiceClient("GameScore");


        #endregion

        #region Public Method

        /// <summary>
        /// Gets the high score list
        /// </summary>
        /// <returns></returns>
        public HttpTask<GameScore[]> Get(int take, int skip)
        {
            if (!Config.IsValid)
                return new HttpTask<GameScore[]>(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask<GameScore[]>(new Exception("Not Authenticated"));

            var dto = new Dictionary<string, int>
            {
                {"take", take},
                { "skip", skip}
            };

            return ServiceClient.Post<GameScore[]>("Get", dto);
        }

        /// <summary>
        /// Returns your current score
        /// </summary>
        /// <returns></returns>
        public HttpTask<GameScore> Self()
        {
            if (!Config.IsValid)
                return new HttpTask<GameScore>(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask<GameScore>(new Exception("Not Authenticated"));

            return ServiceClient.Post<GameScore>("Self");
        }
        
        /// <summary>
        /// Posts the score to the server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public HttpTask Update(GameScore entity)
        {
            if (!Config.IsValid)
                return new HttpTask(new Exception("Configuration is invalid"));

            if (!AccountService.IsAuthenticated)
                return new HttpTask(new Exception("Not Authenticated"));
            
            return ServiceClient.Post("Update", entity);
        }
        
        #endregion
    }
}