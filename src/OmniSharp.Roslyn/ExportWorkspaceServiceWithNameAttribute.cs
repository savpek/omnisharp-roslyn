using System;
using System.Composition;
using System.Dynamic;
using System.Reflection;
using Castle.DynamicProxy;
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
    public class ExportWorkspaceServiceFactoryWithAssemblyQualifiedNameAttribute : ExportAttribute
    {
        public string ServiceType { get; }
        public string Layer { get; }

        public ExportWorkspaceServiceFactoryWithAssemblyQualifiedNameAttribute(string typeAssembly, string typeName, string layer = ServiceLayer.Host)
            : base(typeof(IWorkspaceServiceFactory))
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

    public class SomeInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var resultTypeInternal = Assembly.Load("Microsoft.CodeAnalysis.Features").GetType("Microsoft.CodeAnalysis.PickMembers.PickMembersResult");
            var resultInstance = Activator.CreateInstance(resultTypeInternal, new object[] { invocation.Arguments[1], invocation.Arguments[2] });
            invocation.ReturnValue = resultInstance;
        }
    }

    [Shared]
    [ExportWorkspaceServiceFactoryWithAssemblyQualifiedName("Microsoft.CodeAnalysis.Features", "Microsoft.CodeAnalysis.PickMembers.IPickMembersService")]
    public class FooBarJeeJee : IWorkspaceServiceFactory
    {
        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            ProxyGenerator generator = new ProxyGenerator();
            var internalType = Assembly.Load("Microsoft.CodeAnalysis.Features").GetType("Microsoft.CodeAnalysis.PickMembers.IPickMembersService");
            // var vsDummyType = Assembly.Load("Microsoft.VisualStudio.LanguageServices").GetType("Microsoft.VisualStudio.LanguageServices.Implementation.PickMembers.VisualStudioPickMembersService");
            // var dummyService = Activator.CreateInstance(vsDummyType, new object[] { null });
            return (IWorkspaceService)generator.CreateInterfaceProxyWithoutTarget(internalType, new[] { typeof(IWorkspaceService)}, new SomeInterceptor());
        }
    }

    // [Shared]
    // [ExportWorkspaceServiceWithAssemblyQualifiedName("Microsoft.CodeAnalysis.Features", "Microsoft.CodeAnalysis.PickMembers.IPickMembersService")]
    // public class FooBarJeeJee : IWorkspaceService
    // {

    // }
}
