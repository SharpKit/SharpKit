using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using System.Collections;
using System.Collections.Concurrent;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler
{
    class AssemblyExt
    {
        public AssemblyExt(IAssembly me)
        {
            Assembly = me;
        }

        public IAssembly Assembly { get; private set; }

        List<ResolvedAttribute> _ResolvedAttributes;
        public List<ResolvedAttribute> ResolvedAttributes
        {
            get
            {
                if (_ResolvedAttributes == null)
                    _ResolvedAttributes = Assembly.AssemblyAttributes.Select(ToResolvedAttribute).ToList();
                return _ResolvedAttributes;
            }
        }
        ResolvedAttribute ToResolvedAttribute(IAttribute att)
        {
            return new ResolvedAttribute(Assembly) { IAttribute = att };
        }

    }

    class EntityExt
    {
        public EntityExt(IEntity me)
        {
            Entity = me;
        }

        public IEntity Entity { get; private set; }
        public Dictionary<Type, object> SingleDeclaredAttributeCache { get; set; }
        List<ResolvedAttribute> _ResolvedAttributes;
        public List<ResolvedAttribute> ResolvedAttributes
        {
            get
            {
                if (_ResolvedAttributes == null)
                    _ResolvedAttributes = Entity.Attributes.Select(ToResolvedAttribute).ToList();
                return _ResolvedAttributes;
            }
        }
        ResolvedAttribute ToResolvedAttribute(IAttribute att)
        {
            return new ResolvedAttribute(Entity) { IAttribute = att };
        }

        List<ResolvedAttribute> _ExternalResolvedAttributes;
        public List<ResolvedAttribute> ExternalResolvedAttributes
        {
            get
            {
                if (_ExternalResolvedAttributes == null)
                    _ExternalResolvedAttributes = new List<ResolvedAttribute>();
                return _ExternalResolvedAttributes;
            }
        }

        List<ResolvedAttribute> _ParentExternalResolvedAttributes;
        public List<ResolvedAttribute> ParentExternalResolvedAttributes
        {
            get
            {
                if (_ParentExternalResolvedAttributes == null)
                {
                    _ParentExternalResolvedAttributes = new List<ResolvedAttribute>();
                    var me2 = Entity as IMember;
                    if (me2 != null && me2.MemberDefinition != me2 && me2.MemberDefinition != null)
                    {
                        var me3 = me2.MemberDefinition;
                        var ext3 = me3.GetExtension(false);
                        if (ext3 != null)
                        {
                            if (ext3.ExternalResolvedAttributes != null)
                                _ParentExternalResolvedAttributes.AddRange(ext3.ExternalResolvedAttributes);
                        }
                    }
                }
                return _ParentExternalResolvedAttributes;
            }
        }

        public IEnumerable<ResolvedAttribute> AllResolvedAttributes
        {
            get
            {
                return ExternalResolvedAttributes.Concat(ParentExternalResolvedAttributes).Concat(ResolvedAttributes);
            }
        }

        public bool? IsJsExported { get; set; }
        public bool? IsRemotable { get; set; }
        public bool? IsGlobalType { get; set; }
        public bool? IsNativeType { get; set; }
    }

    class ResolvedAttribute
    {
        public ResolvedAttribute(IEntity owner)
        {
            EntityOwner = owner;
            if (EntityOwner != null)
                Project = (SkProject)EntityOwner.GetNProject();

        }
        public ResolvedAttribute(IAssembly owner)
        {
            AsmOwner = owner;
            if (AsmOwner != null)
                Project = (SkProject)AsmOwner.GetNProject();

        }
        public IEntity EntityOwner { get; set; }
        public IAssembly AsmOwner { get; set; }
        public IAttribute IAttribute { get; set; }
        public Attribute Attribute { get; set; }

        string IAttributeTypeName;
        public ICSharpCode.NRefactory.CSharp.AstNode GetDeclaration()
        {
            if (IAttribute == null)
                return null;
            return IAttribute.GetDeclaration();
        }

        Type MatchedType;
        HashSet<Type> UnmatchedTypes;
        public bool MatchesType<T>()
        {
            var type = typeof(T);
            if (MatchedType != null)
                return type == MatchedType;
            if (UnmatchedTypes != null && UnmatchedTypes.Contains(type))
                return false;
            var x = IsMatch<T>();
            if (x)
            {
                MatchedType = type;
            }
            else
            {
                if (UnmatchedTypes == null)
                    UnmatchedTypes = new HashSet<Type>();
                UnmatchedTypes.Add(type);
            }
            return x;
        }

        private bool IsMatch<T>()
        {
            if (Attribute != null && Attribute is T)
            {
                return true;
            }
            if (IAttribute != null)
            {
                if (IAttributeTypeName == null)
                    IAttributeTypeName = IAttribute.AttributeType.FullName;
                var name2 = typeof(T).FullName;
                if (name2.StartsWith(Sk.MirrorTypePrefix, StringComparison.InvariantCultureIgnoreCase))
                    name2 = name2.Substring(Sk.MirrorTypePrefix.Length);
                if (IAttributeTypeName == name2)
                {
                    return true;
                }
            }
            return false;
        }
        public T ConvertToCustomAttribute<T>() where T : Attribute
        {
            if (Attribute == null && IAttribute != null)
                Attribute = IAttribute.ConvertToCustomAttribute<T>(Project);
            return Attribute as T;
        }

        SkProject Project;
    }

    static class EntityExtProvider
    {
        public static IEnumerable<ResolvedAttribute> GetAllResolvedAttributes(this IEntity ent)
        {
            return ent.GetExtension(true).AllResolvedAttributes;
        }

        public static AssemblyExt GetExtension(this IAssembly ent, bool create)
        {
            var ext = (AssemblyExt)ent.Tag;
            if (ext == null && create)
            {
                ext = new AssemblyExt(ent);
                ent.Tag = ext;
            }
            return ext;
        }

        public static EntityExt GetExtension(this IEntity ent, bool create)
        {
            var ext = (EntityExt)ent.Tag;
            if (ext == null && create)
            {
                ext = new EntityExt(ent);
                ent.Tag = ext;
            }
            return ext;
        }
    }


}
