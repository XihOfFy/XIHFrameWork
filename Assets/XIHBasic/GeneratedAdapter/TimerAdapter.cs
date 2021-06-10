using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace XIHBasic
{   
    public class TimerAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo<System.ComponentModel.ISite> mset_Site_0 = new CrossBindingMethodInfo<System.ComponentModel.ISite>("set_Site");
        static CrossBindingFunctionInfo<System.ComponentModel.ISite> mget_Site_1 = new CrossBindingFunctionInfo<System.ComponentModel.ISite>("get_Site");
        static CrossBindingMethodInfo<System.Boolean> mDispose_2 = new CrossBindingMethodInfo<System.Boolean>("Dispose");
        static CrossBindingFunctionInfo<System.Boolean> mget_CanRaiseEvents_3 = new CrossBindingFunctionInfo<System.Boolean>("get_CanRaiseEvents");
        static CrossBindingFunctionInfo<System.Type, System.Object> mGetService_4 = new CrossBindingFunctionInfo<System.Type, System.Object>("GetService");
        static CrossBindingFunctionInfo<System.Type, System.Runtime.Remoting.ObjRef> mCreateObjRef_5 = new CrossBindingFunctionInfo<System.Type, System.Runtime.Remoting.ObjRef>("CreateObjRef");
        static CrossBindingFunctionInfo<System.Object> mInitializeLifetimeService_6 = new CrossBindingFunctionInfo<System.Object>("InitializeLifetimeService");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(System.Timers.Timer);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : System.Timers.Timer, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            protected override void Dispose(System.Boolean disposing)
            {
                if (mDispose_2.CheckShouldInvokeBase(this.instance))
                    base.Dispose(disposing);
                else
                    mDispose_2.Invoke(this.instance, disposing);
            }

            protected override System.Object GetService(System.Type service)
            {
                if (mGetService_4.CheckShouldInvokeBase(this.instance))
                    return base.GetService(service);
                else
                    return mGetService_4.Invoke(this.instance, service);
            }

            public override System.Runtime.Remoting.ObjRef CreateObjRef(System.Type requestedType)
            {
                if (mCreateObjRef_5.CheckShouldInvokeBase(this.instance))
                    return base.CreateObjRef(requestedType);
                else
                    return mCreateObjRef_5.Invoke(this.instance, requestedType);
            }

            public override System.Object InitializeLifetimeService()
            {
                if (mInitializeLifetimeService_6.CheckShouldInvokeBase(this.instance))
                    return base.InitializeLifetimeService();
                else
                    return mInitializeLifetimeService_6.Invoke(this.instance);
            }

            public override System.ComponentModel.ISite Site
            {
            get
            {
                if (mget_Site_1.CheckShouldInvokeBase(this.instance))
                    return base.Site;
                else
                    return mget_Site_1.Invoke(this.instance);

            }
            set
            {
                if (mset_Site_0.CheckShouldInvokeBase(this.instance))
                    base.Site = value;
                else
                    mset_Site_0.Invoke(this.instance, value);

            }
            }

            protected override System.Boolean CanRaiseEvents
            {
            get
            {
                if (mget_CanRaiseEvents_3.CheckShouldInvokeBase(this.instance))
                    return base.CanRaiseEvents;
                else
                    return mget_CanRaiseEvents_3.Invoke(this.instance);

            }
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

