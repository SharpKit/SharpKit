using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace SharpKit.Compiler
{
    class CustomAttributeProvider : ICustomAttributeProvider
    {
        #region ICSharpModelProvider Members

        public void AddCustomAttribute(IEntity me, Attribute att)
        {
            me.GetExtension(true).ResolvedAttributes.Add(new ResolvedAttribute(me) { Attribute = att });
        }

        public T GetCustomAttribute<T>(IEntity me) where T : Attribute
        {
            return me.GetMetadata<T>();
        }

        public IEnumerable<T> GetCustomAttributes<T>(IEntity me) where T : Attribute
        {
            return me.GetMetadatas<T>();
        }

        public void AddCustomAttribute(IAssembly me, Attribute att)
        {
            me.GetExtension(true).ResolvedAttributes.Add(new ResolvedAttribute(me) { Attribute = att });
        }

        public T GetCustomAttribute<T>(IAssembly me) where T : Attribute
        {
            return me.GetMetadata<T>();
        }

        public IEnumerable<T> GetCustomAttributes<T>(IAssembly me) where T : Attribute
        {
            return me.GetMetadatas<T>();
        }

        #endregion
    }
    public interface ICustomAttributeProvider
    {
        void AddCustomAttribute(IEntity me, Attribute att);
        T GetCustomAttribute<T>(IEntity me) where T : Attribute;
        IEnumerable<T> GetCustomAttributes<T>(IEntity me) where T : Attribute;

        void AddCustomAttribute(IAssembly me, Attribute att);
        T GetCustomAttribute<T>(IAssembly me) where T : Attribute;
        IEnumerable<T> GetCustomAttributes<T>(IAssembly me) where T : Attribute;

    }

}
