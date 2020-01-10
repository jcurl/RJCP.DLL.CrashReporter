namespace RJCP.Diagnostics.Native
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    internal class AppDomainAccessor
    {
        private readonly AppDomain m_AppDomain;

        public AppDomainAccessor(AppDomain appDomain)
        {
            if (appDomain == null) throw new ArgumentNullException(nameof(appDomain));
            m_AppDomain = appDomain;
        }

        public bool SetFirstChanceException(EventHandler<FirstChanceExceptionEventArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            EventInfo fce = m_AppDomain.GetType()
                .GetEvent(nameof(FirstChanceException), BindingFlags.Instance | BindingFlags.Public);
            if (fce == null) return false;

            Type delType = fce.EventHandlerType;
            Delegate del = Delegate.CreateDelegate(delType, handler.Target, handler.Method);

            // Get AppDomain.FirstChanceException add method
            MethodInfo addHandler = fce.GetAddMethod();
            object[] addHandlerArgs = { del };
            addHandler.Invoke(m_AppDomain, addHandlerArgs);
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S3237:'value' parameters should be used", Justification = "Feature not yet supported")]
        public event EventHandler<FirstChanceExceptionEventArgs> FirstChanceException
        {
            add { SetFirstChanceException(value); }
            remove { /* We don't support removing methods. */ }
        }
    }
}
