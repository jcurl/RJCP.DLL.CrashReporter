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
            m_AppDomain = appDomain;
        }

        public bool SetFirstChanceException(EventHandler<FirstChanceExceptionEventArgs> handler)
        {
            EventInfo fce = m_AppDomain.GetType()
                .GetEvent("FirstChanceException", BindingFlags.Instance | BindingFlags.Public);
            if (fce == null) return false;

            Type delType = fce.EventHandlerType;
            Delegate del = Delegate.CreateDelegate(delType, handler.Target, handler.Method);

            // Get AppDomain.FirstChanceException add method
            MethodInfo addHandler = fce.GetAddMethod();
            object[] addHandlerArgs = { del };
            addHandler.Invoke(m_AppDomain, addHandlerArgs);
            return true;
        }
    }
}
