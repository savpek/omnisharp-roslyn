using System;
using System.Composition;
using System.Dynamic;
using System.Reflection;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace OmniSharp
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportWorkspaceServiceWithAssemblyQualifiedNameAttribute : ExportAttribute
    {
        public string ServiceType { get; }
        public string Layer { get; }

        public ExportWorkspaceServiceWithAssemblyQualifiedNameAttribute(string typeAssembly, string typeName, string layer = ServiceLayer.Host)
            : base(typeof(IWorkspaceService))
        {
            var type = Assembly.Load(typeAssembly).GetType(typeName)
                ?? throw new InvalidOperationException($"Could not resolve '{typeName} from '{typeAssembly}'");

            Console.WriteLine($"Resolved to type: {type.AssemblyQualifiedName}");
            this.ServiceType = type.AssemblyQualifiedName;
            this.Layer = layer ?? throw new ArgumentNullException(nameof(layer));
        }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportWorkspaceServiceWithAssemblyQualifiedName2Attribute : ExportWorkspaceServiceAttribute
    {
        public new string ServiceType { get; }
        public new string Layer { get; }

        public ExportWorkspaceServiceWithAssemblyQualifiedName2Attribute(string typeAssembly, string typeName, string layer = ServiceLayer.Host)
            : base(typeof(IWorkspaceService))
        {
            var type = Assembly.Load(typeAssembly).GetType(typeName)
                ?? throw new InvalidOperationException($"Could not resolve '{typeName} from '{typeAssembly}'");

            Console.WriteLine($"Resolved to type: {type.AssemblyQualifiedName}");
            this.ServiceType = type.AssemblyQualifiedName;
            this.Layer = layer ?? throw new ArgumentNullException(nameof(layer));
        }
    }

    public interface ITestWorkspaceService: IWorkspaceService
    {
    }

    [ExportWorkspaceService(typeof(ITestWorkspaceService), ServiceLayer.Host), Shared]
    public class TestWorkspaceService: ITestWorkspaceService
    {
    }

    [Shared]
    [ExportWorkspaceServiceWithAssemblyQualifiedName("Microsoft.CodeAnalysis.Features", "Microsoft.CodeAnalysis.PickMembers.IPickMembersService")]
    public class FooBarJeeJee : DynamicObject, IWorkspaceService
    {
        public FooBarJeeJee()
        {
            Console.WriteLine("MITÄSVITTUATAAS");
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            return !binder.Type.IsValueType;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Console.WriteLine("ASDFASDFASDF");
            throw new InvalidOperationException("VOESAATANA");
        }
    }
}
