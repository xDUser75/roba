namespace Store.Web
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using Microsoft.Practices.ServiceLocation;

    using SharpArch.Core.NHibernateValidator.ValidatorProvider;
    using SharpArch.Data.NHibernate;
    using SharpArch.Web.Areas;
    using SharpArch.Web.Castle;
    using SharpArch.Web.ModelBinder;
    using SharpArch.Web.NHibernate;

    using Store.Data.NHibernateMaps;
    using Store.Web.CastleWindsor;
    using Store.Web.Controllers;

    using System.Configuration;
    using Quartz;
    using Quartz.Impl;
    using System.Web.Security;
    using System.Xml;
    using System.IO;

    //// Note: For instructions on enabling IIS6 or IIS7 classic mode,
    //// visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        #region Constants and Fields

        private WebSessionStorage webSessionStorage;

        #endregion

        #region Public Methods

        public override void Init()
        {
            base.Init();

            // The WebSessionStorage must be created during the Init() to tie in HttpApplication events
            this.webSessionStorage = new WebSessionStorage(this);
        }

        #endregion

        #region Methods

        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if ((Request.Path.IndexOf("unt/LogOn")<0)
                && (Request.Path.IndexOf("unt/ValidateUser") < 0)                 
                && (Request.Path.IndexOf("/Content/")<0)
                && (Request.Path.IndexOf("/Doc")<0 )
                && (Request.Path.IndexOf("RunJob") < 0)
                && (Request.Path.IndexOf("Scripts/") < 0)
                && (Request.Path.IndexOf("Storages/LoadInvoice") < 0))
                if (HttpContext.Current.Session[Store.Data.DataGlobals.ACCOUNT_KEY] == null)
                    Response.Redirect("~/LoginAccount/LogOn", true);
        }

        /// <summary>
        /// Due to issues on IIS7, the NHibernate initialization cannot reside in Init() but
        /// must only be called once.  Consequently, we invoke a thread-safe singleton class to
        /// ensure it's only initialized once.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            NHibernateInitializer.Instance().InitializeNHibernateOnce(this.InitializeNHibernateSession);

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Useful for debugging
            Exception ex = this.Server.GetLastError();
            var reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }

        protected void Application_Start()
        {
            //string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log4Net.config");
            //var fileInfo = new FileInfo(configPath);
            //XmlConfigurator.ConfigureAndWatch(fileInfo);
            log4net.Config.XmlConfigurator.Configure();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new AreaViewEngine());
            //ViewEngines.Engines.Add(new WebFormViewEngine());

            ModelBinders.Binders.DefaultBinder = new SharpModelBinder();
            ModelMetadataProviders.Current = new Store.Core.CustomModelMetadataProvider(); 
            ModelValidatorProviders.Providers.Add(new NHibernateValidatorProvider());

            this.InitializeServiceLocator();

            AreaRegistration.RegisterAllAreas();
            RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
            IScheduler _scheduler = null;
            // start up scheduler

            // construct a factory
            ISchedulerFactory factory = new StdSchedulerFactory();
            // get a scheduler
            _scheduler = factory.GetScheduler();
            // start the scheduler
            _scheduler.Start();
        }

        /// <summary>
        /// Instantiate the container and add all Controllers that derive from
        /// WindsorController to the container.  Also associate the Controller
        /// with the WindsorContainer ControllerFactory.
        /// </summary>
        protected virtual void InitializeServiceLocator()
        {
            IWindsorContainer container = new WindsorContainer();
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));
            container.RegisterControllers(typeof(WorkersController).Assembly);
            //container.RegisterControllers(typeof(CallRFC).Assembly);
            ComponentRegistrar.AddComponentsTo(container);
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
        }

        private XmlNodeList PrepareConfiguration(XmlDocument doc, string factoryName)
        {
            // Add Proper Namespace
            XmlNamespaceManager namespaceMgr = new XmlNamespaceManager(doc.NameTable);
            namespaceMgr.AddNamespace("nhibernate", "urn:nhibernate-configuration-2.2" );
            // Query Elements
            XmlNodeList nhibernateNode = doc.SelectNodes("/configuration/" + factoryName + "/session-factory/property", namespaceMgr);
            return nhibernateNode;
        }

        private NHibernate.Cfg.Configuration ConfigureAccountDB(string fileName, string factoryName)
        {
            // Load Configuration XML
            XmlDocument doc = new XmlDocument();
            NHibernate.Cfg.Configuration conf = new NHibernate.Cfg.Configuration();
            doc.Load(fileName);
            XmlNodeList nodes = PrepareConfiguration(doc, factoryName);
            foreach (XmlNode item in nodes) {
                conf.SetProperty(item.Attributes["name"].Value, item.InnerText);
                 
            }
            return conf;
        }

        /// <summary>
        /// If you need to communicate to multiple databases, you'd add a line to this method to
        /// initialize the other database as well.
        /// </summary>
        private void InitializeNHibernateSession()
        {
            NHibernateSession.Init(
                this.webSessionStorage,
                new[] { this.Server.MapPath("~/bin/Store.Data.dll") },
                new AutoPersistenceModelGenerator().Generate());
            NHibernate.Cfg.Configuration cfg = ConfigureAccountDB(Server.MapPath("~/Web.config"), "hibernate-account-configuration");
            NHibernateSession.AddConfiguration(Store.Data.DataGlobals.ACCOUNT_DB_FACTORY_KEY,  new string[] { Server.MapPath("~/bin/Store.Data.Account.dll") },null, cfg, "",null);
            NHibernateSession.RegisterInterceptor(new AuditInterceptor());            
        }

        #endregion
    }
}