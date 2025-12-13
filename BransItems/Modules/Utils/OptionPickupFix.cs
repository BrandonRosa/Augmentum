using System;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using RoR2;

namespace Augmentum
{
    static class OptionPickupFix
    {
        [SystemInitializer]
        public static void Init()
        {
            IL.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
        }

        static void PickupDisplay_RebuildModel(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before,
                               x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<PickupDisplay>(_ => _.DestroyModel()))))
            {
                Log.Error("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, AccessTools.DeclaredField(typeof(PickupDisplay), nameof(PickupDisplay.dontInstantiatePickupModel)));
            c.EmitSkipMethodCall(OpCodes.Brtrue);
        }

        public static void EmitSkipMethodCall(this ILCursor c, OpCode branchOpCode, MethodDefinition method = null)
        {
            EmitSkipMethodCall(c, branchOpCode, null, method);
        }

        public static void EmitSkipMethodCall(this ILCursor c, OpCode branchOpCode, Action<ILCursor> emitSkippedReturnValue, MethodDefinition method = null)
        {
            if (c is null)
                throw new ArgumentNullException(nameof(c));

            if (branchOpCode.FlowControl != FlowControl.Branch && branchOpCode.FlowControl != FlowControl.Cond_Branch)
                throw new ArgumentException($"Invalid branch OpCode: {branchOpCode}");

            if (method == null)
            {
                if (!c.Next.MatchCallOrCallvirt(out MethodReference nextMethodCall))
                {
                    Log.Error($"Failed to find method call to skip: {c.Context.Method.FullName} at instruction {c.Next} ({c.Index})");
                    return;
                }

                method = nextMethodCall.SafeResolve();

                if (method == null)
                {
                    Log.Error($"Failed to resolve method '{nextMethodCall.FullName}': {c.Context.Method.FullName} at instruction {c.Next} ({c.Index})");
                    return;
                }
            }

            int parameterCount = method.Parameters.Count + (!method.IsStatic ? 1 : 0);
            bool isVoidReturn = method.ReturnType.Is(typeof(void));

            ILLabel skipCallLabel = c.DefineLabel();

            c.Emit(branchOpCode, skipCallLabel);

            c.Index++;

            if (parameterCount > 0 || !isVoidReturn)
            {
                ILLabel afterPatchLabel = c.DefineLabel();
                c.Emit(OpCodes.Br, afterPatchLabel);

                c.MarkLabel(skipCallLabel);

                for (int i = 0; i < parameterCount; i++)
                {
                    c.Emit(OpCodes.Pop);
                }

                if (emitSkippedReturnValue != null)
                {
                    emitSkippedReturnValue(c);
                }
                else if (!isVoidReturn)
                {
                    Log.Warning($"Skipped method ({method.FullName}) is not void, emitting default value: {c.Context.Method.FullName} at instruction {c.Next} ({c.Index})");

                    if (method.ReturnType.IsValueType)
                    {
                        VariableDefinition tmpVar = c.Context.AddVariable(method.ReturnType);

                        c.Emit(OpCodes.Ldloca, tmpVar);
                        c.Emit(OpCodes.Initobj, method.ReturnType);

                        c.Emit(OpCodes.Ldloc, tmpVar);
                    }
                    else
                    {
                        c.Emit(OpCodes.Ldnull);
                    }
                }

                c.MarkLabel(afterPatchLabel);
            }
            else
            {
                c.MarkLabel(skipCallLabel);
            }
        }

        public static VariableDefinition AddVariable(this ILContext context, TypeReference variableType)
        {
            VariableDefinition variableDefinition = new VariableDefinition(variableType);
            context.Method.Body.Variables.Add(variableDefinition);
            return variableDefinition;
        }

    }
}
