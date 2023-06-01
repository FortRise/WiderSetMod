using System;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace EightPlayerMod 
{
    public class ConstructorILType<T1> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3, T4> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3, T4, T5> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3, T4, T5, T6> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3, T4, T5, T6, T7> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3, T4, T5, T6, T7, T8> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3, T4, T5, T6, T7, T8, T9> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }),
                manipulator
            );
        }
    }
    public class ConstructorILType<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : AbstractILType
    {
        public ConstructorILType(Type type) 
        {
            Type = type;
        }

        public ConstructorILType(Type type, bool isPrivate) 
        {
            Type = type;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator)
        {
            BaseHook = new ILHook(
                Type.GetConstructor(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10) }),
                manipulator
            );
        }
    }
}
