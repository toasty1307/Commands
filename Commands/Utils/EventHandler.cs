using System.Threading.Tasks;

namespace Commands.Utils
{
    public delegate void EventHandler<in T1>(T1 t1);
    public delegate void EventHandler<in T1, in T2>(T1 t1, T2 t2);
    public delegate void EventHandler<in T1, in T2, in T3>(T1 t1, T2 t2, T3 t3);
    public delegate void EventHandler<in T1, in T2, in T3, in T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate void EventHandler<in T1, in T2, in T3, in T4, in T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    public delegate void EventHandler<in T1, in T2, in T3, in T4, in T5, in T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate void EventHandler<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    public delegate void EventHandler<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
    public delegate void EventHandler<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);

    public delegate Task AsyncEventHandler<in T1>(T1 t1);
    public delegate Task AsyncEventHandler<in T1, in T2>(T1 t1, T2 t2);
    public delegate Task AsyncEventHandler<in T1, in T2, in T3>(T1 t1, T2 t2, T3 t3);
    public delegate Task AsyncEventHandler<in T1, in T2, in T3, in T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate Task AsyncEventHandler<in T1, in T2, in T3, in T4, in T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    public delegate Task AsyncEventHandler<in T1, in T2, in T3, in T4, in T5, in T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate Task AsyncEventHandler<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    public delegate Task AsyncEventHandler<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
    public delegate Task AsyncEventHandler<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);

    
    public static class EventHandlerExtensions
    {
        public static void SafeInvoke<T1>(this EventHandler<T1> handler, T1 t1) => handler?.Invoke(t1);
        public static void SafeInvoke<T1, T2>(this EventHandler<T1, T2> handler, T1 t1, T2 t2) => handler?.Invoke(t1, t2);
        public static void SafeInvoke<T1, T2, T3>(this EventHandler<T1, T2, T3> handler, T1 t1, T2 t2, T3 t3) => handler?.Invoke(t1, t2, t3);
        public static void SafeInvoke<T1, T2, T3, T4>(this EventHandler<T1, T2, T3, T4> handler, T1 t1, T2 t2, T3 t3, T4 t4) => handler?.Invoke(t1, t2, t3, t4);
        public static void SafeInvoke<T1, T2, T3, T4, T5>(this EventHandler<T1, T2, T3, T4, T5> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => handler?.Invoke(t1, t2, t3, t4, t5);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6>(this EventHandler<T1, T2, T3, T4, T5, T6> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => handler?.Invoke(t1, t2, t3, t4, t5, t6);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7>(this EventHandler<T1, T2, T3, T4, T5, T6, T7> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => handler?.Invoke(t1, t2, t3, t4, t5, t6, t7);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this EventHandler<T1, T2, T3, T4, T5, T6, T7, T8> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => handler.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EventHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) => handler?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        
        public static void SafeInvoke<T1>(this AsyncEventHandler<T1> handler, T1 t1) => handler?.Invoke(t1);
        public static void SafeInvoke<T1, T2>(this AsyncEventHandler<T1, T2> handler, T1 t1, T2 t2) => handler?.Invoke(t1, t2);
        public static void SafeInvoke<T1, T2, T3>(this AsyncEventHandler<T1, T2, T3> handler, T1 t1, T2 t2, T3 t3) => handler?.Invoke(t1, t2, t3);
        public static void SafeInvoke<T1, T2, T3, T4>(this AsyncEventHandler<T1, T2, T3, T4> handler, T1 t1, T2 t2, T3 t3, T4 t4) => handler?.Invoke(t1, t2, t3, t4);
        public static void SafeInvoke<T1, T2, T3, T4, T5>(this AsyncEventHandler<T1, T2, T3, T4, T5> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => handler?.Invoke(t1, t2, t3, t4, t5);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6>(this AsyncEventHandler<T1, T2, T3, T4, T5, T6> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => handler?.Invoke(t1, t2, t3, t4, t5, t6);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7>(this AsyncEventHandler<T1, T2, T3, T4, T5, T6, T7> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => handler?.Invoke(t1, t2, t3, t4, t5, t6, t7);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this AsyncEventHandler<T1, T2, T3, T4, T5, T6, T7, T8> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => handler.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this AsyncEventHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) => handler?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    }
}