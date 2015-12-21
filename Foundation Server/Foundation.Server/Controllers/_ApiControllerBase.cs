using System;
using System.Web.Http;
using Elmah.Contrib.WebApi;
using Foundation.Server.Infrastructure;
using Foundation.Server.Infrastructure.Filters;
using Foundation.Server.Infrastructure.Mailers;
using Foundation.Server.Models;

namespace Foundation.Server.Controllers
{
    [AuthorizeApplicationId]
    [ApiHandleError]
    [ApiBadRequest]
    [ElmahHandleErrorApi]
    public class ApiControllerBase : ApiController
    {
        private AppDatabase _appDatabase;
        /// <summary>
        /// Application Database
        /// </summary>
        public AppDatabase AppDatabase
        {
            get
            {
                if (_appDatabase == null)
                    _appDatabase = new AppDatabase();
                return _appDatabase;
            }
            set { _appDatabase = value; }
        }

        private AppSession _appSession;
        /// <summary>
        /// Application state passed via encrypted header.
        /// Use for un-synced user session.
        /// </summary>
        public AppSession Session
        {
            get
            {
                if (_appSession == null)
                    _appSession = AppSession.GetSession();
                return _appSession;
            }
        }

        private AppAuthorization _appAuthorization;
        /// <summary>
        /// Application state passed via encrypted header.
        /// Use for un-synced user session.
        /// </summary>
        public AppAuthorization Authorization
        {
            get
            {
                if (_appAuthorization == null)
                    _appAuthorization = AppAuthorization.GetSession();
                return _appAuthorization;
            }
        }

        private Mailer _mailer;
        /// <summary>
        /// Application Email System
        /// </summary>
        public Mailer Mailer
        {
            get
            {
                if (_mailer == null)
                    _mailer = new Mailer();
                return _mailer;
            }
            set { _mailer = value; }
        }

        /// <summary>
        /// Session.UserId
        /// </summary>
        public string UserId
        {
            get { return Session.UserId; }
        }

        /// <summary>
        /// Session.IsAuthenticated
        /// </summary>
        public bool IsAuthenticated
        {
            get { return Authorization.IsAuthenticated; }
        }
        

        public virtual new FoundationPrincipal User
        {
            get { return base.User as FoundationPrincipal; }
        }
        
        /// <summary>
        /// Saves changes to session, Disposes DB
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Save auth
                if (_appSession != null && _appAuthorization.IsAuthenticated)
                    Authorization.SetResponse();

                //Save session
                if (_appSession != null)
                    Session.SetResponse();

                //Dispose db
                if (_appDatabase != null)
                    _appDatabase.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}