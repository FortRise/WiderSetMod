using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace EightPlayerMod 
{
    public abstract class AbstractILType 
    {
        public Type Type;
        public bool IsPrivate;
        protected IDetour BaseHook;

        public abstract void Load(ILContext.Manipulator manipulator);
        public void Unload() 
        {
            BaseHook.Dispose();
        }
    }

    public class MethodILType : AbstractILType
    {
        public string MethodName;

        public MethodILType(Type type, string name) 
        {
            Type = type;
            MethodName = name;
        }

        public MethodILType(Type type, string name, bool isPrivate) 
        {
            Type = type;
            MethodName = name;
            IsPrivate = isPrivate;
        }

        public override void Load(ILContext.Manipulator manipulator) 
        {
            if (IsPrivate) 
            {
                this.BaseHook = new ILHook(
                    Type.GetMethod(MethodName, BindingFlags.Instance | BindingFlags.NonPublic),
                    manipulator
                );
                return;
            }
            this.BaseHook = new ILHook(
                Type.GetMethod(MethodName),
                manipulator
            );
        }
    }
}