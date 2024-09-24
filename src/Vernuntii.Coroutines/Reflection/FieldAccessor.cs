using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Vernuntii.Coroutines.Reflection;

delegate ref TField GetValueDelegate<T, TField>(ref T stateMachine);

internal class FieldAccessor<T, TField>
{
    private readonly Type _fieldOwnerType;
    private readonly Type _fieldType;
    private readonly FieldInfo _fieldInfo;
    private readonly GetValueDelegate<T, TField>? _getValueReferenceDelegate;

    internal FieldAccessor(Type fieldOwnerType, Type fieldType, FieldInfo fieldInfo)
    {
        _fieldOwnerType = fieldOwnerType;
        _fieldType = fieldType;
        _fieldInfo = fieldInfo;
        if (RuntimeFeature.IsDynamicCodeSupported) {
            _getValueReferenceDelegate = CompileGetValueDelegate(_fieldInfo);
        }
    }

    internal GetValueDelegate<T, TField> GetValueReference {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _getValueReferenceDelegate ?? throw new InvalidOperationException("Runtime does not support emitting dynamic code");
    }

    private GetValueDelegate<T, TField> CompileGetValueDelegate(FieldInfo coroutineAwaiterFieldInfo)
    {
        var method = new DynamicMethod(
            nameof(GetValueReference),
            returnType: _fieldType.MakeByRefType(),
            parameterTypes: [_fieldOwnerType.MakeByRefType()],
            restrictedSkipVisibility: true);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load argument (stateMachine) onto the stack
        if (!_fieldOwnerType.IsValueType) {
            il.Emit(OpCodes.Ldind_Ref); // Indicate that the argument is a reference type
        }
        il.Emit(OpCodes.Ldflda, coroutineAwaiterFieldInfo); // Load address of the field into the stack
        il.Emit(OpCodes.Ret); // Return the address of the field (by reference)
        return method.CreateDelegate<GetValueDelegate<T, TField>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="methodBuilderFieldInfo"></param>
    /// <returns>The coroutine builder. Use <see cref="Unsafe.Unbox{T}(object)"/> to avoid unboxing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal object GetValue(TypedReference valueOwnerRef)
    {
        Debug.Assert(_fieldInfo is not null);
        return _fieldInfo.GetValueDirect(valueOwnerRef)!;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="methodBuilderFieldInfo"></param>
    /// <param name="value">The boxed version of a coroutine builder</param>
    internal void SetValue(TypedReference valueOwnerRef, object value)
    {
        Debug.Assert(_fieldInfo is not null);
        _fieldInfo.SetValueDirect(valueOwnerRef, value);
    }

    internal static class Exceptions
    {
        internal static InvalidOperationException FieldArityIsNotOne() => throw new NotImplementedException($"The instance of type {typeof(T)} either does not contain a field of type {typeof(TField)} or contains multiple fields of this type.");
    }
}
