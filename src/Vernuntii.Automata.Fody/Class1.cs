using System.Diagnostics;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Vernuntii.Automata.Fody;

public class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        Debugger.Launch();

        var ns = GetNamespace();
        var type = new TypeDefinition(ns, "Hello", TypeAttributes.Public, TypeSystem.ObjectReference);

        AddConstructor(type);

        AddHelloWorld(type);

        ModuleDefinition.Types.Add(type);
        WriteInfo("Added type 'Hello' with method 'World'.");
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }

    string? GetNamespace()
    {
        var namespaceFromConfig = GetNamespaceFromConfig();
        var namespaceFromAttribute = GetNamespaceFromAttribute();
        if (namespaceFromConfig != null && namespaceFromAttribute != null) {
            throw new WeavingException("Configuring namespace from both Config and Attribute is not supported.");
        }

        if (namespaceFromAttribute != null) {
            return namespaceFromAttribute;
        }

        return namespaceFromConfig;
    }

    string? GetNamespaceFromConfig()
    {
        var attribute = Config?.Attribute("Namespace");
        if (attribute == null) {
            return null;
        }

        var value = attribute.Value;
        ValidateNamespace(value);
        return value;
    }

    string? GetNamespaceFromAttribute()
    {
        var attributes = ModuleDefinition.Assembly.CustomAttributes;
        var namespaceAttribute = attributes
            .SingleOrDefault(x => x.AttributeType.FullName == "NamespaceAttribute");
        if (namespaceAttribute == null) {
            return null;
        }

        attributes.Remove(namespaceAttribute);
        var value = (string)namespaceAttribute.ConstructorArguments.First().Value;
        ValidateNamespace(value);
        return value;
    }

    static void ValidateNamespace(string value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value)) {
            throw new WeavingException("Invalid namespace");
        }
    }

    void AddConstructor(TypeDefinition newType)
    {
        var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        var method = new MethodDefinition(".ctor", attributes, TypeSystem.VoidReference);
        var objectConstructor = ModuleDefinition.ImportReference(TypeSystem.ObjectDefinition.GetConstructors().First());
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Call, objectConstructor);
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    void AddHelloWorld(TypeDefinition newType)
    {
        var method = new MethodDefinition("World", MethodAttributes.Public, TypeSystem.StringReference);
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldstr, "Hello World");
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    public override bool ShouldCleanReference => true;
}
