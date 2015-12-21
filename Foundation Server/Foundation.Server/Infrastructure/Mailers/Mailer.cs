using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foundation.Server.Infrastructure.Mailers
{
    /// <summary>
    /// Class for sending Razor Email
    /// </summary>
    public class Mailer
    {
        public static string AppName { get; private set; }
        public static string NoReplyEmail { get; private set; }
        
        public static MailAddress Sender { get; private set; }
        protected SmtpClient Client = new SmtpClient();
        protected ControllerContext Context;

        protected TempDataDictionary TempData
        {
            get { return Context.Controller.TempData; }
        }

        protected dynamic ViewBag
        {
            get { return Context.Controller.ViewBag; }
        }

        protected ViewDataDictionary ViewData
        {
            get { return Context.Controller.ViewData; }
        }

        static Mailer()
        {
            AppName = AppConfig.Settings.AppName;
            NoReplyEmail = AppConfig.Settings.NoReplyEmail;


            Sender = new MailAddress(NoReplyEmail, AppName);
        }

        public Mailer()
        {
            Context = ControllerExtensions.CreateController<MailerController>().ControllerContext;
        }

        protected void RaiseError(Exception ex)
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        }

        /// <summary>
        /// Sends The Email !
        /// </summary>
        /// <param name="viewPath"> Virtual Path to View : ~\Views\Mailer\Welcome</param>
        /// <param name="model">object</param>
        /// <param name="subject">Title</param>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public Task SendAsync(string viewPath, object model, string subject, MailAddress recipients)
        {
            return SendAsync(viewPath, model, subject, null, recipients);
        }


        /// <summary>
        /// Sends The Email !
        /// </summary>
        /// <param name="viewPath"> Virtual Path to View : ~\Views\Mailer\Welcome</param>
        /// <param name="model">object</param>
        /// <param name="subject">Title</param>
        /// <param name="replyTo"></param>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public async Task SendAsync(string viewPath, object model, string subject, MailAddress replyTo, MailAddress recipients)
        {
            if (AppConfig.Settings.DisableEmail)
                return;

            var mail = new MailMessage
            {
                Subject = subject,
                From = Sender,
                IsBodyHtml = true,
                Body = ToHtml(viewPath, model)
            };

            if (replyTo != null)
            {
                mail.ReplyToList.Add(replyTo);
            }

            mail.To.Add(recipients);

            await Client.SendMailAsync(mail);
        }



        /// <summary>
        /// Generates HTML Body
        /// </summary>
        /// <param name="viewPath"> Virtual Path to View : ~\Views\Mailer\Welcome</param>
        /// <param name="model">object</param>
        /// <returns></returns>
        public string ToHtml(string viewPath, object model)
        {
            var data = new ViewDataDictionary(model);
            return ToHtml(viewPath, data);
        }


        /// <summary>
        /// Generates HTML Body
        /// </summary>
        /// <param name="viewPath"> Virtual Path to View : ~\Views\Mailer\Welcome</param>
        /// <param name="viewData">object</param>
        /// <returns></returns>
        public string ToHtml(string viewPath, ViewDataDictionary viewData)
        {
            var result = ViewEngines.Engines.FindView(Context, viewPath, null);

            StringWriter output;
            using (output = new StringWriter())
            {
                var viewContext = new ViewContext(Context, result.View, viewData, Context.Controller.TempData, output);
                result.View.Render(viewContext, output);
                result.ViewEngine.ReleaseView(Context, result.View);
            }

            return output.ToString();
        }
    }
}