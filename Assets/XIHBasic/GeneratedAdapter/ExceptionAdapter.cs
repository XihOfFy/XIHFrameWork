using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace XIHBasic
{   
    public class ExceptionAdapter : CrossBindingAdaptor
    {
        static CrossBindingFunctionInfo<System.String> mget_Message_0 = new CrossBindingFunctionInfo<System.String>("get_Message");
        static CrossBindingFunctionInfo<System.Collections.IDictionary> mget_Data_1 = new CrossBindingFunctionInfo<System.Collections.IDictionary>("get_Data");
        static CrossBindingFunctionInfo<System.Exception> mGetBaseException_2 = new CrossBindingFunctionInfo<System.Exception>("GetBaseException");
        static CrossBindingFunctionInfo<System.String> mget_StackTrace_3 = new CrossBindingFunctionInfo<System.String>("get_StackTrace");
        static CrossBindingFunctionInfo<System.String> mget_HelpLink_4 = new CrossBindingFunctionInfo<System.String>("get_HelpLink");
        static CrossBindingMethodInfo<System.String> mset_HelpLink_5 = new CrossBindingMethodInfo<System.String>("set_HelpLink");
        static CrossBindingFunctionInfo<System.String> mget_Source_6 = new CrossBindingFunctionInfo<System.String>("get_Source");
        static CrossBindingMethodInfo<System.String> mset_Source_7 = new CrossBindingMethodInfo<System.String>("set_Source");
        static CrossBindingMethodInfo<System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext> mGetObjectData_8 = new CrossBindingMethodInfo<System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext>("GetObjectData");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(System.Exception);
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

        public class Adapter : System.Exception, CrossBindingAdaptorType
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

            public override System.Exception GetBaseException()
            {
                if (mGetBaseException_2.CheckShouldInvokeBase(this.instance))
                    return base.GetBaseException();
                else
                    return mGetBaseException_2.Invoke(this.instance);
            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {
                if (mGetObjectData_8.CheckShouldInvokeBase(this.instance))
                    base.GetObjectData(info, context);
                else
                    mGetObjectData_8.Invoke(this.instance, info, context);
            }

            public override System.String Message
            {
            get
            {
                if (mget_Message_0.CheckShouldInvokeBase(this.instance))
                    return base.Message;
                else
                    return mget_Message_0.Invoke(this.instance);

            }
            }

            public override System.Collections.IDictionary Data
            {
            get
            {
                if (mget_Data_1.CheckShouldInvokeBase(this.instance))
                    return base.Data;
                else
                    return mget_Data_1.Invoke(this.instance);

            }
            }

            public override System.String StackTrace
            {
            get
            {
                if (mget_StackTrace_3.CheckShouldInvokeBase(this.instance))
                    return base.StackTrace;
                else
                    return mget_StackTrace_3.Invoke(this.instance);

            }
            }

            public override System.String HelpLink
            {
            get
            {
                if (mget_HelpLink_4.CheckShouldInvokeBase(this.instance))
                    return base.HelpLink;
                else
                    return mget_HelpLink_4.Invoke(this.instance);

            }
            set
            {
                if (mset_HelpLink_5.CheckShouldInvokeBase(this.instance))
                    base.HelpLink = value;
                else
                    mset_HelpLink_5.Invoke(this.instance, value);

            }
            }

            public override System.String Source
            {
            get
            {
                if (mget_Source_6.CheckShouldInvokeBase(this.instance))
                    return base.Source;
                else
                    return mget_Source_6.Invoke(this.instance);

            }
            set
            {
                if (mset_Source_7.CheckShouldInvokeBase(this.instance))
                    base.Source = value;
                else
                    mset_Source_7.Invoke(this.instance, value);

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

