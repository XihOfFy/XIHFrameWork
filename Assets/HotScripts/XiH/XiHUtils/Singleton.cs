namespace XiHUtil
{
    public class Singleton<T> where T : class, new()
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }

        protected Singleton() { }
        public static void SetInstance(T t) => instance = t;
    }
}
