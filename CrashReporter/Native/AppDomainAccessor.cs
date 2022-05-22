namespace RJCP.Diagnostics.Native
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    internal class AppDomainAccessor
    {
        private readonly AppDomain m_AppDomain;
        private readonly EventInfo m_FirstChanceException;

        public AppDomainAccessor(AppDomain appDomain)
        {
            m_AppDomain = appDomain;
            m_FirstChanceException = m_AppDomain.GetType()
                .GetEvent("FirstChanceException", BindingFlags.Instance | BindingFlags.Public);
        }

        public bool IsSupported { get { return m_FirstChanceException != null; } }

        public bool SetFirstChanceException(EventHandler<FirstChanceExceptionEventArgs> handler)
        {
            if (!IsSupported) return false;

            Type delType = m_FirstChanceException.EventHandlerType;
            Delegate del = Delegate.CreateDelegate(delType, handler.Target, handler.Method);

            // Get AppDomain.FirstChanceException add method
            MethodInfo addHandler = m_FirstChanceException.GetAddMethod();
            object[] addHandlerArgs = { del };
            addHandler.Invoke(m_AppDomain, addHandlerArgs);
            return true;
        }
    }
}
